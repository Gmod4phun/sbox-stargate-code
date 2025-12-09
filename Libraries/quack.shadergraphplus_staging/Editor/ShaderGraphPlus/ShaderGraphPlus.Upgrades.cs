
using Editor.ShaderGraph;
using Facepunch.ActionGraphs;
using ShaderGraphPlus.Nodes;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Text.Json.Nodes;
using static Sandbox.Resources.ResourceGenerator;

namespace ShaderGraphPlus;

public partial class ShaderGraphPlus
{
	private static JsonElement UpgradeJsonUpgradeable( int versionNumber, ISGPJsonUpgradeable jsonUpgradeable, Type type, JsonProperty jsonProperty, JsonSerializerOptions serializerOptions )
	{
		ArgumentNullException.ThrowIfNull( jsonUpgradeable );

		var jsonObject = JsonNode.Parse( jsonProperty.Value.GetRawText() ) as JsonObject;

		SGPJsonUpgrader.Upgrade( versionNumber, jsonObject, type );

		return JsonSerializer.Deserialize<JsonElement>( jsonObject.ToJsonString(), serializerOptions );
	}

	private static JsonElement UpgradeJsonUpgradeable( int versionNumber, ISGPJsonUpgradeable jsonUpgradeable, Type type, JsonElement jsonElement, JsonSerializerOptions serializerOptions )
	{
		ArgumentNullException.ThrowIfNull( jsonUpgradeable );

		var jsonObject = JsonNode.Parse( jsonElement.GetRawText() ) as JsonObject;

		SGPJsonUpgrader.Upgrade( versionNumber, jsonObject, type );

		return JsonSerializer.Deserialize<JsonElement>( jsonObject.ToJsonString(), serializerOptions );
	}

