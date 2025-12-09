using ShaderGraphPlus.Nodes;
using System.Reflection.Metadata;
using System.Text.Json.Nodes;

namespace ShaderGraphPlus;

public interface ISGPJsonUpgradeable
{
	[Hide]
	public int Version { get; }
}

/// <summary>
/// Data that helps us fixup and reconnect broken node connections when updating nodes.
/// </summary>
struct NodeConnectionFixupData
{
	public Dictionary<NodeInput, string> NodeInputs;
	public BaseNodePlus NodeToConnectTo;

	public NodeConnectionFixupData( Dictionary<NodeInput, string> mapping0, BaseNodePlus nodeToConnectTo )
	{
		NodeInputs = mapping0;
		NodeToConnectTo = nodeToConnectTo;
	}

}

partial class ShaderGraphPlus
{
	internal static class VersioningInfo
	{
		/// <summary>
		/// Json Property name of the version number.
		/// </summary>
		internal const string JsonPropertyName = "__version";

		internal const string VersionClassPropertyName = "Version";

		internal const string ShaderGraphReleaseVersionDate = "";
	}

	internal static JsonSerializerOptions SerializerOptions( bool indented = false )
	{
		var options = new JsonSerializerOptions
		{
			WriteIndented = indented,
			PropertyNameCaseInsensitive = true,
			NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
			DefaultIgnoreCondition = JsonIgnoreCondition.Never,
			ReadCommentHandling = JsonCommentHandling.Skip,
		};

		options.Converters.Add( new JsonStringEnumConverter( null, true ) );

		return options;
	}

	public string Serialize()
	{
		var doc = new JsonObject();
		var options = SerializerOptions( true );

		SerializeObject( this, doc, options );
		SerializeNodes( Nodes, doc, options );
		SerializeParameters( Parameters, doc, options );

		return doc.ToJsonString( options );
	}

	public void Deserialize( string json, string subgraphPath = null, string fileName = "" )
	{
		using var doc = JsonDocument.Parse( json );
		var root = doc.RootElement;
		var options = SerializerOptions();

		// Get the version so we can handle upgrades
		var graphFileVersion = GetVersion( root );

		SGPLog.Info( $"Deserializing graph \"{fileName}\" version \"{graphFileVersion}\"", ConCommands.VerboseSerialization );

		DeserializeObject( this, root, options );
		DeserializeNodes( root, options, subgraphPath, graphFileVersion );
		DeserializeParameters( root, options );

		if ( graphFileVersion < 3 )
		{
			GraphV3Upgrade();
		}
	}

	public IEnumerable<BaseNodePlus> DeserializeNodes( string json )
	{
		using var doc = JsonDocument.Parse( json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip } );
		var root = doc.RootElement;

		// Check for version in the JSON
		var graphFileVersion = GetVersion( root );

