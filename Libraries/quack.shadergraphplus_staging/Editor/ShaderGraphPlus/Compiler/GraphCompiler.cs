using Editor;
using Microsoft.CodeAnalysis;
using Sandbox;
using ShaderGraphPlus.Diagnostics;
using ShaderGraphPlus.Nodes;
using System.Runtime.CompilerServices;
using System.Text;

namespace ShaderGraphPlus;

public sealed partial class GraphCompiler
{
	public struct GraphIssue
	{
		public BaseNodePlus Node;
		public string Message;
		public bool IsWarning;
	}

	internal static Dictionary<Type, (string type, bool isEditorType)> ValueTypes => new()
	{
		{ typeof( int ), ( "int", false ) },
		{ typeof( bool ), ( "bool", false ) },
		{ typeof( float ), ( "float", false ) },
		{ typeof( Vector2 ),( "float2", false ) },
		{ typeof( Vector3 ),( "float3", false ) },
		{ typeof( Vector4 ),( "float4", false ) },
		{ typeof( Color ), ( "float4", false ) },
		{ typeof( Float2x2 ), ( "float2x2", true ) },
		{ typeof( Float3x3 ), ( "float3x3", true ) },
		{ typeof( Float4x4 ), ( "float4x4", true ) },
		{ typeof( Texture2DObject ), ( "Texture2D", true ) },
		{ typeof( TextureCubeObject ), ( "TextureCube", true ) },
		{ typeof( Sampler ), ( "SamplerState", true ) },
	};

	internal static HashSet<Type> ValueTypesNoDefault => new()
	{
		{ typeof( Sampler ) },
		{ typeof( Texture2DObject ) },
		{ typeof( TextureCubeObject ) },
	};

	public bool Debug { get; private set; } = false;

	/// <summary>
	/// Current graph we're compiling
	/// </summary>
	public ShaderGraphPlus Graph { get; private set; }

	/// <summary>
	/// Current SubGraph
	/// </summary>
	private ShaderGraphPlus Subgraph = null;
	private SubgraphNode SubgraphNode = null;
	private List<(SubgraphNode, ShaderGraphPlus)> SubgraphStack = new();

	public Dictionary<string, string> CompiledTextures { get; private set; } = new();

	/// <summary>
	/// The loaded sub-graphs
	/// </summary>
	public List<ShaderGraphPlus> Subgraphs { get; private set; }
	public HashSet<string> PixelIncludes { get; private set; } = new();
	public HashSet<string> VertexIncludes { get; private set; } = new();
	public Dictionary<string, string> VertexInputs { get; private set; } = new();
	public Dictionary<string, string> PixelInputs { get; private set; } = new();
	public int VoidLocalCount { get; set; } = 0;
	public Dictionary<string, string> SyncIDs = new();

	public Asset _Asset { get; private set; }

	/// <summary>
	/// Is this compile for just the preview or not, preview uses attributes for constant values
	/// </summary>
	public bool IsPreview { get; private set; }
	public bool IsNotPreview => !IsPreview;

	private partial class CompileResult
	{
		public List<(NodeResult localResult, NodeResult funcResult)> Results = new();
		public Dictionary<NodeInput, NodeResult> InputResults = new();

		public Dictionary<string, Sampler> SamplerStates = new();
		public Dictionary<string, TextureInput> TextureInputs = new();
		public Dictionary<string, Gradient> Gradients = new();
		public Dictionary<string, (string Options, NodeResult Result)> Parameters = new();
		public Dictionary<string, object> Attributes { get; private set; } = new();
		public HashSet<string> Functions { get; private set; } = new();
		public Dictionary<string, string> Globals { get; private set; } = new();
		public Dictionary<string, VoidData> VoidLocals { get; private set; } = new();
		public List<VoidData> CustomCodeResults { get; private set; } = new();
		internal List<int> CustomFunctionHashCodes { get; private set; } = new();

		//public int VoidLocalCount { get; set; } = 0;
		public string RepresentativeTexture { get; set; }

		public void SetAttributes( Dictionary<string, object> attributes )
		{
			Attributes = attributes;
		}
	}

	public enum ShaderStage
	{
		Vertex,
		Pixel,
	}

	public ShaderStage Stage { get; private set; }
	public bool IsVs => Stage == ShaderStage.Vertex;
	public bool IsPs => Stage == ShaderStage.Pixel;
	private string StageName => Stage == ShaderStage.Vertex ? "vs" : "ps";
	private string CurrentResultInput;

	private CompileResult VertexResult { get; set; } = new();
	private CompileResult PixelResult { get; set; } = new();
	private CompileResult ShaderResult => Stage == ShaderStage.Vertex ? VertexResult : PixelResult;

	public Action<string, object> OnAttribute { get; set; }

	// Init to 1, 0 is reserved.
	public int PreviewID { get; internal set; } = 1;

	private List<NodeInput> InputStack = new();

	private readonly Dictionary<BaseNodePlus, List<string>> NodeErrors = new();
	private readonly Dictionary<BaseNodePlus, List<string>> NodeWarnings = new();

	/// <summary>
	/// Error list.
	/// </summary>
	public IEnumerable<GraphIssue> Errors => NodeErrors
		.Select( x => new GraphIssue { Node = x.Key, Message = x.Value.FirstOrDefault(), IsWarning = false } );

	/// <summary>
	/// Warning list.
	/// </summary>
	public IEnumerable<GraphIssue> Warnings => NodeWarnings
		.Select( x => new GraphIssue { Node = x.Key, Message = x.Value.FirstOrDefault(), IsWarning = true } );

	public GraphCompiler( Asset asset, ShaderGraphPlus graph, Dictionary<string, ShaderFeatureBase> shaderFeatures, bool preview )
	{
		Graph = graph;
		_Asset = asset;
		IsPreview = preview;
		Stage = ShaderStage.Pixel;
		Subgraphs = new();
		AddSubgraphs( Graph );
		ShaderFeatures = shaderFeatures;

		// Set the Initial Vertex and Pixel stage inputs from ShaderTemplate.
		VertexInputs = ShaderTemplate.VertexInputs;
		PixelInputs = ShaderTemplate.PixelInputs;
	}

	public void SetShaderAttribute<T>( string name, T value )
	{
		if ( value is Float2x2 || value is Float3x3 || value is Float4x4 )
			return;

		if ( !ShaderResult.Attributes.ContainsKey( name ) )
		{
			OnAttribute?.Invoke( name, value );
			ShaderResult.Attributes[name] = value;
		}
		else
		{
			//SGPLog.Warning( $"ShaderResult.Attributes already contains key \"{name}\"" );
		}
	}

	internal KeyValuePair<string, TextureInput> GetExistingTextureInputEntry( string key )
	{
		return ShaderResult.TextureInputs.Where( x => x.Key == key ).FirstOrDefault();
	}

	public static string CleanName( string name )
	{
		if ( string.IsNullOrWhiteSpace( name ) )
			return "";

		name = name.Trim();
		name = new string( name.Where( x => char.IsLetter( x ) || char.IsNumber( x ) || x == '_' ).ToArray() );

		return name;
	}

	internal void PostProcessVoidFunctionResult( VoidFunctionBase node, string funcName = "" )
	{
		if ( ShaderResult.VoidLocals.TryGetValue( node.Identifier, out VoidData data ) )
		{
			if ( data.AlreadyPostProcessed )
				return;

			var functionCall = data.FunctionCall;

			foreach ( var targetProperty in data.TargetProperties )
			{
				var property = node.GetType().GetProperty( targetProperty.Key.targetProperty );

				if ( property == null && property.PropertyType != typeof( NodeInput ) )
				{
					SGPLog.Error( $"\"{property.PropertyType}\" is not of type \"{typeof( NodeInput )}\"" );
					continue;
				}

				var inputAttribute = property.GetCustomAttribute<BaseNodePlus.InputAttribute>();
				var titleAttribute = property.GetCustomAttribute<TitleAttribute>();

				var title = "";
				if ( titleAttribute is not null )
				{
					title = titleAttribute.Value;
				}

				//SGPLog.Info( $"targetProperty `{targetProperty.Key.targetProperty}`", IsPreview );

				var nodeInput = (NodeInput)property.GetValue( node, null );
				var funcResult = new NodeResult();
				object defaultValue = EditorTypeLibrary.Create( inputAttribute.Type.Name, inputAttribute.Type );

				PropertyInfo defaultPropertyInfo = node.GetType().GetProperty( targetProperty.Value );

				// Check if we have a valid Default Value property name reference.
				if ( !string.IsNullOrWhiteSpace( targetProperty.Value ) )
				{
					if ( defaultPropertyInfo != null )
					{
						defaultValue = defaultPropertyInfo.GetValue( node );

						if ( defaultValue.GetType() != inputAttribute.Type )
						{
							Exeptions.SGPExeption( $"Default value \"{targetProperty.Value}\" does not match type of \"{title}\" node input!" );
						}
					}
					else
					{
						Exeptions.SGPExeption( $"Could not find property with the name \"{targetProperty.Value}\"" );
					}
				}

				if ( !ValueTypesNoDefault.Contains( inputAttribute.Type ) )
				{
					funcResult = ResultOrDefault( nodeInput, defaultValue );
				}
				else
				{
					funcResult = Result( nodeInput );
				}

				if ( !funcResult.IsValid )
				{
					if ( node is BaseNodePlus baseNode )
					{
						baseNode.HasError = true;
					}

					NodeErrors.Add( node, [$"Missing required input \"{title}\"."] );
				}
				else
				{
					if ( node is BaseNodePlus baseNode )
					{
						baseNode.HasError = false;
					}
				}

				functionCall = functionCall.Replace( targetProperty.Key.placeholderName, funcResult.Code );
			}

			ShaderResult.VoidLocals[node.Identifier] = data with { FunctionCall = functionCall, AlreadyPostProcessed = true };
		}
	}