	private void GraphV3Upgrade()
	{
		if ( IsSubgraph )
		{
			foreach ( var subgraphInput in Nodes.OfType<SubgraphInput>() )
			{
				if ( string.IsNullOrWhiteSpace( subgraphInput.InputName ) )
					continue;

				BaseBlackboardParameter blackboardParameter = null;

				if ( subgraphInput.InputType == SubgraphPortType.Bool )
				{
					blackboardParameter = new BoolSubgraphInputParameter()
					{
						Name = subgraphInput.InputName,
						Description = subgraphInput.InputDescription,
						Value = subgraphInput.GetValue<bool>(),
						IsRequired = subgraphInput.IsRequired,
						PortOrder = subgraphInput.PortOrder,
					};
				}
				else if ( subgraphInput.InputType == SubgraphPortType.Int )
				{
					blackboardParameter = new IntSubgraphInputParameter()
					{
						Name = subgraphInput.InputName,
						Description = subgraphInput.InputDescription,
						Value = subgraphInput.GetValue<int>(),
						IsRequired = subgraphInput.IsRequired,
						PortOrder = subgraphInput.PortOrder,
					};
				}
				else if ( subgraphInput.InputType == SubgraphPortType.Float )
				{
					blackboardParameter = new FloatSubgraphInputParameter()
					{
						Name = subgraphInput.InputName,
						Description = subgraphInput.InputDescription,
						Value = subgraphInput.GetValue<float>(),
						IsRequired = subgraphInput.IsRequired,
						PortOrder = subgraphInput.PortOrder,
					};
				}
				else if ( subgraphInput.InputType == SubgraphPortType.Vector2 )
				{
					blackboardParameter = new Float2SubgraphInputParameter()
					{
						Name = subgraphInput.InputName,
						Description = subgraphInput.InputDescription,
						Value = subgraphInput.GetValue<Vector2>(),
						IsRequired = subgraphInput.IsRequired,
						PortOrder = subgraphInput.PortOrder,
					};
				}
				else if ( subgraphInput.InputType == SubgraphPortType.Vector3 )
				{
					blackboardParameter = new Float3SubgraphInputParameter()
					{
						Name = subgraphInput.InputName,
						Description = subgraphInput.InputDescription,
						Value = subgraphInput.GetValue<Vector3>(),
						IsRequired = subgraphInput.IsRequired,
						PortOrder = subgraphInput.PortOrder,
					};
				}
				else if ( subgraphInput.InputType == SubgraphPortType.Color )
				{
					blackboardParameter = new ColorSubgraphInputParameter()
					{
						Name = subgraphInput.InputName,
						Description = subgraphInput.InputDescription,
						Value = subgraphInput.GetValue<Color>(),
						IsRequired = subgraphInput.IsRequired,
						PortOrder = subgraphInput.PortOrder,
					};
				}

				subgraphInput.BlackboardParameterIdentifier = blackboardParameter.Identifier;

				AddParameter( blackboardParameter );
			}
		}
		else
		{
			foreach ( var parameterNode in Nodes.OfType<IParameterNode>() )
			{
				if ( string.IsNullOrWhiteSpace( parameterNode.Name ) )
					continue;

				BaseBlackboardParameter blackboardParameter = null;

				if ( parameterNode is IntParameterNode intNode )
				{
					blackboardParameter = new IntParameter()
					{
						Name = intNode.Name,
						Value = intNode.Value,
						UI = intNode.UI,
						IsAttribute = intNode.IsAttribute,
					};

					intNode.BlackboardParameterIdentifier = blackboardParameter.Identifier;
				}
				else if ( parameterNode is BoolParameterNode boolNode )
				{
					blackboardParameter = new BoolParameter()
					{
						Name = boolNode.Name,
						Value = boolNode.Value,
						UI = boolNode.UI,
						IsAttribute = boolNode.IsAttribute,
					};

					boolNode.BlackboardParameterIdentifier = blackboardParameter.Identifier;
				}
				else if ( parameterNode is FloatParameterNode floatNode )
				{
					blackboardParameter = new FloatParameter()
					{
						Name = floatNode.Name,
						Value = floatNode.Value,
						UI = floatNode.UI,
						IsAttribute = floatNode.IsAttribute,
					};

					floatNode.BlackboardParameterIdentifier = blackboardParameter.Identifier;
				}
				else if ( parameterNode is Float2ParameterNode float2Node )
				{
					blackboardParameter = new Float2Parameter()
					{
						Name = float2Node.Name,
						Value = float2Node.Value,
						UI = float2Node.UI,
						IsAttribute = float2Node.IsAttribute,
					};

					float2Node.BlackboardParameterIdentifier = blackboardParameter.Identifier;

				}
				else if ( parameterNode is Float3ParameterNode float3Node )
				{
					blackboardParameter = new Float3Parameter()
					{
						Name = float3Node.Name,
						Value = float3Node.Value,
						UI = float3Node.UI,
						IsAttribute = float3Node.IsAttribute,
					};

					float3Node.BlackboardParameterIdentifier = blackboardParameter.Identifier;
				}
				else if ( parameterNode is ColorParameterNode colorNode )
				{
					blackboardParameter = new ColorParameter()
					{
						Name = colorNode.Name,
						Value = colorNode.Value,
						UI = colorNode.UI,
						IsAttribute = colorNode.IsAttribute,
					};

					colorNode.BlackboardParameterIdentifier = blackboardParameter.Identifier;
				}

				AddParameter( blackboardParameter );
			}
		}
	}

	private bool ShouldConvertParameterNodeToConstant( string typeName, JsonElement element )
	{
		// Only upgrade if it's a parameter node type
		if ( !IsParameterNodeTypeToConvertToConstant( typeName ) )
			return false;

		// Only convert if it dosent have a name (indicating it's meant to be a constant value)
		if ( element.TryGetProperty( "Name", out var nameProperty ) )
		{
			var name = nameProperty.GetString();

			return string.IsNullOrWhiteSpace( name );
		}

		// No "Name" property? assume its ment to be a constant.
		return true;
	}