		return DeserializeNodes( root, SerializerOptions(), null, graphFileVersion );
	}

	public IEnumerable<BaseBlackboardParameter> DeserializeParameters( string json )
	{
		using var doc = JsonDocument.Parse( json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip } );
		var root = doc.RootElement;

		return DeserializeParameters( root, SerializerOptions() );
	}

	private IEnumerable<BaseBlackboardParameter> DeserializeParameters( JsonElement doc, JsonSerializerOptions options )
	{
		var parameters = new Dictionary<string, BaseBlackboardParameter>();

		if ( doc.TryGetProperty( "parameters", out var arrayProperty ) )
		{
			foreach ( var element in arrayProperty.EnumerateArray() )
			{
				var typeName = element.GetProperty( "_class" ).GetString();
				var typeDesc = EditorTypeLibrary.GetType( typeName );
				var type = new ClassParameterType( typeDesc );

				BaseBlackboardParameter parameter;

				if ( typeDesc != null )
				{
					parameter = EditorTypeLibrary.Create<BaseBlackboardParameter>( typeName );
					DeserializeObject( parameter, element, options );

					//SGPLog.Info( $"parameter.Name == {parameter.Name}" );

					if ( string.IsNullOrWhiteSpace( parameter.Name ) )
					{
						parameter.Name = $"parameter{parameters.Count}";
					}

					if ( parameter is ColorParameter bp )
					{
						bp.UI = bp.UI with { ShowTypeProperty = false };
					}

					parameters.Add( parameter.Name, parameter );

					AddParameter( parameter );
				}
			}
		}

		return parameters.Values;
	}

	private static void DeserializeObject( object obj, JsonElement doc, JsonSerializerOptions options )
	{
		var type = obj.GetType();
		var properties = type.GetProperties( BindingFlags.Instance | BindingFlags.Public )
			.Where( x => x.GetSetMethod() != null );

		// Check if we need to upgrade the core graph.
		if ( obj is ShaderGraphPlus )
		{
			if ( typeof( ISGPJsonUpgradeable ).IsAssignableFrom( type ) )
			{
				var propertyTypeInstance = EditorTypeLibrary.Create( type.Name, type );
				int oldVersionNumber = 0;

				// if we have a valid version then set oldVersionNumber otherwise just use a version of 0.
				if ( doc.TryGetProperty( VersioningInfo.JsonPropertyName, out var versionElement ) )
				{
					oldVersionNumber = versionElement.GetInt32();
					SGPLog.Info( $"Got graph \"{type}\" upgradeable version \"{oldVersionNumber}\"", ConCommands.VerboseJsonUpgrader );
				}
				else
				{
					SGPLog.Warning( $"Failed to get graph \"{type}\" upgradeable version. defaulting to \"0\"", ConCommands.VerboseJsonUpgrader );
				}

				// Dont even bother upgrading if we dont need to.
				if ( propertyTypeInstance is ISGPJsonUpgradeable upgradeable && oldVersionNumber < upgradeable.Version )
				{
					SGPLog.Info( $"Upgrading grapg \"{type}\" from version \"{oldVersionNumber}\" to \"{upgradeable.Version}\"", ConCommands.VerboseJsonUpgrader );

					var upgradedElement = UpgradeJsonUpgradeable( oldVersionNumber, upgradeable, type, doc, options );

					doc = upgradedElement;
				}
				else
				{
					SGPLog.Info( $"Graph \"{type}\" is already at the latest version :)", ConCommands.VerboseJsonUpgrader );
				}
			}
		}

		// Check if we need to upgrade any nodes :).
		if ( type.IsAssignableTo( typeof( BaseNodePlus ) ) )
		{
			if ( typeof( ISGPJsonUpgradeable ).IsAssignableFrom( type ) )
			{
				var propertyTypeInstance = EditorTypeLibrary.Create( type.Name, type );
				int oldVersionNumber = 0;

				// if we have a valid version then set oldVersionNumber otherwise just use a version of 0.
				if ( doc.TryGetProperty( VersioningInfo.JsonPropertyName, out var versionElement ) )
				{
					oldVersionNumber = versionElement.GetInt32();
					SGPLog.Info( $"Got node \"{type}\" upgradeable version \"{oldVersionNumber}\"", ConCommands.VerboseJsonUpgrader );
				}
				else
				{
					SGPLog.Warning( $"Failed to get node \"{type}\" upgradeable version. defaulting to \"0\"", ConCommands.VerboseJsonUpgrader );
				}

				// Dont even bother upgrading if we dont need to.
				if ( propertyTypeInstance is ISGPJsonUpgradeable upgradeable && oldVersionNumber < upgradeable.Version )
				{
					SGPLog.Info( $"Upgrading node \"{type}\" from version \"{oldVersionNumber}\" to \"{upgradeable.Version}\"", ConCommands.VerboseJsonUpgrader );

					var upgradedElement = UpgradeJsonUpgradeable( oldVersionNumber, upgradeable, type, doc, options );

					doc = upgradedElement;
				}
				else
				{
					SGPLog.Info( $"Node \"{type}\" is already at the latest version :)", ConCommands.VerboseJsonUpgrader );
				}
			}
		}

		// start deserilzing each property of the current type we are deserialzing. Also handle 
		// any property that needs upgrading.
		foreach ( var jsonProperty in doc.EnumerateObject() )
		{
			var propertyInfo = properties.FirstOrDefault( x =>
			{
				var propName = x.Name;


				if ( x.GetCustomAttribute<JsonPropertyNameAttribute>() is JsonPropertyNameAttribute jpna )
					propName = jpna.Name;

				return string.Equals( propName, jsonProperty.Name, StringComparison.OrdinalIgnoreCase );
			} );

			if ( propertyInfo == null )
				continue;

			if ( propertyInfo.CanWrite == false )
				continue;

			if ( propertyInfo.IsDefined( typeof( JsonIgnoreAttribute ) ) )
				continue;

			// Handle any types that use the ISGPJsonUpgradeable interface
			if ( typeof( ISGPJsonUpgradeable ).IsAssignableFrom( propertyInfo.PropertyType ) )
			{
				var propertyTypeInstance = EditorTypeLibrary.Create( propertyInfo.PropertyType.Name, propertyInfo.PropertyType );
				int oldVersionNumber = 0;

				// if we have a valid version then set oldVersionNumber otherwise just use a version of 0.
				if ( jsonProperty.Value.TryGetProperty( VersioningInfo.JsonPropertyName, out var versionElement ) )
				{
					oldVersionNumber = versionElement.GetInt32();
					SGPLog.Info( $"Got property \"{type}\" upgradeable version \"{oldVersionNumber}\"", ConCommands.VerboseJsonUpgrader );
				}
				else
				{
					SGPLog.Info( $"Failed to get property \"{type}\" upgradeable version. defaulting to \"0\"", ConCommands.VerboseJsonUpgrader );
				}

				// Dont even bother upgrading if we dont need to.
				if ( propertyTypeInstance is ISGPJsonUpgradeable upgradeable && oldVersionNumber < upgradeable.Version )
				{
					SGPLog.Info( $"Upgrading property \"{type}\" from version \"{oldVersionNumber}\" to \"{upgradeable.Version}\"", ConCommands.VerboseJsonUpgrader );

					var upgradedElement = UpgradeJsonUpgradeable( oldVersionNumber, upgradeable, propertyInfo.PropertyType, jsonProperty, options );

					propertyInfo.SetValue( obj, JsonSerializer.Deserialize( upgradedElement.GetRawText(), propertyInfo.PropertyType, options ) );

					// Continue to the next jsonProperty :)
					continue;
				}
				else
				{
					SGPLog.Info( $"property \"{propertyInfo.PropertyType}\" is already at the latest version :)", ConCommands.VerboseJsonUpgrader );
				}
			}

			propertyInfo.SetValue( obj, JsonSerializer.Deserialize( jsonProperty.Value.GetRawText(), propertyInfo.PropertyType, options ) );
		}
	}

	private IEnumerable<BaseNodePlus> DeserializeNodes( JsonElement doc, JsonSerializerOptions options, string subgraphPath = null, int graphFileVersion = -1 )
	{
		var nodes = new Dictionary<string, BaseNodePlus>();
		var identifiers = _nodes.Count > 0 ? new Dictionary<string, string>() : null;
		var connections = new List<(IPlugIn Plug, NodeInput Value)>();
		var connectionFixups = new List<NodeConnectionFixupData>();

		var arrayProperty = doc.GetProperty( "nodes" );
		foreach ( var element in arrayProperty.EnumerateArray() )
		{
			var typeName = element.GetProperty( "_class" ).GetString();

			// Use the new typename if applicable.
			if ( ProjectUpgrading.NodeTypeNameMapping.TryGetValue( typeName, out string newTypeName ) )
			{
				typeName = newTypeName;
			}

			var typeDesc = EditorTypeLibrary.GetType( typeName );
			var type = new ClassNodeType( typeDesc );

			BaseNodePlus node;
			if ( typeDesc is null )
			{
				SGPLog.Error( $"Missing Node : \"{typeName}\"" );
				var missingNode = new MissingNode( typeName, element );
				node = missingNode;
				DeserializeObject( node, element, options );

				nodes.Add( node.Identifier, node );

				AddNode( node );
			}
			else
			{
				if ( HandleGraphNodeUpgrade( typeName, element, graphFileVersion, options, out var upgradedNode, out var newConnectionFixups ) )
				{
					node = upgradedNode;

					if ( newConnectionFixups.Any() )
					{
						connectionFixups.AddRange( newConnectionFixups );
					}
				}
				else // Nothing to upgrade
				{
					SGPLog.Info( $"Deserializing node \"{typeName}\" version \"{GetVersion( element )}\"", ConCommands.VerboseSerialization );

					node = EditorTypeLibrary.Create<BaseNodePlus>( typeName );
					DeserializeObject( node, element, options );
				}

				if ( identifiers != null && _nodes.ContainsKey( node.Identifier ) )
				{
					identifiers.Add( node.Identifier, node.NewIdentifier() );
				}

				if ( node is IInitializeNode initializeableNode )
				{
					initializeableNode.InitializeNode();
				}

				if ( node is SubgraphNode subgraphNode )
				{
					if ( !Editor.FileSystem.Content.FileExists( subgraphNode.SubgraphPath ) )
					{
						var missingNode = new MissingNode( typeName, element );
						node = missingNode;
						DeserializeObject( node, element, options );
					}
					else
					{
						subgraphNode.OnNodeCreated();
					}
				}

				foreach ( var input in node.Inputs )
				{
					if ( !element.TryGetProperty( input.Identifier, out var connectedElem ) )
						continue;

					var connected = connectedElem
						.Deserialize<NodeInput?>();

					if ( connected is { IsValid: true } )
					{
						var connection = connected.Value;
						if ( !string.IsNullOrEmpty( subgraphPath ) )
						{
							connection = new()
							{
								Identifier = connection.Identifier,
								Output = connection.Output,
								Subgraph = subgraphPath
							};
						}

						connections.Add( (input, connection) );
					}
				}

				if ( !node.UpgradedToNewNode )
				{
					nodes.Add( node.Identifier, node );

					AddNode( node );
				}
			}
		}

		foreach ( var (input, value) in connections )
		{
			var outputIdent = identifiers?.TryGetValue( value.Identifier, out var newIdent ) ?? false
				? newIdent : value.Identifier;

			if ( nodes.TryGetValue( outputIdent, out var node ) )
			{
				var output = node.Outputs.FirstOrDefault( x => x.Identifier == value.Output );

				// Uprgraded node but the NodeResult property name on the new node output differs from the old one.
				// Bit of a hack really.
				//if ( replacedNodes.TryGetValue( node.Identifier, out var oldNode ) && output is null )
				//{
				//	ProjectUpgrading.ReplaceOutputReference( node, oldNode, value.Output, ref output );
				//}

				if ( output is null )
				{
					// Check for Aliases
					foreach ( var op in node.Outputs )
					{
						if ( op is not BasePlugOut plugOut ) continue;

						var aliasAttr = plugOut.Info.Property?.GetCustomAttribute<AliasAttribute>();
						if ( aliasAttr is not null && aliasAttr.Value.Contains( value.Output ) )
						{
							output = plugOut;
							break;
						}
					}
				}
				input.ConnectedOutput = output;
			}
		}

		// Fixup any broken connections for any node that was "upgraded".
		// Though in some cases it may not work when the node we are
		// connecting from has itself been "upgraded".
		foreach ( var data in connectionFixups )
		{
			if ( data.NodeInputs == null )
				continue;

			foreach ( var mapping in data.NodeInputs )
			{
				var nodeFrom = FindNode( mapping.Key.Identifier );

				if ( nodeFrom != null )
				{
					SGPLog.Warning( $"Trying to connect \"{mapping.Key.Output}\" from node \"{nodeFrom}\" to \"{mapping.Value}\"" );

					data.NodeToConnectTo.Graph = this;
					data.NodeToConnectTo.ConnectNode( mapping.Value, mapping.Key.Output, mapping.Key.Identifier );
				}
				else
				{
					SGPLog.Error( $"Could not find node with identifier \"{mapping.Key.Identifier}\"" );
				}
			}
		}

		return nodes.Values;
	}

	public string SerializeNodes()
	{
		return SerializeNodes( Nodes );
	}

	public string UndoStackSerialize()
	{
		var doc = new JsonObject();
		var options = SerializerOptions();

		doc = SerializeNodes( Nodes, doc );

		return SerializeParameters( Parameters, doc ).ToJsonString( options );
	}

	public string SerializeNodes( IEnumerable<BaseNodePlus> nodes )
	{
		var doc = new JsonObject();
		var options = SerializerOptions();

		SerializeNodes( nodes, doc, options );

		return doc.ToJsonString( options );
	}

	public JsonObject SerializeNodes( IEnumerable<BaseNodePlus> nodes, JsonObject doc )
	{
		var options = SerializerOptions();

		SerializeNodes( nodes, doc, options );

		return doc;
	}

	private static void SerializeObject( object obj, JsonObject doc, JsonSerializerOptions options, Dictionary<string, string> identifiers = null )
	{
		var type = obj.GetType();
		var properties = type.GetProperties( BindingFlags.Instance | BindingFlags.Public )
			.Where( x => x.GetSetMethod() != null );

		if ( obj is ShaderGraphPlus sgp && sgp is ISGPJsonUpgradeable iupgradeable )
		{
			doc.Add( VersioningInfo.JsonPropertyName, JsonSerializer.SerializeToNode( iupgradeable.Version, options ) );
		}

		foreach ( var property in properties )
		{
			if ( !property.CanRead )
				continue;

			if ( property.PropertyType == typeof( NodeInput ) )
				continue;

			if ( property.IsDefined( typeof( JsonIgnoreAttribute ) ) )
				continue;

			var propertyName = property.Name;
			if ( property.GetCustomAttribute<JsonPropertyNameAttribute>() is { } jpna )
				propertyName = jpna.Name;

			var propertyValue = property.GetValue( obj );
			if ( propertyName == "Identifier" && propertyValue is string identifier )
			{
				if ( identifiers.TryGetValue( identifier, out var newIdentifier ) )
				{
					propertyValue = newIdentifier;
				}
			}

			if ( propertyName != "Version" )
			{
				doc.Add( propertyName, JsonSerializer.SerializeToNode( propertyValue, options ) );
			}

			if ( propertyValue is ISGPJsonUpgradeable upgradeable )
			{
				//doc.Add( VersioningInfo.VersionJsonPropertyName, JsonSerializer.SerializeToNode( upgradeable.Version, options ) );
			}
		}

		if ( obj is IGraphNode node )
		{
			foreach ( var input in node.Inputs )
			{
				if ( input.ConnectedOutput is not { } output )
					continue;

				doc.Add( input.Identifier, JsonSerializer.SerializeToNode( new NodeInput
				{
					Identifier = identifiers?.TryGetValue( output.Node.Identifier, out var newIdent ) ?? false ? newIdent : output.Node.Identifier,
					Output = output.Identifier,
				} ) );
			}
		}
	}

	private static void SerializeNodes( IEnumerable<BaseNodePlus> nodes, JsonObject doc, JsonSerializerOptions options )
	{
		var identifiers = new Dictionary<string, string>();
		foreach ( var node in nodes )
		{
			identifiers.Add( node.Identifier, $"{identifiers.Count}" );
		}

		var nodeArray = new JsonArray();

		foreach ( var node in nodes )
		{
			if ( node is DummyNode )
				continue;

			var type = node.GetType();
			var nodeObject = new JsonObject { { "_class", type.Name }, { VersioningInfo.JsonPropertyName, node.Version } };

			//if ( node is ISGPJsonUpgradeable upgradeable )
			//{
			//	nodeObject.Add( VersioningInfo.JsonPropertyName, upgradeable.Version );
			//}

			SerializeObject( node, nodeObject, options, identifiers );

			nodeArray.Add( nodeObject );
		}

		doc.Add( "nodes", nodeArray );
	}

	public string SerializeParameters()
	{
		return SerializeParameters( Parameters );
	}

	private string SerializeParameters( IEnumerable<BaseBlackboardParameter> parameters )
	{
		var doc = new JsonObject();
		var options = SerializerOptions();

		SerializeParameters( parameters, doc, options );

		return doc.ToJsonString( options );
	}

	private JsonObject SerializeParameters( IEnumerable<BaseBlackboardParameter> parameters, JsonObject doc )
	{
		var options = SerializerOptions();

		SerializeParameters( parameters, doc, options );

		return doc;
	}

	private static void SerializeParameters( IEnumerable<BaseBlackboardParameter> parameters, JsonObject doc, JsonSerializerOptions options )
	{
		var parameterArray = new JsonArray();

		foreach ( var parameter in parameters )
		{
			var type = parameter.GetType();
			var parameterObject = new JsonObject { { "_class", type.Name } };

			SerializeObject( parameter, parameterObject, options );

			parameterArray.Add( parameterObject );
		}

		doc.Add( "parameters", parameterArray );
	}

}