	private void AddSubgraphs( ShaderGraphPlus graph )
	{
		if ( graph != Graph )
		{
			if ( Subgraphs.Contains( graph ) )
				return;
			Subgraphs.Add( graph );
		}
		foreach ( var node in graph.Nodes )
		{
			if ( node is SubgraphNode subgraphNode )
			{
				AddSubgraphs( subgraphNode.Subgraph );
			}
		}
	}

	public void RegisterVertexInput( string type, string name, string semantic )
	{
		name = CleanName( name );

		if ( ShaderTemplate.InternalVertexInputs.ContainsKey( name ) )
		{
			SGPLog.Error( $"InternalVertexInputs already contains VetexInput \"{name}\"" );

			return;
		}

		var vertexInput = $"{type} {name} : {semantic};";

		if ( !VertexInputs.ContainsKey( name ) )
		{
			VertexInputs.Add( name, vertexInput );
		}
		else
		{
			SGPLog.Warning( $"VertexInputs already contains key \"{name}\"", IsNotPreview );
		}
	}

	public void RegisterPixelInput( string type, string name, string semantic )
	{
		name = CleanName( name );

		if ( ShaderTemplate.InternalPixelInputs.ContainsKey( name ) )
		{
			SGPLog.Error( $"InternalPixelInputs already contains PixelInput \"{name}\"" );

			return;
		}

		var pixelInput = $"{type} {name} : {semantic};";

		if ( !PixelInputs.ContainsKey( name ) )
		{
			PixelInputs.Add( name, pixelInput );
		}
		else
		{
			SGPLog.Warning( $"PixelInputs already contains key \"{name}\"", IsNotPreview );
		}
	}

	public void RegisterVoidFunction( string functionCall, string nodeID, List<VoidFunctionArgument> args, out List<(string userAssigned, string compilerAssigned)> Outputs )
	{
		List<TargetResultData> targetResults = new();
		Outputs = new();

		if ( !ShaderResult.VoidLocals.ContainsKey( nodeID ) )
		{
			var funcCall = functionCall;
			Dictionary<string, string> functionOutputs = new();
			Dictionary<(string, string), string> targetProperties = new();

			foreach ( var inputArg in args.Where( x => x.ArgumentType == VoidFunctionArgumentType.Input ).Index() )
			{
				targetProperties.Add( (inputArg.Item.TargetProperty, inputArg.Item.VarName), inputArg.Item.DefaultTargetProperty );
			}

			foreach ( var outputArg in args.Where( x => x.ArgumentType == VoidFunctionArgumentType.Output ).Index() )
			{
				funcCall = funcCall.Replace( outputArg.Item.VarName, outputArg.Item.TargetProperty );

				var userAssignedname = outputArg.Item.TargetProperty;
				var hlslType = outputArg.Item.ResultType.GetHLSLDataType();
				var id = VoidLocalCount++;
				var varName = $"ol_{id}";
				TargetResultData data = new();

				funcCall = funcCall.Replace( userAssignedname, varName );

				data.CompilerAssignedName = varName;
				data.UserAssignedName = userAssignedname;
				data.ResultType = GetResultTypeFromHLSLDataType( hlslType );

				Outputs.Add( new( userAssignedname, varName ) );

				targetResults.Add( data );
			}

			var voidData = new VoidData()
			{
				TargetResults = targetResults,

				TargetProperties = targetProperties,
				ResultType = ResultType.Invalid,
				FunctionCall = funcCall,
				AlreadyDefined = false,
				InlineCode = false,
				BoundNodeIdentifier = nodeID
			};

			ShaderResult.VoidLocals.Add( nodeID, voidData );
		}
	}

	internal string CustomCodeRegister( string functionName, string functionInputs, string nodeID, StringBuilder inlineCodeSb, Dictionary<string, string> outputResults, bool isInlineCode = false )
	{
		var functionOutputsSb = new StringBuilder();
		List<TargetResultData> targetResults = new();

		foreach ( var output in outputResults.Index() )
		{
			var userAssignedname = output.Item.Key;
			var id = VoidLocalCount++; // name;
			var varName = $"ol_{id}";
			TargetResultData data = new();

			if ( !isInlineCode )
			{
				functionOutputsSb.Append( (output.Index + 1) == outputResults.Count ? $"{varName}" : $" {varName}, " );
			}
			else
			{
				inlineCodeSb.Replace( userAssignedname, varName );
			}

			data.CompilerAssignedName = varName;
			data.UserAssignedName = userAssignedname;
			data.ResultType = GetResultTypeFromHLSLDataType( output.Item.Value );

			targetResults.Add( data );
		}

		// Assemble the function call
		var funcCall = "";
		if ( !isInlineCode )
		{
			funcCall = $"{functionName}( {functionInputs},{functionOutputsSb.ToString()} )";
		}
		else
		{
			funcCall = $"{inlineCodeSb.ToString()}";
		}

		//SGPLog.Info( $"Generated FunctionCall \"{funcCall}\"", IsNotPreview );

		var voidData = new VoidData()
		{
			TargetResults = targetResults,
			ResultType = ResultType.Invalid,
			FunctionCall = funcCall,
			AlreadyDefined = false,
			InlineCode = isInlineCode,//false,
			BoundNodeIdentifier = SubgraphNode == null ? nodeID : SubgraphNode.Identifier,
		};

		//if ( !ShaderResult.VoidLocalsNew.Contains(  ) );
		{
			ShaderResult.CustomCodeResults.Add( voidData );

			return funcCall;
		}
	}

	public void RegisterInclude( string path )
	{
		var list = IsVs ? VertexIncludes : PixelIncludes;

		if ( list.Contains( path ) )
			return;

		list.Add( path );
	}

	/// <summary>
	/// Register some generic global parameter for a node to use.
	/// </summary>
	public void RegisterGlobal( string name, string global )
	{
		var result = ShaderResult;
		if ( result.Globals.ContainsKey( name ) )
			return;

		result.Globals.Add( name, global );
	}

	public string ResultHLSLFunction( string name, params string[] args )
	{
		if ( !GraphHLSLFunctions.HasFunction( name ) )
			return null;

		var result = ShaderResult;
		if ( !result.Functions.Contains( name ) )
			result.Functions.Add( name );

		return $"{name}( {string.Join( ", ", args )} )";
	}

	public string RegisterHLSLFunction( string code, [CallerArgumentExpression( nameof( code ) )] string functionName = "" )
	{
		if ( !GraphHLSLFunctions.HasFunction( functionName ) )
		{
			GraphHLSLFunctions.RegisterFunction( functionName, code );
		}

		return functionName;
	}

	/// <summary>
	/// Loops through ShaderResult.Gradients to find the matching key then returns the corresponding Gradient.
	/// </summary>
	public Gradient GetGradient( string gradient_name )
	{
		var result = ShaderResult;

		Gradient searchResult = new();

		foreach ( var gradient in result.Gradients )
		{
			if ( gradient.Key == gradient_name )
			{
				searchResult = gradient.Value;
			}
		}

		return searchResult;
	}

	/// <summary>
	/// Register a gradient and return the name of the graident. A generic name is returned instead if the gradient name is empty.
	/// </summary>
	public string RegisterGradient( Gradient gradient, string gradient_name )
	{
		var result = ShaderResult;

		var name = CleanName( gradient_name );

		name = string.IsNullOrWhiteSpace( name ) ? $"Gradient{result.Gradients.Count}" : name;

		var id = name;

		if ( !result.Gradients.ContainsKey( id ) )
		{
			result.Gradients.Add( id, gradient );
		}

		return name;
	}

	internal void RegisterSyncID( string syncID, string name )
	{
		if ( !SyncIDs.ContainsKey( syncID ) )
		{
			SyncIDs.Add( syncID, name );
		}
	}

	internal bool CheckTextureInputRegistration( string name )
	{
		return ShaderResult.TextureInputs.ContainsKey( name );
	}

	private string SetResultTextureName( string name, bool useProvidedName )
	{
		var cleanedName = CleanName( name );

		if ( useProvidedName )
		{
			return string.IsNullOrWhiteSpace( cleanedName ) ? $"Texture_{StageName}_{ShaderResult.TextureInputs.Count}" : cleanedName;
		}
		else
		{
			return string.IsNullOrWhiteSpace( cleanedName ) || IsPreview ? $"Texture_{StageName}_{ShaderResult.TextureInputs.Count}" : cleanedName;
		}
	}

	/// <summary>
	/// Register a texture and return the name of it
	/// </summary>
	public string ResultTexture( TextureInput input, bool useProvidedName = false )
	{
		var name = SetResultTextureName( input.Name, useProvidedName );
		var result = ShaderResult;

		if ( !result.TextureInputs.TryGetValue( name, out var existingValue ) )
		{
			result.TextureInputs.Add( name, input );
		}

		var globalName = $"g_t{name}";

		if ( CurrentResultInput == "Albedo" )
		{
			result.RepresentativeTexture = globalName;
		}

		return globalName;
	}