	private bool ShouldConvertTextureNodes( string typeName, JsonElement element )
	{
		if ( !IsOldTextureSamplerNodeType( typeName ) )
			return false;

		return true;
	}

	private static bool IsOldTextureSamplerNodeType( string typeName )
	{
		return typeName switch
		{
			"TextureCubeObjectNode" => true,
			"Texture2DObjectNode" => true,
			"TextureSampler" => true,
			"TextureTriplanar" => true,
			"NormalMapTriplanar" => true,
			"TextureCube" => true,
			_ => false
		};
	}

	private static bool IsParameterNodeTypeToConvertToConstant( string typeName )
	{
		return typeName switch
		{
			"BoolParameterNode" => true,
			"IntParameterNode" => true,
			"FloatParameterNode" => true,
			"Float2ParameterNode" => true,
			"Float3ParameterNode" => true,
			"ColorParameterNode" => true,
			_ => false
		};
	}

	private BaseNodePlus InitNewTextureSamplerNode( BaseNodePlus newSamplerNode, JsonElement element,
		out NodeConnectionFixupData connectionFixupData
	)
	{
		connectionFixupData = new();
		if ( newSamplerNode.Graph == null )
		{
			newSamplerNode.Graph = this;
		}

		var imageProperty = element.GetProperty( "Image" );
		var textureUI = new TextureInput();
		if ( newSamplerNode is Texture2DSamplerBase samplerBase )
		{
			textureUI = samplerBase.UI with { DefaultTexture = imageProperty.GetString() };
			samplerBase.InternalImage = imageProperty.GetString();
		}

		if ( !string.IsNullOrWhiteSpace( textureUI.Name ) )
		{
			BaseNodePlus newNode01 = default;
			if ( IsSubgraph )
			{

			}
			else
			{
				newNode01 = new Texture2DParameterNode()
				{
					Position = new Vector2( newSamplerNode.Position.x - 192, newSamplerNode.Position.y )
				};
			}

			BaseBlackboardParameter parameter = default;
			foreach ( var param in Parameters )
			{
				if ( param.Name == textureUI.Name )
				{
					parameter = param;
					break;
				}
			}

			if ( parameter == null )
			{
				if ( IsSubgraph )
				{
					parameter = new Texture2DSubgraphInputParameter()
					{
						Name = textureUI.Name,
						Value = textureUI with { DefaultTexture = imageProperty.GetString() }
					};

					AddParameter( parameter );
				}
				else
				{
					parameter = new Texture2DParameter()
					{
						Name = textureUI.Name,
						Value = textureUI with { DefaultTexture = imageProperty.GetString() }
					};

					AddParameter( parameter );
				}

			}

			if ( IsSubgraph )
			{

			}
			else
			{
				var parameterNode = newNode01 as Texture2DParameterNode;

				parameterNode.BlackboardParameterIdentifier = parameter.Identifier;
				parameterNode.UI = textureUI;

				AddNode( parameterNode );

				newSamplerNode.ConnectNode(
					nameof( SampleTexture2DNode.Texture2DInput ),
					nameof( Texture2DParameterNode.Result ),
					parameterNode.Identifier
				);
			}
		}

		if ( newSamplerNode is Texture2DSamplerBase )
		{
			var connectionsToFix = new Dictionary<NodeInput, string>();
			if ( element.TryGetProperty( "Coords", out var coordsProp ) ) //&& element.TryGetProperty( "Sampler", out var samplerProp ) )
			{
				connectionsToFix.Add( JsonSerializer.Deserialize<NodeInput>( coordsProp, SerializerOptions() ), "CoordsInput" );
			}
			if ( element.TryGetProperty( "Sampler", out var samplerProp ) )
			{
				connectionsToFix.Add( JsonSerializer.Deserialize<NodeInput>( samplerProp, SerializerOptions() ), "SamplerInput" );
			}

			connectionFixupData = new NodeConnectionFixupData( connectionsToFix, newSamplerNode );
		}

		return newSamplerNode;
	}

