using Editor;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

public enum SubgraphPortType
{
	[Icon( "check_box" )]
	Bool,
	[Icon( "looks_one" )]
	Int,
	[Icon( "looks_one" )]
	Float,
	[Title( "Float2" ), Icon( "looks_two" )]
	Vector2,
	[Title( "Float3" ), Icon( "looks_3" )]
	Vector3,
	[Title( "Float4" ), Icon( "looks_4" )]
	Vector4,
	[Title( "Color" ), Icon( "palette" )]
	Color,
	[Title( "Float2x2" ), Icon( "apps" )]
	Float2x2,
	[Title( "Float3x3" ), Icon( "apps" )]
	Float3x3,
	[Title( "Float4x4" ), Icon( "apps" )]
	Float4x4,
	[Title( "Gradient" ), Icon( "gradient" )]
	Gradient,
	[Title( "Sampler State" ), Icon( "colorize" )]
	SamplerState,
	[Title( "Texture2D" ), Icon( "texture" )]
	Texture2DObject,
	[Title( "TextureCube" ), Icon( "view_in_ar" )]
	TextureCubeObject,
	[Hide]
	Invalid
}

/// <summary>
/// Input of a Subgraph.
/// </summary>
[Title( "Subgraph Input" ), Icon( "input" ), SubgraphOnly]
[InternalNode]
public sealed class SubgraphInput : ShaderNodePlus, IErroringNode, IWarningNode, IInitializeNode, IBlackboardSyncableNode, IMetaDataNode
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.SubgraphNode;

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	//[Hide, JsonIgnore]
	//private bool IsSubgraph => (Graph is ShaderGraphPlus shaderGraph && shaderGraph.IsSubgraph);

	[Hide]
	public Guid BlackboardParameterIdentifier { get; set; }

	[Hide]
	public override string Title => string.IsNullOrWhiteSpace( InputName ) ?
	$"Subgraph Input" :
	$"{InputName}";

	[Input, Title( "Preview" ), Hide]
	public NodeInput PreviewInput { get; set; }

	[Hide, Browsable( false )]
	public SubgraphPortType InputType { get; set; } = SubgraphPortType.Invalid;

	/// <summary>
	/// Name of this input.
	/// </summary>
	public string InputName { get; set; } = "In0";

	/// <summary>
	/// Description of this input.
	/// </summary>
	[TextArea]
	public string InputDescription { get; set; } = "";

	[Hide, Browsable( false )]
	public object DefaultData { get; set; } = null;

	/// <summary>
	/// Is this input required to have a valid connection?
	/// </summary>
	public bool IsRequired { get; set; } = false;

	public int PortOrder { get; set; }

	public SubgraphInput() : base()
	{

	}

	public SubgraphInput( bool isRequired ) : this()
	{
		IsRequired = isRequired;
	}

	public void UpdateFromBlackboard( BaseBlackboardParameter blackboardParameter )
	{
		if ( blackboardParameter is BoolSubgraphInputParameter boolParameter )
		{
			InputName = boolParameter.Name;
			InputDescription = boolParameter.Description;
			SetDefaultValue( boolParameter.Value );
			IsRequired = boolParameter.IsRequired;
			PortOrder = boolParameter.PortOrder;
		}
		else if ( blackboardParameter is IntSubgraphInputParameter intParameter )
		{
			InputName = intParameter.Name;
			InputDescription = intParameter.Description;
			SetDefaultValue( intParameter.Value );
			IsRequired = intParameter.IsRequired;
			PortOrder = intParameter.PortOrder;
		}
		else if ( blackboardParameter is FloatSubgraphInputParameter floatParameter )
		{
			InputName = floatParameter.Name;
			InputDescription = floatParameter.Description;
			SetDefaultValue( floatParameter.Value );
			IsRequired = floatParameter.IsRequired;
			PortOrder = floatParameter.PortOrder;
		}
		else if ( blackboardParameter is Float2SubgraphInputParameter float2Parameter )
		{
			InputName = float2Parameter.Name;
			InputDescription = float2Parameter.Description;
			SetDefaultValue( float2Parameter.Value );
			IsRequired = float2Parameter.IsRequired;
			PortOrder = float2Parameter.PortOrder;
		}
		else if ( blackboardParameter is Float3SubgraphInputParameter float3Parameter )
		{
			InputName = float3Parameter.Name;
			InputDescription = float3Parameter.Description;
			SetDefaultValue( float3Parameter.Value );
			IsRequired = float3Parameter.IsRequired;
			PortOrder = float3Parameter.PortOrder;
		}
		else if ( blackboardParameter is Float4SubgraphInputParameter float4Parameter )
		{
			InputName = float4Parameter.Name;
			InputDescription = float4Parameter.Description;
			SetDefaultValue( float4Parameter.Value );
			IsRequired = float4Parameter.IsRequired;
			PortOrder = float4Parameter.PortOrder;
		}
		else if ( blackboardParameter is ColorSubgraphInputParameter colorParameter )
		{
			InputName = colorParameter.Name;
			InputDescription = colorParameter.Description;
			SetDefaultValue( colorParameter.Value );
			IsRequired = colorParameter.IsRequired;
			PortOrder = colorParameter.PortOrder;
		}
		else if ( blackboardParameter is Float2x2SubgraphInputParameter float2x2Parameter )
		{
			InputName = float2x2Parameter.Name;
			InputDescription = float2x2Parameter.Description;
			SetDefaultValue( float2x2Parameter.Value );
			IsRequired = float2x2Parameter.IsRequired;
			PortOrder = float2x2Parameter.PortOrder;
		}
		else if ( blackboardParameter is Float3x3SubgraphInputParameter float3x3Parameter )
		{
			InputName = float3x3Parameter.Name;
			InputDescription = float3x3Parameter.Description;
			SetDefaultValue( float3x3Parameter.Value );
			IsRequired = float3x3Parameter.IsRequired;
			PortOrder = float3x3Parameter.PortOrder;
		}
		else if ( blackboardParameter is Float4x4SubgraphInputParameter float4x4Parameter )
		{
			InputName = float4x4Parameter.Name;
			InputDescription = float4x4Parameter.Description;
			SetDefaultValue( float4x4Parameter.Value );
			IsRequired = float4x4Parameter.IsRequired;
			PortOrder = float4x4Parameter.PortOrder;
		}
		else if ( blackboardParameter is GradientSubgraphInputParameter gradientParameter )
		{
			InputName = gradientParameter.Name;
			InputDescription = gradientParameter.Description;
			SetDefaultValue( gradientParameter.Value );
			IsRequired = gradientParameter.IsRequired;
			PortOrder = gradientParameter.PortOrder;
		}
		else if ( blackboardParameter is Texture2DSubgraphInputParameter texture2DParameter )
		{
			InputName = texture2DParameter.Name;
			InputDescription = texture2DParameter.Description;
			SetDefaultValue( texture2DParameter.Value );
			IsRequired = true;
			PortOrder = texture2DParameter.PortOrder;
		}
		else if ( blackboardParameter is TextureCubeSubgraphInputParameter textureCubeParameter )
		{
			InputName = textureCubeParameter.Name;
			InputDescription = textureCubeParameter.Description;
			SetDefaultValue( textureCubeParameter.Value );
			IsRequired = true;
			PortOrder = textureCubeParameter.PortOrder;
		}
		else if ( blackboardParameter is SamplerStateSubgraphInputParameter samplerStateParameter )
		{
			InputName = samplerStateParameter.Name;
			InputDescription = samplerStateParameter.Description;
			SetDefaultValue( samplerStateParameter.Value );
			IsRequired = true;
			PortOrder = samplerStateParameter.PortOrder;
		}
	}

	public void InitializeNode()
	{
		if ( DefaultData is JsonElement element )
		{
			switch ( InputType )
			{
				case SubgraphPortType.Bool:
					DefaultData = JsonSerializer.Deserialize<bool>( element, ShaderGraphPlus.SerializerOptions() );
					break;
				case SubgraphPortType.Int:
					DefaultData = JsonSerializer.Deserialize<int>( element, ShaderGraphPlus.SerializerOptions() );
					break;
				case SubgraphPortType.Float:
					DefaultData = JsonSerializer.Deserialize<float>( element, ShaderGraphPlus.SerializerOptions() );
					break;
				case SubgraphPortType.Vector2:
					DefaultData = JsonSerializer.Deserialize<Vector2>( element, ShaderGraphPlus.SerializerOptions() );
					break;
				case SubgraphPortType.Vector3:
					DefaultData = JsonSerializer.Deserialize<Vector3>( element, ShaderGraphPlus.SerializerOptions() );
					break;
				case SubgraphPortType.Vector4:
					DefaultData = JsonSerializer.Deserialize<Vector4>( element, ShaderGraphPlus.SerializerOptions() );
					break;
				case SubgraphPortType.Color:
					DefaultData = JsonSerializer.Deserialize<Color>( element, ShaderGraphPlus.SerializerOptions() );
					break;
				case SubgraphPortType.Float2x2:
					DefaultData = JsonSerializer.Deserialize<Float2x2>( element, ShaderGraphPlus.SerializerOptions() );
					break;
				case SubgraphPortType.Float3x3:
					DefaultData = JsonSerializer.Deserialize<Float3x3>( element, ShaderGraphPlus.SerializerOptions() );
					break;
				case SubgraphPortType.Float4x4:
					DefaultData = JsonSerializer.Deserialize<Float4x4>( element, ShaderGraphPlus.SerializerOptions() );
					break;
				case SubgraphPortType.Gradient:
					DefaultData = JsonSerializer.Deserialize<Gradient>( element, ShaderGraphPlus.SerializerOptions() );
					break;
				case SubgraphPortType.SamplerState:
					DefaultData = JsonSerializer.Deserialize<Sampler>( element, ShaderGraphPlus.SerializerOptions() );
					break;
				case SubgraphPortType.Texture2DObject:
					DefaultData = JsonSerializer.Deserialize<TextureInput>( element, ShaderGraphPlus.SerializerOptions() ) with { Type = TextureType.Tex2D };
					break;
				case SubgraphPortType.TextureCubeObject:
					DefaultData = JsonSerializer.Deserialize<TextureInput>( element, ShaderGraphPlus.SerializerOptions() ) with { Type = TextureType.TexCube };
					break;
				default:
					throw new InvalidOperationException();
			}
		}
	}

	internal T GetValue<T>()
	{
		if ( DefaultData is T value )
		{
			return value;
		}
		else
		{
			if ( DefaultData == null )
			{
				throw new Exception( "DefaultData is null!!!" );
			}

			throw new InvalidCastException( $"Cannot cast \"{DefaultData.GetType()}\" to \"{typeof( T )}\"" );
		}
	}

	internal object GetData()
	{
		if ( DefaultData == null )
		{
			throw new Exception( "DefaultData is null!!!" );
		}

		return DefaultData;
	}

	internal void SetDefaultValue<T>( T value )
	{
		InputType = value switch
		{
			bool => SubgraphPortType.Bool,
			int => SubgraphPortType.Int,
			float => SubgraphPortType.Float,
			Vector2 => SubgraphPortType.Vector2,
			Vector3 => SubgraphPortType.Vector3,
			Vector4 => SubgraphPortType.Vector4,
			Color => SubgraphPortType.Color,
			Float2x2 => SubgraphPortType.Float2x2,
			Float3x3 => SubgraphPortType.Float3x3,
			Float4x4 => SubgraphPortType.Float4x4,
			Gradient => SubgraphPortType.Gradient,
			TextureInput v => v.Type == TextureType.Tex2D ? SubgraphPortType.Texture2DObject : SubgraphPortType.TextureCubeObject,
			_ => throw new NotImplementedException(),
		};
		DefaultData = value;
	}

	[Output, Title( "Value" ), Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		if ( InputType == SubgraphPortType.Texture2DObject || InputType == SubgraphPortType.TextureCubeObject )
		{
			var resultType = ResultType.Invalid;
			var textureInput = GetValue<TextureInput>();
			var textureGlobal = "";
			if ( InputType == SubgraphPortType.Texture2DObject )
			{
				textureInput = textureInput with { Name = InputName, Type = TextureType.Tex2D };
				textureGlobal = compiler.ResultTexture( textureInput, true );
				resultType = ResultType.Texture2DObject;
			}
			else
			{
				textureInput = textureInput with { Name = InputName, Type = TextureType.TexCube };
				textureGlobal = compiler.ResultTexture( textureInput, true );
				resultType = ResultType.TextureCubeObject;
			}

			var result = new NodeResult( resultType, "TextureInput", textureInput );

			SGPLog.Info( $"SubgraphInput.Result // textureGlobal is : {textureGlobal}" );

			result.AddMetadataEntry( "TextureGlobal", textureGlobal );

			return result;
		}
		else
		{
			(ResultType resultType, string defaultCode) defaultResult = InputType switch
			{
				SubgraphPortType.Bool => (ResultType.Bool, $"{compiler.ResultValue( GetValue<bool>() )}"),
				SubgraphPortType.Int => (ResultType.Int, $"{compiler.ResultValue( GetValue<int>() )}"),
				SubgraphPortType.Float => (ResultType.Float, $"{compiler.ResultValue( GetValue<float>() )}"),
				SubgraphPortType.Vector2 => (ResultType.Vector2, $"float2( {compiler.ResultValue( GetValue<Vector2>() )} )"),
				SubgraphPortType.Vector3 => (ResultType.Vector3, $"float3( {compiler.ResultValue( GetValue<Vector3>() )} )"),
				SubgraphPortType.Vector4 => (ResultType.Vector4, $"float4( {compiler.ResultValue( GetValue<Vector4>() )} )"),
				SubgraphPortType.Color => (ResultType.Color, $"float4( {compiler.ResultValue( GetValue<Color>() )} )"),
				SubgraphPortType.Float2x2 => (ResultType.Float2x2, $"float2x2( {compiler.ResultValue( GetValue<Float2x2>() )} )"),
				SubgraphPortType.Float3x3 => (ResultType.Float3x3, $"float3x3( {compiler.ResultValue( GetValue<Float3x3>() )} )"),
				SubgraphPortType.Float4x4 => (ResultType.Float4x4, $"float4x4( {compiler.ResultValue( GetValue<Float4x4>() )} )"),
				SubgraphPortType.Gradient => (ResultType.Gradient, compiler.RegisterGradient( GetValue<Gradient>(), "" )),
				SubgraphPortType.SamplerState => (ResultType.Sampler, $"{compiler.ResultSampler( GetValue<Sampler>() )}"),
				_ => throw new Exception( $"Unknown PortType \"{InputType}\"" )
			};

			//SGPLog.Info( $"defaultResult is : {defaultResult.defaultCode}" );

			return new NodeResult( defaultResult.resultType, defaultResult.defaultCode, constant: true );
		}
	};

	public NodeResult GetResult( GraphCompiler compiler )
	{
		return Result.Invoke( compiler );
	}

	public List<string> GetWarnings()
	{
		var warnings = new List<string>();

		return warnings;
	}

	public List<string> GetErrors()
	{
		List<string> errors = new List<string>();

		if ( string.IsNullOrWhiteSpace( InputName ) )
		{
			errors.Add( $"SubgraphInput of InputType \"{InputType}\" must have a name!" );
		}

		if ( InputType == SubgraphPortType.Invalid )
		{
			errors.Add( $"SubgraphInput InputType is invalid!" );
		}

		return errors;
	}

}