	/// <summary>
	/// Register a texture and return the name of it
	/// </summary>
	public string ResultTexture( TextureInput input, Texture texture, bool isTex2DParameterConnected = false )
	{
		var name = SetResultTextureName( input.Name, false );
		var result = ShaderResult;

		if ( !isTex2DParameterConnected )
		{
			SGPLog.Info( $"ResultTextureNew Path 1 // g_t{name}" );

			if ( !result.TextureInputs.TryGetValue( name, out var existingValue ) )
			{
				result.TextureInputs.Add( name, input );

				if ( texture != null )
				{
					SetShaderAttribute( name, texture );
				}
			}
		}
		else
		{
			if ( texture != null )
			{
				SetShaderAttribute( name, texture );
			}
		}

		var globalName = $"g_t{name}";

		if ( CurrentResultInput == "Albedo" )
		{
			result.RepresentativeTexture = globalName;
		}

		return globalName;
	}

	public string ResultSampler( Sampler sampler, bool alreadyProcessed = false )
	{
		var name = CleanName( sampler.Name );
		name = string.IsNullOrWhiteSpace( name ) ? $"Sampler{ShaderResult.SamplerStates.Count}" : name;
		var id = name;

		if ( IsPreview )//|| string.IsNullOrWhiteSpace( name ) || Subgraph is not null )
		{
			return ResultValue( sampler ).Code;
		}

		if ( IsNotPreview )
		{
			if ( !ShaderResult.SamplerStates.ContainsKey( id ) )
			{
				ShaderResult.SamplerStates.Add( id, sampler );
			}
			else
			{
				SGPLog.Warning( $"ShaderResult.SamplerStates already contains id \"{id}\"" );
			}

			return $"g_s{id}";
		}

		return $"g_s{id}";
	}

	public string ResultSamplerOrDefault( NodeInput samplerInput, Sampler defaultSampler )
	{
		var resultSampler = Result( samplerInput );
		return resultSampler.IsValid ? resultSampler.Code : ResultSampler( defaultSampler );
	}

	/// <summary>
	/// Get result of an input with an optional default value if it failed to resolve
	/// </summary>
	public NodeResult ResultOrDefault<T>( NodeInput input, T defaultValue )
	{
		var result = Result( input );
		return result.IsValid ? result : ResultValue( defaultValue );
	}

	/// <summary>
	/// Get result of an named reroute
	/// </summary>
	internal NodeResult ResultNamedReroute( string name )
	{
		var node = Graph.FindNamedRerouteDeclarationNode( name );

		if ( node != null )
		{
			var result = node.Result.Invoke( this );

			return result;
		}

		return default;
	}

	/// <summary>
	/// Get result of an input
	/// </summary>
	public NodeResult Result( NodeInput input )
	{
		if ( !input.IsValid )
			return default;

		BaseNodePlus node = null;
		if ( string.IsNullOrEmpty( input.Subgraph ) )
		{
			// Local results.
			if ( Subgraph is not null )
			{
				var nodeId = string.Join( ',', SubgraphStack.Select( x => x.Item1.Identifier ) );

				return Result( new()
				{
					Identifier = input.Identifier,
					Output = input.Output,
					Subgraph = Subgraph.Path,
					SubgraphNode = nodeId
				} );
			}

			node = Graph.FindNode( input.Identifier );
		}
		else
		{
			var subgraph = Subgraphs.FirstOrDefault( x => x.Path == input.Subgraph );
			if ( subgraph is not null )
			{
				node = subgraph.FindNode( input.Identifier );
			}
		}

		if ( ShaderResult.InputResults.TryGetValue( input, out var result ) )
		{
			return result;
		}

		if ( node == null )
		{
			return default;
		}

		var nodeType = node.GetType();
		var property = nodeType.GetProperty( input.Output );

		if ( property == null )
		{
			// Search for alias
			var allProperties = nodeType.GetProperties();
			foreach ( var prop in allProperties )
			{
				var alias = prop.GetCustomAttribute<AliasAttribute>();
				if ( alias is null ) continue;
				foreach ( var al in alias.Value )
				{
					if ( al == input.Output )
					{
						property = prop;
						break;
					}
				}
				if ( property != null )
					break;
			}
		}

		object value = null;

		if ( node is not IRerouteNode && node is not CustomFunctionNode && InputStack.Contains( input ) )
		{
			NodeErrors[node] = new List<string> { "Circular reference detected" };
			return default;
		}

		InputStack.Add( input );

		if ( Subgraph is not null && node.Graph != Subgraph )
		{
			if ( node.Graph != Graph )
			{
				Subgraph = node.Graph as ShaderGraphPlus;
			}
			else
			{
				Subgraph = null;
			}
		}

		if ( node is VoidFunctionBase VoidFunctionBase )
		{
			VoidFunctionBase.Register( this );

			if ( !ShaderResult.VoidLocals.ContainsKey( VoidFunctionBase.Identifier ) )
			{
				VoidFunctionBase.RegisterVoidFunction( this );
			}
			else
			{
				//SGPLog.Info( $"Node with ID `{Identifier}` has already been registerd!", compiler.IsPreview );
			}

			PostProcessVoidFunctionResult( VoidFunctionBase );
		}

		// Nodes that just make use of Metadata in NodeResult.Metadata
		if ( node is not SubgraphInput && node is IMetaDataNode imetaDataNode )
		{
			var metaDataResult = imetaDataNode.GetResult( this );

			if ( metaDataResult.IsValid )
			{
				InputStack.Remove( input );

				return metaDataResult;
			}
		}

		if ( node is CustomFunctionNode customFunctionNode )
		{
			var funcResult = customFunctionNode.GetResult( this );
			funcResult.AddMetadataEntry( nameof( MetadataType.VoidResultUserDefinedName ), input.Output );

			if ( !funcResult.IsValid )
			{
				if ( !NodeErrors.TryGetValue( node, out var errors ) )
				{
					errors = new();
					NodeErrors.Add( node, errors );
				}

				if ( funcResult.Errors is null || funcResult.Errors.Length == 0 )
				{
					errors.Add( $"Missing input" );
				}
				else
				{
					foreach ( var error in funcResult.Errors )
						errors.Add( error );
				}

				InputStack.Remove( input );
				return default;
			}

			VoidData voidData;
			if ( SubgraphNode != null )
			{
				voidData = ShaderResult.CustomCodeResults.Where( x => x.BoundNodeIdentifier == SubgraphNode.Identifier ).FirstOrDefault();
			}
			else
			{
				voidData = ShaderResult.CustomCodeResults.Where( x => x.BoundNodeIdentifier == customFunctionNode.Identifier ).FirstOrDefault();
			}

			var userAssignedVariableName = input.Output;
			var compilerAssignedVariableName = voidData.GetCompilerAssignedName( userAssignedVariableName );

			if ( !voidData.IsValid )
			{
				SGPLog.Error( $"Couldnt find VoidData in dictionary!", IsPreview );

				NodeErrors[node] = new List<string> { $"Failed to get result!", };

				return default;
			}
			else
			{
				var resultType = voidData.GetResultResultType( compilerAssignedVariableName );
				var localResult = new NodeResult( resultType, $"{compilerAssignedVariableName}", false, funcResult.Metadata );

				if ( Subgraph != null )
				{
					if ( ShaderResult.CustomFunctionHashCodes.Contains( voidData.GetHashCode() ) )
					{
						InputStack.Remove( input );
						return localResult;
					}
				}
				else
				{
					// return the localResult if we are getting a result from a node that we have already evaluated. 
					foreach ( var inputResult in ShaderResult.InputResults )
					{
						if ( inputResult.Key.Identifier == input.Identifier )
						{
							InputStack.Remove( input );
							return localResult;
						}
					}
				}

				ShaderResult.InputResults.Add( input, localResult );
				ShaderResult.Results.Add( (localResult, funcResult) );
				ShaderResult.CustomFunctionHashCodes.Add( voidData.GetHashCode() );

				InputStack.Remove( input );

				return localResult;
			}
		}

		if ( node is SubgraphNode subgraphNode )
		{
			var newStack = (subgraphNode, Subgraph);
			var lastNode = SubgraphNode;

			SubgraphStack.Add( newStack );
			Subgraph = subgraphNode.Subgraph;
			SubgraphNode = subgraphNode;

			if ( !Subgraphs.Contains( Subgraph ) )
			{
				Subgraphs.Add( Subgraph );
			}

			var resultNode = Subgraph.Nodes.OfType<SubgraphOutput>().Where( x => x.OutputName == input.Output ).FirstOrDefault();
			var resultInput = resultNode.Inputs.FirstOrDefault( x => x.Identifier == input.Output );
			if ( resultInput?.ConnectedOutput is not null )
			{
				var nodeId = string.Join( ',', SubgraphStack.Select( x => x.Item1.Identifier ) );
				var newConnection = new NodeInput()
				{
					Identifier = resultInput.ConnectedOutput.Node.Identifier,
					Output = resultInput.ConnectedOutput.Identifier,
					Subgraph = Subgraph.Path,
					SubgraphNode = nodeId
				};
				var newResult = Result( newConnection );

				if ( NodeErrors.Any() )
				{
					InputStack.Remove( input );
					return default;
				}

				InputStack.Remove( input );
				SubgraphStack.RemoveAt( SubgraphStack.Count - 1 );
				Subgraph = newStack.Item2;
				SubgraphNode = lastNode;

				return newResult;
			}
			else
			{
				value = GetDefaultValue( subgraphNode, input.Output, resultInput.Type );
				SubgraphStack.RemoveAt( SubgraphStack.Count - 1 );
				Subgraph = newStack.Item2;
				SubgraphNode = lastNode;
			}
		}
		else
		{
			if ( Subgraph is not null )
			{

				if ( node is SubgraphInput subgraphInput && !string.IsNullOrWhiteSpace( subgraphInput.InputName ) )
				{
					var newResult = ResolveSubgraphInput( subgraphInput, ref value, out var error );

					if ( !string.IsNullOrWhiteSpace( error.ErrorString ) )
					{
						NodeErrors.Add( error.Node, new List<string> { error.ErrorString } );

						InputStack.Remove( input );
						return default;
					}

					if ( newResult.IsValid )
					{
						InputStack.Remove( input );
						return newResult;
					}
				}
			}
			else if ( Graph.IsSubgraph )
			{
				if ( node is SubgraphInput subgraphInput )
				{
					//SGPLog.Info( $"Resolving subgraph input : {subgraphInput.InputName} " );

					if ( subgraphInput.PreviewInput.IsValid )
					{
						var subgraphInputResult = Result( subgraphInput.PreviewInput );

						InputStack.Remove( input );
						return subgraphInputResult;
					}
				}
			}

			if ( value is null )
			{
				if ( property == null )
				{
					InputStack.Remove( input );
					return default;
				}

				value = property.GetValue( node );
			}

			if ( value == null )
			{
				InputStack.Remove( input );
				return default;
			}
		}

		if ( value is NodeResult nodeResult )
		{
			InputStack.Remove( input );
			return nodeResult;
		}
		else if ( value is NodeInput nodeInput )
		{
			if ( nodeInput == input )
			{
				InputStack.Remove( input );
				return default;
			}

			var newResult = Result( nodeInput );

			InputStack.Remove( input );
			return newResult;
		}
		else if ( value is NodeResult.Func resultFunc )
		{
			var funcResult = resultFunc.Invoke( this );

			if ( !funcResult.IsValid )
			{
				if ( !NodeErrors.TryGetValue( node, out var errors ) )
				{
					errors = new();
					NodeErrors.Add( node, errors );
				}

				if ( funcResult.Errors is null || funcResult.Errors.Length == 0 )
				{
					errors.Add( $"Missing input" );
				}
				else
				{
					foreach ( var error in funcResult.Errors )
						errors.Add( error );
				}

				InputStack.Remove( input );
				return default;
			}

			funcResult.SetVoidLocalTargetID( node.Identifier );

			if ( IsPreview )
			{
				funcResult.SetPreviewID( node.PreviewID );
				funcResult.ShouldPreview = node.CanPreview;
			}
			else
			{
				funcResult.ShouldPreview = false;
			}

			//if ( subgraphResult )
			//{
			//	funcResult.SetPreviewID( SubgraphNode.PreviewID );
			//}

			// We can return this result without making it a local variable because it's constant
			if ( funcResult.Constant )
			{
				//SGPLog.Info( $"Result from node : `{node}` is constant! which is `{funcResult.Code}`", IsNotPreview );
				InputStack.Remove( input );

				return funcResult;
			}

			int id = ShaderResult.InputResults.Count;
			var varName = $"l_{id}";
			var localResult = new NodeResult( funcResult.ResultType, varName, false, funcResult.Metadata );
			localResult.SetPreviewID( funcResult.PreviewID );
			localResult.SetVoidLocalTargetID( funcResult.VoidLocalTargetID );
			localResult.SkipLocalGeneration = funcResult.SkipLocalGeneration;
			localResult.ShouldPreview = funcResult.ShouldPreview;

			if ( !ShaderResult.InputResults.ContainsKey( input ) )
			{
				ShaderResult.InputResults.Add( input, localResult );
			}

			ShaderResult.Results.Add( (localResult, funcResult) );

			InputStack.Remove( input );

			return localResult;
		}

		var resultVal = ResultValue( value );

		InputStack.Remove( input );

		return resultVal;
	}