	private BaseNodePlus ConvertToNewTextureSampleNode( string typeName, JsonElement element, JsonSerializerOptions options, out NodeConnectionFixupData connectionFixupData )
	{
		connectionFixupData = new();

		switch ( typeName )
		{
			case "Texture2DObjectNode":
				BaseNodePlus newNode0 = default;
				var uiProp = element.GetProperty( "UI" );
				var textureInput = JsonSerializer.Deserialize<TextureInput>( uiProp, SerializerOptions() );
				var newUI1 = textureInput with { Name = textureInput.Name, Type = TextureType.Tex2D };
				BaseBlackboardParameter blackboardParameter1 = default;

				if ( IsSubgraph )
				{
					blackboardParameter1 = new Texture2DSubgraphInputParameter()
					{
						Name = textureInput.Name,
						Value = newUI1
					};
				}
				else
				{
					blackboardParameter1 = new Texture2DParameter()
					{
						Name = textureInput.Name,
						Value = newUI1
					};
				}

				if ( IsSubgraph )
				{
					newNode0 = new SubgraphInput()
					{
						BlackboardParameterIdentifier = blackboardParameter1.Identifier,
						InputName = newUI1.Name,
						InputType = SubgraphPortType.Texture2DObject,
						DefaultData = textureInput
					};
				}
				else
				{
					var tex2DParamNode = new Texture2DParameterNode();

					tex2DParamNode.BlackboardParameterIdentifier = blackboardParameter1.Identifier;
					tex2DParamNode.UI = newUI1;
					AddParameter( blackboardParameter1 );

					newNode0 = tex2DParamNode;
				}

				// Copy basic node properties
				DeserializeObject( newNode0, element, options );

				return newNode0;
			case "TextureCubeObjectNode":
				var newNode1 = new TextureCubeParameterNode();

				// Copy basic node properties
				DeserializeObject( newNode1, element, options );

				var newUI2 = newNode1.UI with { Name = newNode1.UI.Name, Type = TextureType.TexCube };

				var blackboardParameter2 = new TextureCubeParameter()
				{
					Name = newNode1.UI.Name,
					Value = newUI2
				};

				newNode1.BlackboardParameterIdentifier = blackboardParameter2.Identifier;
				newNode1.UI = newUI2;

				AddParameter( blackboardParameter2 );

				return newNode1;
			case "TextureSampler":
				var newNode2 = new SampleTexture2DNode();

				// Copy basic node properties
				DeserializeObject( newNode2, element, options );

				return InitNewTextureSamplerNode( newNode2, element, out connectionFixupData );
			case "TextureTriplanar":
				var newNode3 = new SampleTexture2DTriplanarNode();

				// Copy basic node properties
				DeserializeObject( newNode3, element, options );

				return InitNewTextureSamplerNode( newNode3, element, out connectionFixupData );
			case "NormalMapTriplanar":
				var newNode4 = new SampleTexture2DNormalMapTriplanarNode();

				// Copy basic node properties
				DeserializeObject( newNode4, element, options );

				return InitNewTextureSamplerNode( newNode4, element, out connectionFixupData );
			case "TextureCube":
				var newNode5 = new SampleTextureCubeNode();

				// Copy basic node properties
				DeserializeObject( newNode5, element, options );

				if ( newNode5.Graph == null )
				{
					newNode5.Graph = this;
				}

				var textureProperty = element.GetProperty( "Texture" );
				var textureUI = newNode5.UI with { DefaultTexture = textureProperty.GetString(), Type = TextureType.TexCube };
				newNode5.Texture = textureProperty.GetString();

				return newNode5;
		}

		throw new Exception( $"Could not convert \"{typeName}\" to new TextureSampler node!" );
	}