	/// <summary>
	/// Get result of two inputs and cast to the largest component of the two (a float2 and float3 will both become float3 results)
	/// </summary>
	public (NodeResult, NodeResult) Result( NodeInput a, NodeInput b, float defaultA = 0.0f, float defaultB = 1.0f )
	{
		var resultA = ResultOrDefault( a, defaultA );
		var resultB = ResultOrDefault( b, defaultB );

		if ( resultA.Components == resultB.Components )
			return (resultA, resultB);

		if ( resultA.Components < resultB.Components )
			return (new( resultB.ResultType, resultA.Cast( resultB.Components ) ), resultB);

		return (resultA, new( resultA.ResultType, resultB.Cast( resultA.Components ) ));
	}

	/// <summary>
	/// Get result of a value that can be set in material editor
	/// </summary>
	public NodeResult ResultParameter<T>( string name, T value, T min = default, T max = default, bool isRange = false, bool isAttribute = false, ParameterUI ui = default )
	{
		if ( IsPreview || string.IsNullOrWhiteSpace( name ) || Subgraph is not null )
			return ResultValue( value );


		var attribName = name;
		name = CleanName( name );
		var prefix = GetLocalPrefix( value );

		// Make sure the type T is can have a Default();
		bool canHaveDefualt = typeof( T ) switch
		{
			Type t when t == typeof( Float2x2 ) => false,
			Type t when t == typeof( Float3x3 ) => false,
			Type t when t == typeof( Float4x4 ) => false,
			_ => true,
		};

		if ( !name.StartsWith( prefix ) )
			name = prefix + name;

		if ( ShaderResult.Parameters.TryGetValue( name, out var parameter ) )
			return parameter.Result;

		parameter.Result = ResultValue( value, name );

		var options = new StringWriter();

		// If we're an attribute, we don't care about the UI options
		if ( isAttribute )
		{
			options.Write( $"Attribute( \"{attribName}\" ); " );

			if ( value is bool boolValue )
			{
				options.Write( $"Default( {(boolValue ? 1 : 0)} ); " );
			}
			else if ( canHaveDefualt )
			{
				options.Write( $"Default{parameter.Result.Components}( {value} );" );
			}
		}
		else if ( value is not Float2x2 || value is not Float3x3 || value is not Float4x4 )
		{
			if ( ui.Type != UIType.Default )
			{
				options.Write( $"UiType( {ui.Type} ); " );
			}

			if ( ui.Step > 0.0f )
			{
				options.Write( $"UiStep( {ui.Step} ); " );
			}

			options.Write( $"UiGroup( \"{ui.UIGroup}\" ); " );

			if ( value is bool boolValue )
			{
				options.Write( $"Default( {(boolValue ? 1 : 0)} ); " );
			}
			else if ( canHaveDefualt )
			{
				options.Write( $"Default{parameter.Result.Components}( {value} ); " );
			}

			if ( value is not bool && parameter.Result.Components > 0 && isRange )
			{
				options.Write( $"Range{parameter.Result.Components}( {min}, {max} ); " );
			}
		}

		parameter.Options = options.ToString().Trim();

		// Avoid adding a matrix parameter to the graph if isAttribute is false. Which would make it code to the shader code and unable to be set externally via C#.
		if ( value is not Float2x2 || value is not Float3x3 || value is not Float4x4 )
		{
			ShaderResult.Parameters.Add( name, parameter );
		}

		return parameter.Result;
	}

	private string CompileTexture( string path, TextureInput textureInput )
	{
		if ( string.IsNullOrWhiteSpace( path ) )
			return "";

		var resourceText = string.Format( ShaderTemplate.TextureDefinition,
			path,
			textureInput.ColorSpace,
			textureInput.ImageFormat,
			textureInput.Processor
		);

		var assetPath = $"shadergraphplus/{path.Replace( ".", "_" )}_shadergraphplus.generated.vtex";

		if ( !CompiledTextures.ContainsKey( resourceText ) )
		{
			CompiledTextures.Add( resourceText, assetPath );
		}
		else
		{
			return CompiledTextures[resourceText];
		}

		var resourcePath = Editor.FileSystem.Root.GetFullPath( "/.source2/temp" );
		resourcePath = System.IO.Path.Combine( resourcePath, assetPath );

		if ( AssetSystem.CompileResource( resourcePath, resourceText ) )
		{
			return assetPath;
		}
		else
		{
			Log.Warning( $"Failed to compile {path}" );
			return "";
		}
	}