	private BaseNodePlus ConvertToConstantNode( string typeName, JsonElement element, JsonSerializerOptions options )
	{
		if ( element.TryGetProperty( "Value", out var parameterValueElement ) )
		{
			// Map the parameter type to InputType and set default values
			switch ( typeName )
			{
				case "BoolParameterNode":
					var newNode1 = new BoolConstantNode()
					{
						Value = parameterValueElement.GetBoolean()
					};

					// Copy basic node properties
					DeserializeObject( newNode1, element, options );
					return newNode1;
				case "IntParameterNode":
					var newNode2 = new IntConstantNode()
					{
						Value = parameterValueElement.GetInt32()
					};

					// Copy basic node properties
					DeserializeObject( newNode2, element, options );
					return newNode2;
				case "FloatParameterNode":
					var newNode3 = new FloatConstantNode()
					{
						Value = parameterValueElement.GetSingle()
					};

					// Copy basic node properties
					DeserializeObject( newNode3, element, options );
					return newNode3;
				case "Float2ParameterNode":
					var vector2 = JsonSerializer.Deserialize<Vector2>( parameterValueElement.GetRawText(), options );
					var newNode4 = new Float2ConstantNode()
					{
						Value = vector2
					};

					// Copy basic node properties
					DeserializeObject( newNode4, element, options );
					return newNode4;
				case "Float3ParameterNode":
					var vector3 = JsonSerializer.Deserialize<Vector3>( parameterValueElement.GetRawText(), options );
					var newNode5 = new Float3ConstantNode()
					{
						Value = vector3
					};

					// Copy basic node properties
					DeserializeObject( newNode5, element, options );
					return newNode5;
				case "ColorParameterNode":
					var color = JsonSerializer.Deserialize<Color>( parameterValueElement.GetRawText(), options );
					var newNode6 = new ColorConstantNode()
					{
						Value = color
					};

					// Copy basic node properties
					DeserializeObject( newNode6, element, options );

					return newNode6;
			}

			throw new Exception( "Couldnt convert nameless Parameter node to Constant node" );
		}

		throw new Exception( "Couldnt convert nameless Parameter node to Constant node" );
	}

	/// <summary>
	/// Handle any potential node upgrades from one graph version to another.
	/// </summary>
	/// <param name="typeName"></param>
	/// <param name="nodeElement"></param>
	/// <param name="graphFileVersion"></param>
	/// <param name="options"></param>
	/// <param name="upgradedNode"></param>
	/// <param name="connectionFixups"></param>
	/// <returns></returns>
	private bool HandleGraphNodeUpgrade( string typeName, JsonElement nodeElement, int graphFileVersion, JsonSerializerOptions options, out BaseNodePlus upgradedNode, out List<NodeConnectionFixupData> connectionFixups )
	{
		upgradedNode = null;
		connectionFixups = new();

		if ( graphFileVersion < 3 && ShouldConvertParameterNodeToConstant( typeName, nodeElement ) )
		{
			SGPLog.Info( $"Converting Unnamed Parameter node {typeName} to a constant node.", ConCommands.VerboseSerialization );

			upgradedNode = ConvertToConstantNode( typeName, nodeElement, options );

			return true;
		}
		else if ( graphFileVersion < 3 && ShouldConvertTextureNodes( typeName, nodeElement ) )
		{
			upgradedNode = ConvertToNewTextureSampleNode( typeName, nodeElement, options, out var connectionFixupDataNew );

			connectionFixups.Add( connectionFixupDataNew );

			return true;
		}

		/*
		if ( graphFileVersion < 4 )
		{

			return true;
		}
		else if ( IsSubgraph && graphFileVersion < 4 )
		{

			return true;
		}
		*/


		return false;
	}

	private static int GetVersion( JsonElement element )
	{
		if ( element.TryGetProperty( VersioningInfo.JsonPropertyName, out var versionElement ) )
		{
			return versionElement.GetInt32();
		}

		SGPLog.Error( $"JsonElement has no property named \"{VersioningInfo.JsonPropertyName}\". Defaulting to 0...." );

		return 0;
	}
}