	/// <summary>
	/// Get result of a value, in preview mode an attribute will be registered and returned
	/// Only supports float, Vector2, Vector3, Vector4, Color.
	/// </summary>
	public NodeResult ResultValue<T>( T value, string name = null, bool previewOverride = false )
	{
		if ( value is NodeInput nodeInput ) return Result( nodeInput );

		bool isConstant = IsPreview && !previewOverride;
		bool isNamed = isConstant || !string.IsNullOrWhiteSpace( name );
		name = isConstant ? $"g_{StageName}_{ShaderResult.Attributes.Count}" : name;

		if ( isConstant )
		{
			SetShaderAttribute( name, value );
		}

		return value switch
		{
			bool v => isNamed ? new NodeResult( ResultType.Bool, $"{name}" ) : new NodeResult( ResultType.Bool, $"{v.ToString().ToLower()}" ) { },
			int v => isNamed ? new NodeResult( ResultType.Int, $"{name}" ) : new NodeResult( ResultType.Int, $"{v}", true ),
			float v => isNamed ? new NodeResult( ResultType.Float, $"{name}" ) : new NodeResult( ResultType.Float, $"{v}", true ),
			Vector2 v => isNamed ? new NodeResult( ResultType.Vector2, $"{name}" ) : new NodeResult( ResultType.Vector2, $"float2( {v.x}, {v.y} )" ),
			Vector3 v => isNamed ? new NodeResult( ResultType.Vector3, $"{name}" ) : new NodeResult( ResultType.Vector3, $"float3( {v.x}, {v.y}, {v.z} )" ),
			Vector4 v => isNamed ? new NodeResult( ResultType.Vector4, $"{name}" ) : new NodeResult( ResultType.Vector4, $"float4( {v.x}, {v.y}, {v.z}, {v.w} )" ),
			Color v => isNamed ? new NodeResult( ResultType.Color, $"{name}" ) : new NodeResult( ResultType.Color, $"float4( {v.r}, {v.g}, {v.b}, {v.a} )" ),
			Float2x2 v => isNamed ? new NodeResult( ResultType.Float2x2, $"{value}" ) : new NodeResult( ResultType.Float2x2, $"float2x2( {v.M11}, {v.M12}, {v.M21}, {v.M22} )" ),
			Float3x3 v => isNamed ? new NodeResult( ResultType.Float3x3, $"{value}" ) : new NodeResult( ResultType.Float3x3, $"float3x3( {v.M11}, {v.M12}, {v.M13}, {v.M21}, {v.M22}, {v.M23}, {v.M31}, {v.M32}, {v.M33} )" ),
			Float4x4 v => isNamed ? new NodeResult( ResultType.Float4x4, $"{value}" ) : new NodeResult( ResultType.Float4x4, $"float4x4( {v.M11}, {v.M12}, {v.M13}, {v.M14}, {v.M21}, {v.M22}, {v.M23}, {v.M24}, {v.M31}, {v.M32}, {v.M33}, {v.M34}, {v.M41}, {v.M42}, {v.M43}, {v.M44} )" ),
			Sampler v => new NodeResult( ResultType.Sampler, $"{name}", true ),
			TextureInput v => new NodeResult( ResultType.Texture2DObject, $"{name}", true ),
			_ => throw new ArgumentException( $"Unsupported attribute type `{value.GetType()}`" )
		};
	}

	private static string GetLocalPrefix<T>( T value )
	{
		var prefix = value switch
		{
			bool _ => "g_b",
			int _ => "g_n",
			float _ => "g_fl",
			Vector2 _ => "g_v",
			Vector3 _ => "g_v",
			Vector4 _ => "g_v",
			Color _ => "g_v",
			Float2x2 _ => "g_m",
			Float3x3 _ => "g_m",
			Float4x4 _ => "g_m",
			Texture2DObject _ => "g_t",
			TextureCubeObject _ => "g_t",
			Sampler _ => "g_s",
			_ => throw new Exception( $"Unknown value type \"{value.GetType()}\"" )
		};

		return prefix;
	}

	private static object GetDefaultValue( SubgraphNode node, string name, Type type )
	{
		if ( !node.DefaultValues.TryGetValue( name, out var value ) )
		{
			switch ( type )
			{
				case Type t when t == typeof( bool ):
					return false;
				case Type t when t == typeof( int ):
					return 1;
				case Type t when t == typeof( float ):
					return 1.0f;
				case Type t when t == typeof( Vector2 ):
					return Vector2.One;
				case Type t when t == typeof( Vector3 ):
					return Vector3.One;
				case Type t when t == typeof( Vector4 ):
					return Vector4.One;
				case Type t when t == typeof( Color ):
					return Color.White;
				case Type t when t == typeof( Sampler ):
					return new Sampler();
				case Type t when t == typeof( Texture2DObject ):
					return new TextureInput() { Type = TextureType.Tex2D };
				case Type t when t == typeof( TextureCubeObject ):
					return new TextureInput() { Type = TextureType.TexCube };
				default:
					throw new Exception( $"Type `{type}` has no default!" );
			}

		}

		if ( value is JsonElement el )
		{
			if ( type == typeof( bool ) )
			{
				value = el.GetBoolean();
			}
			else if ( type == typeof( int ) )
			{
				value = el.GetInt32();
			}
			else if ( type == typeof( float ) )
			{
				value = el.GetSingle();
			}
			else if ( type == typeof( Vector2 ) )
			{
				value = Vector2.Parse( el.GetString() );
			}
			else if ( type == typeof( Vector3 ) )
			{
				value = Vector3.Parse( el.GetString() );
			}
			else if ( type == typeof( Vector4 ) )
			{
				value = Vector4.Parse( el.GetString() );
			}
			else if ( type == typeof( Color ) )
			{
				value = Color.Parse( el.GetString() ) ?? Color.White;
			}
			else if ( type == typeof( Sampler ) )
			{
				value = JsonSerializer.Deserialize<Sampler>( el, ShaderGraphPlus.SerializerOptions() );
			}
			else if ( type == typeof( Texture2DObject ) )
			{
				var textureinput = JsonSerializer.Deserialize<TextureInput>( el, ShaderGraphPlus.SerializerOptions() );
				value = textureinput with { Type = TextureType.Tex2D };
			}
			else if ( type == typeof( TextureCubeObject ) )
			{
				var textureinput = JsonSerializer.Deserialize<TextureInput>( el, ShaderGraphPlus.SerializerOptions() );
				value = textureinput with { Type = TextureType.TexCube };
			}
		}

		return value;

	}

	private NodeResult ResolveSubgraphInput( SubgraphInput inputNode, ref object value, out (SubgraphNode Node, string ErrorString) error )
	{
		var lastStack = SubgraphStack.LastOrDefault();
		var lastNodeEntered = lastStack.Item1;
		error = new();

		if ( lastNodeEntered != null )
		{
			var parentInput = lastNodeEntered.InputReferences.FirstOrDefault( x => x.Key.Identifier == inputNode.InputName );
			if ( parentInput.Key is not null )
			{
				var lastSubgraph = Subgraph;
				var lastNode = SubgraphNode;
				Subgraph = lastStack.Item2;
				SubgraphNode = (Subgraph is null) ? null : lastNodeEntered;
				SubgraphStack.RemoveAt( SubgraphStack.Count - 1 );

				var connectedPlug = parentInput.Key.ConnectedOutput;
				if ( connectedPlug is not null )
				{
					var nodeId = string.Join( ',', SubgraphStack.Select( x => x.Item1.Identifier ) );
					var newResult = Result( new()
					{
						Identifier = connectedPlug.Node.Identifier,
						Output = connectedPlug.Identifier,
						Subgraph = Subgraph?.Path,
						SubgraphNode = nodeId
					} );
					SubgraphStack.Add( lastStack );
					Subgraph = lastSubgraph;
					SubgraphNode = lastNode;
					return newResult;
				}
				else
				{
					// These inputs are always required when within a subgraph.
					if ( Graph.IsSubgraph )
					{
						if ( parentInput.Value.inputNodeValueType == typeof( Texture2DObject ) )
						{
							error = new( lastNode, $"Texture2DObject Input \"{parentInput.Value.inputNode.InputName}\" is required when in a subgraph" );
							value = null;
							return new();
						}
						else if ( parentInput.Value.inputNodeValueType == typeof( TextureCubeObject ) )
						{
							error = new( lastNode, $"TextureCubeObject Input \"{parentInput.Value.inputNode.InputName}\" is required when in a subgraph" );
							value = null;
							return new();
						}
						else if ( parentInput.Value.inputNodeValueType == typeof( Sampler ) )
						{

							error = new( lastNode, $"SamplerState Input \"{parentInput.Value.inputNode.InputName}\" is required when in a subgraph" );
							value = null;
							return new();
						}
					}

					value = GetDefaultValue( lastNodeEntered, inputNode.InputName, parentInput.Value.inputNodeValueType );

					SubgraphStack.Add( lastStack );
					Subgraph = lastSubgraph;
					SubgraphNode = lastNode;

					if ( value is TextureInput textureInput )
					{
						var texurePath = CompileTexture( textureInput.DefaultTexture, textureInput );
						var textureGlobal = ResultTexture( textureInput, Texture.Load( texurePath ) );
						var resultType = textureInput.Type == TextureType.Tex2D ? ResultType.Texture2DObject : ResultType.TextureCubeObject;
						var result = new NodeResult( resultType, "TextureInput", textureInput );

						result.AddMetadataEntry( "TextureGlobal", textureGlobal );

						return result;
					}
					else if ( value is Sampler sampler )
					{
						var samplerResult = ResultSampler( sampler );
						return new NodeResult( ResultType.Sampler, samplerResult, constant: true );
					}
				}
			}
		}

		return new();
	}



	private static int GetComponentCount( Type inputType )
	{
		return inputType switch
		{
			Type t when t == typeof( int ) => 1,
			Type t when t == typeof( float ) => 1,
			Type t when t == typeof( Vector2 ) => 2,
			Type t when t == typeof( Vector3 ) => 3,
			Type t when t == typeof( Vector4 ) || t == typeof( Color ) => 4,
			Type t when t == typeof( Float4x4 ) => 16,
			Type t when t == typeof( Float3x3 ) => 6,
			Type t when t == typeof( Float2x2 ) => 4,
			_ => 0
		};
	}

	private static ResultType GetResultTypeFromHLSLDataType( string DataType )
	{
		return DataType switch
		{
			"bool" => ResultType.Bool,
			"int" => ResultType.Int,
			"float" => ResultType.Float,
			"float2" => ResultType.Vector2,
			"float3" => ResultType.Vector3,
			"float4" => ResultType.Color,
			"float2x2" => ResultType.Float2x2,
			"float3x3" => ResultType.Float3x3,
			"float4x4" => ResultType.Float4x4,
			"Texture2D" => ResultType.Texture2DObject,
			"TextureCube" => ResultType.TextureCubeObject,
			"SamplerState" => ResultType.Sampler,
			_ => throw new ArgumentException( $"Unknown DataType `{DataType}`" )
		};
	}

	private static IEnumerable<PropertyInfo> GetNodeInputProperties( Type type )
	{
		return type.GetProperties( BindingFlags.Instance | BindingFlags.Public )
			.Where( property => property.GetSetMethod() != null &&
			property.PropertyType == typeof( NodeInput ) &&
			property.IsDefined( typeof( BaseNodePlus.InputAttribute ), false ) );
	}

	private string GenerateFeatures()
	{
		var sb = new StringBuilder();
		var result = ShaderResult;

		// Register any Graph level Shader Features...
		//RegisterShaderFeatures( Graph.shaderFeatureNodeResults );

		if ( Graph.Domain is ShaderDomain.BlendingSurface )
		{
			sb.AppendLine( "Feature( F_MULTIBLEND, 0..3 ( 0=\"1 Layers\", 1=\"2 Layers\", 2=\"3 Layers\", 3=\"4 Layers\", 4=\"5 Layers\" ), \"Number Of Blendable Layers\" );" );
		}

		foreach ( var feature in ShaderFeatures )
		{
			if ( feature.Value is ShaderFeatureBoolean boolFeature )
			{
				sb.AppendLine( $"Feature( {boolFeature.GetFeatureString()}, 0..1, \"{boolFeature.HeaderName}\" );" );
			}
			else if ( feature.Value is ShaderFeatureEnum enumFeature )
			{
				var optionsBody = BuildFeatureOptionsBody( enumFeature.Options );
				sb.AppendLine( $"Feature( {enumFeature.GetFeatureString()}, 0..{enumFeature.Options.Count - 1} ( {optionsBody} ), \"{enumFeature.HeaderName}\" );" );
			}
		}

		//if ( Graph.FeatureRules.Any() )
		//{
		//	foreach ( var rule in Graph.FeatureRules )
		//	{
		//		if ( rule.IsValid )
		//		{
		//			sb.AppendLine( $"FeatureRule(Allow1( {String.Join( ", ", rule.Features )} ), \"{rule.HoverHint}\");" );
		//		}
		//	}
		//}

		return sb.ToString();
	}

	private string GenerateCommon()
	{
		var sb = new StringBuilder();

		if ( ShaderFeatures.Any() )
		{
			sb.AppendLine( $"#ifndef SWITCH_TRUE" );
			sb.AppendLine( $"#define SWITCH_TRUE 1" );
			sb.AppendLine( $"#endif" );

			sb.AppendLine( $"#ifndef SWITCH_FALSE" );
			sb.AppendLine( $"#define SWITCH_FALSE 0" );
			sb.AppendLine( $"#endif" );

			sb.AppendLine();
		}

		var blendMode = Graph.BlendMode;
		var alphaTest = blendMode == BlendMode.Masked ? 1 : 0;
		var translucent = blendMode == BlendMode.Translucent ? 1 : 0;

		sb.AppendLine( $"#ifndef S_ALPHA_TEST" );
		sb.AppendLine( $"#define S_ALPHA_TEST {alphaTest}" );
		sb.AppendLine( $"#endif" );

		sb.AppendLine( $"#ifndef S_TRANSLUCENT" );
		sb.AppendLine( $"#define S_TRANSLUCENT {translucent}" );
		sb.AppendLine( $"#endif" );

		return sb.ToString();
	}

	public string GeneratePostProcessingComponent( PostProcessingComponentInfo postProcessiComponentInfo, string className, string shaderPath )
	{
		var ppcb = new PostProcessingComponentBuilder( postProcessiComponentInfo );
		var type = "";

		foreach ( var parameter in ShaderResult.Parameters )
		{
			type = parameter.Value.Result.ComponentType.ToSimpleString();

			if ( type is "System.Boolean" )
			{
				ppcb.AddBoolProperty( parameter.Key, parameter.Value.Options );
			}
			if ( type is "float" )
			{
				ppcb.AddFloatProperty( type, parameter.Key, parameter.Value.Options );
			}
			if ( type is "Vector2" )
			{
				ppcb.AddVector2Property( type, parameter.Key, parameter.Value.Options );
			}
			if ( type is "Vector3" )
			{
				ppcb.AddVector3Property( type, parameter.Key, parameter.Value.Options );
			}
			if ( type is "Vector4" )
			{
				ppcb.AddVector4Property( type, parameter.Key, parameter.Value.Options );
			}
			if ( type is "Color" )
			{
				ppcb.AddVector4Property( type, parameter.Key, parameter.Value.Options );
			}
		}

		return ppcb.Finish( className, shaderPath );
	}

	/// <summary>
	/// Generate shader code, will evaluate the graph if it hasn't already.
	/// Different code is generated for preview and not preview.
	/// </summary>
	public string Generate()
	{
		if ( Graph.IsSubgraph )
		{
			if ( !Graph.Nodes.OfType<SubgraphOutput>().Any() )
			{
				NodeErrors.Add( new DummyNode(), [$"There must be atleast one Subgraph Output node!"] );
			}
		}

		// May have already evaluated and there's errors
		if ( Errors.Any() )
			return null;

		/*
		if ( Graph.MaterialDomain == MaterialDomain.BlendingSurface )
		{
			VertexInputs.Add( "vColorBlendValues", "float4 vColorBlendValues : TEXCOORD4 < Semantic( VertexPaintBlendParams ); >;" );
			VertexInputs.Add( "vColorPaintValues", "float4 vColorPaintValues : TEXCOORD5 < Semantic( VertexPaintTintColor ); >;" );
		
			PixelInputs.Add( "vBlendValues", "float4 vBlendValues : TEXCOORD14;" );
			PixelInputs.Add( "vPaintValues", "float4 vPaintValues : TEXCOORD15;" );
		}
		*/

		// Pre-Register anything before we Generate anything. Shouldn't cause any issues i hope.
		foreach ( var node in Graph.Nodes.OfType<IPreRegisterNodeData>() )
		{
			node.PreRegister( this );
		}

		var material = GenerateMaterial();
		var pixelOutput = GeneratePixelOutput();

		// If we have any errors after evaluating, no point going further
		if ( Errors.Any() )
			return null;

		string template = ShaderTemplate.Code;

		if ( Graph.Domain is ShaderDomain.BlendingSurface )
		{
			template = ShaderTemplateBlending.Code;
		}

		return string.Format( template,
			Graph.Description, // {0}
			IndentString( GenerateFeatures(), 1 ), // {1}
			IndentString( GenerateCommon(), 1 ), // {2}
			IndentString( GenerateStageInputs( ShaderStage.Vertex ), 1 ), // {3}
			IndentString( GenerateStageInputs( ShaderStage.Pixel ), 1 ), // {4}
			IndentString( GenerateGlobals(), 1 ), // {5}
			IndentString( GenerateLocals(), 2 ), // {6}
			IndentString( material, 2 ), // {7}
			IndentString( GenerateVertex(), 2 ), // {8}
			IndentString( GenerateGlobals(), 1 ), // {9}
			IndentString( GenerateVertexComboRules(), 1 ), // {10}
			IndentString( GeneratePixelComboRules(), 1 ),  // {11}
			IndentString( GenerateFunctions( PixelResult ), 1 ),  // {12}
			IndentString( GenerateFunctions( VertexResult ), 1 ),  // {13}
			IndentString( GeneratePixelInit(), 2 ), // {14}
			IndentString( pixelOutput, 2 ) // {15}
		);
	}

	private string GenerateStageInputs( ShaderStage shaderStage )
	{
		var sb = new StringBuilder();

		if ( shaderStage == ShaderStage.Vertex )
		{
			foreach ( var vertexInput in VertexInputs )
			{
				if ( !ShaderTemplate.InternalVertexInputs.ContainsValue( vertexInput.Value ) )
				{
					sb.AppendLine( vertexInput.Value );
				}
			}
		}
		else
		{
			foreach ( var pixelInput in PixelInputs )
			{
				if ( !ShaderTemplate.InternalPixelInputs.ContainsValue( pixelInput.Value ) )
				{
					sb.AppendLine( pixelInput.Value );
				}
			}
		}

		return sb.ToString();
	}

	private static string GenerateFunctions( CompileResult result )
	{
		if ( !result.Functions.Any() )
			return null;

		var sb = new StringBuilder();
		foreach ( var function in result.Functions )
		{
			if ( GraphHLSLFunctions.TryGetFunction( function, out var code ) )
			{
				sb.Append( code );
			}
		}

		return sb.ToString();
	}

	public static string IndentString( string input, int tabCount )
	{
		if ( tabCount == 0 ) return input;

		if ( string.IsNullOrWhiteSpace( input ) )
			return input;

		var tabs = new string( '\t', tabCount );
		var lines = input.Split( '\n' );

		for ( int i = 0; i < lines.Length; i++ )
		{
			lines[i] = tabs + lines[i];
		}

		return string.Join( "\n", lines );
	}

	private string GenerateVertexComboRules()
	{
		var sb = new StringBuilder();

		foreach ( var include in VertexIncludes )
		{
			sb.AppendLine( $"#include \"{include}\"" );
		}

		if ( IsNotPreview )
			return null;

		sb.AppendLine();
		return sb.ToString();
	}

	private string GeneratePixelComboRules()
	{
		var sb = new StringBuilder();
		var pixelIncludes = new HashSet<string>( PixelIncludes );

		if ( Graph.Domain == ShaderDomain.PostProcess )
		{
			pixelIncludes.Add( "postprocess/functions.hlsl" );
			pixelIncludes.Add( "postprocess/common.hlsl" );
		}

		sb.AppendLine();

		foreach ( var include in pixelIncludes )
		{
			sb.AppendLine( $"#include \"{include}\"" );
		}

		if ( !IsNotPreview )
		{
			sb.AppendLine();
			sb.AppendLine( "DynamicCombo( D_RENDER_BACKFACES, 0..1, Sys( ALL ) );" );
			sb.AppendLine( "RenderState( CullMode, D_RENDER_BACKFACES ? NONE : BACK );" );
		}
		else
		{
			sb.AppendLine();
			sb.AppendLine( "RenderState( CullMode, F_RENDER_BACKFACES ? NONE : DEFAULT );" );
		}

		return sb.ToString();
	}

	private string GeneratePixelInit()
	{
		Stage = ShaderStage.Pixel;
		if ( Graph.ShadingModel == ShadingModel.Lit && Graph.Domain != ShaderDomain.PostProcess )
			return ShaderTemplate.Material_init;
		return "";
	}

	private string GeneratePixelOutput()
	{
		Stage = ShaderStage.Pixel;

		Subgraph = null;
		SubgraphStack.Clear();

		if ( Graph.ShadingModel == ShadingModel.Unlit || Graph.Domain == ShaderDomain.PostProcess )
		{
			var resultNode = Graph.Nodes.OfType<BaseResult>().FirstOrDefault();
			if ( resultNode == null )
				return null;

			var albedoResult = resultNode.GetAlbedoResult( this );
			string albedo = "float3( 1.0f, 1.0f, 1.0f )";
			if ( albedoResult.IsValid )
			{
				albedo = albedoResult.Cast( GetComponentCount( typeof( Vector3 ) ) ) ?? "float3( 1.0f, 1.0f, 1.0f )";
			}

			var opacityResult = resultNode.GetOpacityResult( this );
			string opacity = "1.0f";
			if ( opacityResult.IsValid )
			{
				opacity = opacityResult.Cast( 1 ) ?? "1.0f";
			}

			return $"return float4( {albedo}, {opacity} );";
		}
		else if ( Graph.ShadingModel == ShadingModel.Lit )
		{
			return ShaderTemplate.Material_output;
		}

		return null;
	}

	private string GenerateGlobals()
	{
		var sb = new StringBuilder();

		foreach ( var global in ShaderResult.Globals )
		{
			sb.AppendLine( global.Value );
		}

		foreach ( var feature in ShaderFeatures )
		{
			if ( IsPreview )
			{
				sb.AppendLine( $"DynamicCombo( {feature.Value.GetDynamicComboString()}, {feature.Value.GetOptionRangeString()}, Sys( ALL ) );" );
			}
			else
			{
				sb.AppendLine( $"StaticCombo( {feature.Value.GetStaticComboString()}, {feature.Value.GetFeatureString()}, Sys( ALL ) );" );
			}

			sb.AppendLine();
		}

		// Support for color buffer in post-process shaders
		if ( IsPs && Graph.Domain is ShaderDomain.PostProcess )
		{
			sb.AppendLine( "Texture2D g_tColorBuffer < Attribute( \"ColorBuffer\" ); SrgbRead ( true ); >;" );
		}

		if ( IsPreview )
		{
			foreach ( var result in ShaderResult.TextureInputs )
			{
				sb.Append( $"{result.Value.CreateTexture( result.Key )} <" )
				  .Append( $" Attribute( \"{result.Key}\" );" )
				  .Append( $" SrgbRead( {result.Value.SrgbRead} ); >;" )
				  .AppendLine();
			}

			foreach ( var result in ShaderResult.Attributes )
			{
				if ( result.Value is Float2x2 || result.Value is Float3x3 || result.Value is Float4x4 || result.Value is Texture )
					continue;

				var typeName = result.Value switch
				{
					bool _ => "bool",
					int _ => "int",
					float _ => "float",
					Vector2 _ => "float2",
					Vector3 _ => "float3",
					Vector4 _ => "float4",
					Color _ => "float4",
					Sampler _ => "SamplerState",
					_ => null
				};

				sb.AppendLine( $"{typeName} {result.Key} < Attribute( \"{result.Key}\" ); >;" );
			}

			sb.AppendLine( "float g_flPreviewTime < Attribute( \"g_flPreviewTime\" ); >;" );
			sb.AppendLine( $"int g_iStageId < Attribute( \"g_iStageId\" ); >;" );
		}
		else
		{
			foreach ( var sampler in ShaderResult.SamplerStates )
			{
				if ( sampler.Value.IsAttribute )
				{
					sb.AppendLine( $"SamplerState g_s{sampler.Key} < Attribute( \"{sampler.Key}\" ); >;" );
				}
				else
				{
					sb.Append( $"SamplerState g_s{sampler.Key} <" )
					  .Append( $" Filter( {sampler.Value.Filter.ToString().ToUpper()} );" )
					  .Append( $" AddressU( {sampler.Value.AddressModeU.ToString().ToUpper()} );" )
					  .Append( $" AddressV( {sampler.Value.AddressModeV.ToString().ToUpper()} );" )
					  .Append( $" AddressW( {sampler.Value.AddressModeW.ToString().ToUpper()} );" )
					  .Append( $" MaxAniso( {sampler.Value.MaxAnisotropy.ToString()} ); >;" )
					  .AppendLine();
				}
			}

			foreach ( var result in ShaderResult.TextureInputs )
			{
				// If we're an attribute, we don't care about texture inputs
				if ( result.Value.IsAttribute )
					continue;

				var defaultTex = result.Value.DefaultTexture;
				sb.Append( $"{result.Value.CreateInput}( {result.Key}, {result.Value.ColorSpace}, 8," )
				  .Append( $" \"{result.Value.Processor.ToString()}\"," )
				  .Append( $" \"_{result.Value.ExtensionString.ToLower()}\"," )
				  .Append( $" \"{result.Value.UIGroup}\"," )
				  .Append( string.IsNullOrEmpty( defaultTex )
							? $" Default4( {result.Value.DefaultColor} ) );"
							: $" DefaultFile( \"{defaultTex}\" ) );" )
				  .AppendLine();
			}

			foreach ( var result in ShaderResult.TextureInputs )
			{
				// If we're an attribute, we don't care about the UI options
				if ( result.Value.IsAttribute )
				{
					sb.AppendLine( $"{result.Value.CreateTexture( result.Key )} < Attribute( \"{result.Key}\" ); >;" );
				}
				else
				{
					sb.Append( $"{result.Value.CreateTexture( result.Key )} < Channel( RGBA, Box( {result.Key} ), {(result.Value.SrgbRead ? "Srgb" : "Linear")} );" )
					  .Append( $" OutputFormat( {result.Value.ImageFormat} );" )
					  .Append( $" SrgbRead( {result.Value.SrgbRead} ); >;" )
					  .AppendLine();
				}
			}

			if ( !string.IsNullOrWhiteSpace( ShaderResult.RepresentativeTexture ) )
			{
				sb.AppendLine( $"TextureAttribute( LightSim_DiffuseAlbedoTexture, {ShaderResult.RepresentativeTexture} )" );
				sb.AppendLine( $"TextureAttribute( RepresentativeTexture, {ShaderResult.RepresentativeTexture} )" );
			}

			foreach ( var parameter in ShaderResult.Parameters )
			{
				var resultType = parameter.Value.Result.ResultType;

				sb.AppendLine( $"{parameter.Value.Result.TypeName} {parameter.Key} < {parameter.Value.Options} >;" );
			}
		}

		if ( sb.Length > 0 )
		{
			sb.Insert( 0, "\n" );
		}

		return sb.ToString();
	}

	internal void GenerateLocalResults( ref StringBuilder sb, IEnumerable<(NodeResult localResult, NodeResult funcResult)> shaderResults, out (NodeResult localResult, NodeResult funcResult) lastResult, bool appendOverride = false, bool noPreviewOverride = false, int indentLevel = 0 )
	{
		lastResult = (new NodeResult(), new NodeResult());

		foreach ( var result in shaderResults )
		{
			lastResult = result;
			var shouldSkip = result.funcResult.SkipLocalGeneration;

			if ( !shouldSkip || appendOverride )
			{
				string comboBody = "";

				if ( result.funcResult.TryGetMetaData<string>( nameof( MetadataType.ComboSwitchBody ), out var comboSwitchBody ) )
				{
					comboBody = comboSwitchBody;

					if ( !string.IsNullOrWhiteSpace( comboSwitchBody ) )
					{
						sb.AppendLine( IndentString( comboSwitchBody, indentLevel ) );
					}
				}

				if ( ShaderResult.VoidLocals.Any() && ShaderResult.VoidLocals.ContainsKey( result.funcResult.VoidLocalTargetID ) )
				{
					var data = ShaderResult.VoidLocals[result.funcResult.VoidLocalTargetID];
					sb.AppendLine();

					if ( !data.AlreadyDefined )
					{
						ShaderResult.VoidLocals[result.funcResult.VoidLocalTargetID] = data with { AlreadyDefined = true, InlineCode = false };

						// Init all the output results.
						foreach ( var outResult in data.TargetResults )
						{
							sb.AppendLine( IndentString( data.ResultInit( outResult.CompilerAssignedName, outResult.ResultType ), indentLevel ) );
						}

						sb.AppendLine( IndentString( $"{data.FunctionCall};", indentLevel ) );
					}
				}

				if ( ShaderResult.CustomCodeResults.Count > 0 && result.localResult.Metadata.Count > 0 )
				{
					var voidData = ShaderResult.CustomCodeResults
						.FirstOrDefault( vd => vd.TargetResults.Any( targetData => targetData.UserAssignedName == result.localResult.GetMetadata<string>( nameof( MetadataType.VoidResultUserDefinedName ) )
						&& targetData.CompilerAssignedName == result.localResult.Code
					) );

					if ( voidData.IsValid )
					{
						sb.AppendLine();

						// Init all the output results.
						foreach ( var outResult in voidData.TargetResults )
						{
							sb.AppendLine( IndentString( voidData.ResultInit( outResult.CompilerAssignedName, outResult.ResultType ), indentLevel ) );
						}

						if ( !voidData.InlineCode )
						{
							sb.AppendLine( IndentString( $"{voidData.FunctionCall};", indentLevel ) );
						}
						else
						{
							sb.AppendLine( IndentString( $"{voidData.FunctionCall}", indentLevel ) );
						}

						sb.AppendLine();
					}
				}

				if ( result.funcResult.ResultType != ResultType.Void )
				{
					if ( result.funcResult.ResultType == ResultType.Float2x2
					 || result.funcResult.ResultType == ResultType.Float3x3
					 || result.funcResult.ResultType == ResultType.Float4x4 )
					{
						if ( IsPreview )
						{
							sb.AppendLine( IndentString( $"{result.funcResult.TypeName} {result.localResult} = {result.funcResult.TypeName}( {result.funcResult.Code} );", indentLevel ) );
						}
						else
						{
							sb.AppendLine( IndentString( $"{result.funcResult.TypeName} {result.localResult} = {result.funcResult.Code};", indentLevel ) );
						}
					}
					else
					{
						sb.AppendLine( IndentString( $"{result.funcResult.TypeName} {result.localResult} = {result.funcResult.Code};", indentLevel ) );
					}
				}


				if ( !noPreviewOverride )
				{
					if ( IsPs && IsPreview && string.IsNullOrWhiteSpace( comboBody ) )
					{
						if ( result.localResult.ResultType == ResultType.Bool )
						{
							// TODO : There is no way to know what the actual value of the result is.
						}
						else if ( result.localResult.CanPreview && result.localResult.ShouldPreview && result.localResult.PreviewID != ShaderGraphPlusGlobals.GraphCompiler.NoNodePreviewID )
						{
							sb.AppendLine( IndentString( $"if ( g_iStageId == {result.localResult.PreviewID} ) return {result.localResult.Cast( 4, 1.0f )};", indentLevel ) );
						}
					}
				}
			}
		}
	}

	private string GenerateLocals( bool noPreviewOverride = false )
	{
		var sb = new StringBuilder();

		if ( ShaderResult.Results.Any() )
		{
			sb.AppendLine();
		}

		if ( Debug )
		{
			if ( IsPreview )
			{
				Log.Info( $"Registerd Gradient Count for Preview Is : {ShaderResult.Gradients.Count}" );
			}
			else
			{
				Log.Info( $"Registerd Gradient Count for Compile Is : {ShaderResult.Gradients.Count}" );
			}
		}

		foreach ( var gradient in ShaderResult.Gradients )
		{
			//Log.Info($"Found Gradient : {gradient.Key}");
			//Log.Info($" Gradient Blend Mode : {gradient.Value.Blending}");

			sb.AppendLine( $"Gradient {gradient.Key} = Gradient::Init();" );
			sb.AppendLine();

			var colorindex = 0;
			var alphaindex = 0;

			sb.AppendLine( $"{gradient.Key}.colorsLength = {gradient.Value.Colors.Count};" );
			sb.AppendLine( $"{gradient.Key}.alphasLength = {gradient.Value.Alphas.Count};" );

			foreach ( var color in gradient.Value.Colors )
			{
				if ( Debug )
				{
					Log.Info( $"{gradient.Key} Gradient Color {colorindex} : {color.Value} Time : {color.Time}" );
				}

				// All good with time as the 4th component?
				sb.AppendLine( $"{gradient.Key}.colors[{colorindex++}] = float4( {color.Value.r}, {color.Value.g}, {color.Value.b}, {color.Time} );" );
			}

			foreach ( var alpha in gradient.Value.Alphas )
			{
				sb.AppendLine( $"{gradient.Key}.alphas[{alphaindex++}] = float( {alpha.Value} );" );
			}

			sb.AppendLine();
		}

		GenerateLocalResults( ref sb, ShaderResult.Results, out _, false, noPreviewOverride, 0 );

		return sb.ToString();
	}

	private string GenerateMaterial()
	{
		Stage = ShaderStage.Pixel;
		Subgraph = null;
		SubgraphStack.Clear();

		if ( Graph.ShadingModel != ShadingModel.Lit || Graph.Domain == ShaderDomain.PostProcess ) return "";

		var resultNode = Graph.Nodes.OfType<BaseResult>().FirstOrDefault();

		if ( resultNode == null )
			return null;

		var sb = new StringBuilder();
		var visited = new HashSet<string>();

		foreach ( var property in GetNodeInputProperties( resultNode.GetType() ) )
		{
			if ( property.Name == "PositionOffset" )
				continue;

			CurrentResultInput = property.Name;
			visited.Add( property.Name );

			NodeResult result;

			if ( property.GetValue( resultNode ) is NodeInput connection && connection.IsValid() )
			{
				result = Result( connection );
			}
			else
			{
				var editorAttribute = property.GetCustomAttribute<BaseNodePlus.NodeValueEditorAttribute>();
				if ( editorAttribute == null )
					continue;

				var valueProperty = resultNode.GetType().GetProperty( editorAttribute.ValueName );
				if ( valueProperty == null )
					continue;

				result = ResultValue( valueProperty.GetValue( resultNode ) );

			}

			if ( Errors.Any() )
				return null;

			if ( !result.IsValid() )
				continue;

			if ( string.IsNullOrWhiteSpace( result.Code ) )
				continue;

			var inputAttribute = property.GetCustomAttribute<BaseNodePlus.InputAttribute>();
			var componentCount = GetComponentCount( inputAttribute.Type );

			sb.AppendLine( $"m.{property.Name} = {result.Cast( componentCount )};" );
		}

		if ( Graph.IsSubgraph )
		{
			var subgraphOutputs = Graph.Nodes.OfType<SubgraphOutput>();
			var reservedPreview = new Dictionary<string, BaseNodePlus>();

			foreach ( var subgraphOutput in subgraphOutputs )
			{
				if ( reservedPreview.ContainsKey( $"{subgraphOutput.Preview}" ) )
				{
					NodeErrors.Add( subgraphOutput, [$"Node with id \"{reservedPreview[$"{subgraphOutput.Preview}"].Identifier}\" has already set \"{subgraphOutput.Preview}\" as its preview type"] );
					continue;
				}

				if ( subgraphOutput.Preview == SubgraphOutputPreviewType.None )
					continue;

				reservedPreview.Add( $"{subgraphOutput.Preview}", subgraphOutput );

				subgraphOutput.AddMaterialOutput( this, sb, subgraphOutput.Preview, out var errors );

				if ( errors.Any() )
				{
					NodeErrors.Add( new DummyNode(), errors );

					return "";
				}
			}

			reservedPreview.Clear();
		}

		visited.Clear();
		CurrentResultInput = null;

		return sb.ToString();
	}

	private string GenerateVertex()
	{
		Stage = ShaderStage.Vertex;

		var resultNode = Graph.Nodes.OfType<BaseResult>().FirstOrDefault();
		if ( resultNode == null )
			return null;

		var positionOffsetInput = resultNode.GetPositionOffset();

		var sb = new StringBuilder();
		var sb2 = new StringBuilder();

		foreach ( var vertexInput in VertexInputs )
		{
			sb2.AppendLine( $"i.{vertexInput.Key} = v.{vertexInput.Key};" );
		}

		switch ( Graph.Domain )
		{
			case ShaderDomain.Surface:
				sb.AppendLine( $@"
PixelInput i = ProcessVertex( v );
i.vPositionOs = v.vPositionOs.xyz;

{sb2.ToString()}
ExtraShaderData_t extraShaderData = GetExtraPerInstanceShaderData( v.nInstanceTransformID );
i.vTintColor = extraShaderData.vTint;

VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );
		" );
				break;
			case ShaderDomain.BlendingSurface:
				sb.AppendLine( $@"
PixelInput i = ProcessVertex( v );

{sb2.ToString()}
i.vBlendValues = v.vColorBlendValues;
i.vPaintValues = v.vColorPaintValues;
" );
				break;
			case ShaderDomain.PostProcess:
				sb.AppendLine( $@"
PixelInput i;

{sb2.ToString()}
i.vPositionPs = float4( v.vPositionOs.xy, 0.0f, 1.0f );
i.vPositionWs = float3( v.vTexCoord, 0.0f );
" );
				break;
		}

		NodeResult result;

		if ( positionOffsetInput is NodeInput connection && connection.IsValid() )
		{
			result = Result( connection );

			if ( !Errors.Any() && result.IsValid() && !string.IsNullOrWhiteSpace( result.Code ) )
			{
				var componentCount = GetComponentCount( typeof( Vector3 ) );

				GenerateLocalResults( ref sb, ShaderResult.Results, out _, false, true, 0 );

				sb.AppendLine( $"i.vPositionWs.xyz += {result.Cast( componentCount )};" );
				sb.AppendLine( "i.vPositionPs.xyzw = Position3WsToPs( i.vPositionWs.xyz );" );
			}
		}

		switch ( Graph.Domain )
		{
			case ShaderDomain.Surface:
				sb.AppendLine( "return FinalizeVertex( i );" );
				break;
			case ShaderDomain.BlendingSurface:
				sb.AppendLine( "return FinalizeVertex( i );" );
				break;
			case ShaderDomain.PostProcess:
				sb.AppendLine( "return i;" );
				break;
		}
		return sb.ToString();
	}
}
