using VanillaGraph = Editor.ShaderGraph;
using VanillaNodes = Editor.ShaderGraph.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;

namespace ShaderGraphPlus.Internal;

/// <summary>
/// In charge of converting a ShaderGraph project into a ShaderGraphPlus project. Use Convert() to convert a project and get the result.
/// </summary>
internal class ProjectConverter
{
	private VanillaGraph.ShaderGraph ShaderGraph { get; }
	private ShaderGraphPlus ShaderGraphPlus { get; }
	private bool IsSubgraph { get; }

	private Dictionary<Type, BaseNodeConvert> RegisterdNodes { get; set; }
	private List<ConnectionData> ShaderGraphConnections { get; set; }

	private string FunctionResultID { get; set; } = "";
	private List<(string OldFuncResultID, string outputName)> SubgraphOutputIds { get; set; }

	public List<string> ReservedIds { get; set; } = new();

	public bool Errored { get; private set; } = false;

	public List<NodeConnectionFixupData> ConnectionFixupDatas { get; set; } = new();

	internal ProjectConverter( VanillaGraph.ShaderGraph shaderGraph, ShaderGraphPlus shaderGraphPlus, bool isSubgraph = false )
	{
		ShaderGraph = shaderGraph;
		ShaderGraphPlus = shaderGraphPlus;
		IsSubgraph = isSubgraph;

		if ( IsSubgraph )
		{
			FunctionResultID = ShaderGraph.Nodes.OfType<VanillaGraph.FunctionResult>().FirstOrDefault().Identifier;
			SubgraphOutputIds = new();
		}

		ReservedIds = new();
		ShaderGraphConnections = new();

		RegisterConverters();
	}


	internal ShaderGraphPlus Convert()
	{
		ConvertProjectRoot();

		if ( !Errored )
		{
			ConvertNodes();
			CreateConnections();
			ConnectNodesTest();

			return ShaderGraphPlus;
		}

		return ShaderGraphPlus;
	}

	private void RegisterConverters()
	{
		RegisterdNodes = new();

		var converters = EditorTypeLibrary.GetTypes<BaseNodeConvert>().Where( x => !x.IsAbstract );
		foreach ( var convert in converters )
		{
			var instance = EditorTypeLibrary.Create( convert.Name, convert.TargetType );
			if ( instance != null && instance is BaseNodeConvert baseNodeConvert )
			{
				RegisterdNodes.Add( baseNodeConvert.NodeTypeToConvert, baseNodeConvert );
			}
		}
	}

	private void ConvertProjectRoot()
	{
		if ( !SupportedVersions.Contains( ShaderGraph.Version ) )
		{
			SGPLog.Error( $"ShaderGraph version \"{ShaderGraph.Version}\" is unsupported" );
			Errored = true;
			return;
		}

		ShaderGraphPlus.IsSubgraph = ShaderGraph.IsSubgraph;
		ShaderGraphPlus.Path = ShaderGraph.Path.Replace( !IsSubgraph ? ".shdrgrph" : ".shdrfunc", !IsSubgraph ? $".{ShaderGraphPlusGlobals.AssetTypeExtension}" : $".{ShaderGraphPlusGlobals.SubgraphAssetTypeExtension}" );
		ShaderGraphPlus.Model = ShaderGraph.Model;
		ShaderGraphPlus.Description = ShaderGraph.Description;

		// Subgraph properties
		ShaderGraphPlus.Title = ShaderGraph.Title;
		ShaderGraphPlus.Description = ShaderGraph.Description;
		ShaderGraphPlus.Category = ShaderGraph.Category;
		ShaderGraphPlus.Icon = ShaderGraph.Icon;
		ShaderGraphPlus.AddToNodeLibrary = ShaderGraph.AddToNodeLibrary;

		switch ( ShaderGraph.BlendMode )
		{
			case Editor.ShaderGraph.BlendMode.Opaque:
				ShaderGraphPlus.BlendMode = BlendMode.Opaque;
				break;
			case Editor.ShaderGraph.BlendMode.Masked:
				ShaderGraphPlus.BlendMode = BlendMode.Masked;
				break;
			case Editor.ShaderGraph.BlendMode.Translucent:
				ShaderGraphPlus.BlendMode = BlendMode.Translucent;
				break;
		}

		switch ( ShaderGraph.ShadingModel )
		{
			case Editor.ShaderGraph.ShadingModel.Lit:
				ShaderGraphPlus.ShadingModel = ShadingModel.Lit;
				break;
			case Editor.ShaderGraph.ShadingModel.Unlit:
				ShaderGraphPlus.ShadingModel = ShadingModel.Unlit;
				break;
		}

		switch ( ShaderGraph.Domain )
		{
			case Editor.ShaderGraph.ShaderDomain.Surface:
				ShaderGraphPlus.Domain = ShaderDomain.Surface;
				break;
			case Editor.ShaderGraph.ShaderDomain.PostProcess:
				ShaderGraphPlus.Domain = ShaderDomain.PostProcess;
				break;
		}

		ShaderGraphPlus.PreviewSettings.ShowGround = ShaderGraph.PreviewSettings.ShowGround;
		ShaderGraphPlus.PreviewSettings.ShowSkybox = ShaderGraph.PreviewSettings.ShowSkybox;
		ShaderGraphPlus.PreviewSettings.EnableShadows = ShaderGraph.PreviewSettings.EnableShadows;
		ShaderGraphPlus.PreviewSettings.BackgroundColor = ShaderGraph.PreviewSettings.BackgroundColor;
		ShaderGraphPlus.PreviewSettings.Tint = ShaderGraph.PreviewSettings.Tint;
	}

	private void ConvertNodes()
	{
		var convertedNodes = new Dictionary<string, BaseNodePlus>();
		var subgraphOutputNames = new List<string>();


		foreach ( var vanillaNode in ShaderGraph.Nodes )
		{
			if ( RegisterdNodes.TryGetValue( vanillaNode.GetType(), out var nodeConvert ) )
			{
				var newConvertedNodes = nodeConvert.Convert( this, vanillaNode );
				var connections = GetConnections( vanillaNode, nodeConvert.GetNodeInputNameMappings() );

				foreach ( var convertedNode in newConvertedNodes )
				{
					if ( convertedNode is not SubgraphOutput && !ReservedIds.Contains( convertedNode.Identifier ) )
					{
						ReservedIds.Add( convertedNode.Identifier );
					}

					convertedNodes.Add( convertedNode.Identifier, convertedNode );
				}

				if ( vanillaNode is not VanillaGraph.FunctionResult )
				{
					ShaderGraphConnections.AddRange( connections );
				}
			}
			else
			{
				throw new Exception( $"Node type \"{vanillaNode.GetType()}\" does not have an associated NodeConvert class" );
			}
		}

		// Add the converted nodes to the new graph.
		foreach ( var convertedNode in convertedNodes.Values )
		{
			ShaderGraphPlus.AddNode( convertedNode );
		}
	}

	private void CreateConnections()
	{
		if ( IsSubgraph && !string.IsNullOrWhiteSpace( FunctionResultID ) )
		{
			var functionResultNode = ShaderGraph.FindNode( FunctionResultID );

			if ( functionResultNode != null )
			{
				foreach ( var input in functionResultNode.Inputs )
				{
					if ( input.ConnectedOutput is not { } output )
						continue;

					if ( !string.IsNullOrWhiteSpace( output.Identifier ) )
					{
						var subgraphOutputName = SubgraphOutputIds.Where( x => x.outputName == input.Identifier ).FirstOrDefault();

						var nodeToConnectTo = ShaderGraphPlus.Nodes.OfType<SubgraphOutput>().Where( x => x.OutputName == subgraphOutputName.outputName ).FirstOrDefault();
						if ( nodeToConnectTo != null )
						{
							nodeToConnectTo.ConnectNode( subgraphOutputName.outputName, output.Identifier, output.Node.Identifier );
						}
					}
				}
			}
		}

		foreach ( var connectionData in ShaderGraphConnections )
		{
			var nodeToConnectTo = ShaderGraphPlus.Nodes.Where( x => x.Identifier == connectionData.InputNodeIdentifier ).FirstOrDefault();
			if ( nodeToConnectTo != null )
			{
				nodeToConnectTo.ConnectNode( connectionData.InputName, connectionData.OutputName, connectionData.OutputNodeIdentifier );
			}
		}
	}

	private List<ConnectionData> GetConnections( ShaderGraphBaseNode vanillaNode, Dictionary<string, string> mappings )
	{
		List<ConnectionData> connectionsData = new();

		foreach ( var input in vanillaNode.Inputs )
		{
			if ( input.ConnectedOutput is not { } output )
				continue;

			var nodeInput = new VanillaGraph.NodeInput()
			{
				Identifier = output.Node.Identifier,
				Output = output.Identifier,
			};

			if ( nodeInput is { IsValid: true } )
			{
				if ( vanillaNode is VanillaGraph.SubgraphNode subgraphNode && !string.IsNullOrEmpty( subgraphNode.SubgraphPath ) )
				{
					nodeInput = nodeInput with { Subgraph = subgraphNode.SubgraphPath };
				}

				var connectionData = new ConnectionData()
				{
					InputName = input.Identifier,
					InputType = input.Type,
					InputNodeIdentifier = vanillaNode.Identifier,
					OutputName = nodeInput.Output,
					OutputType = output.Type,
					OutputNodeIdentifier = nodeInput.Identifier,
				};

				// Change the InputName in the case where the input.Identifier is differnt from ShaderGraph to ShaderGraphPlus.
				if ( mappings.TryGetValue( connectionData.InputName, out var newInputName ) )
				{
					//SGPLog.Info( $"Changing InputName from \"{connectionData.InputName}\" to \"{newInputName}\"" );
					connectionData.InputName = newInputName;
				}

				connectionsData.Add( connectionData );
			}
		}

		return connectionsData;
	}

	public void AddBlackboardParameter( BaseBlackboardParameter blackboardParameter )
	{
		ShaderGraphPlus.AddParameter( blackboardParameter );
	}

	public void AddNewNode( BaseNodePlus node )
	{
		ShaderGraphPlus.AddNode( node );
	}

	internal void AddNewSubgraphOutputID( string outputName )
	{
		if ( IsSubgraph )
		{
			SubgraphOutputIds.Add( new( FunctionResultID, outputName ) );
		}
	}

	internal void ConnectNodesTest()
	{
		foreach ( var data in ConnectionFixupDatas )
		{
			if ( data.NodeInputs == null )
				continue;

			foreach ( var mapping in data.NodeInputs )
			{
				var nodeFrom = ShaderGraphPlus.FindNode( mapping.Key.Identifier );

				if ( nodeFrom != null )
				{
					SGPLog.Warning( $"Trying to connect \"{mapping.Key.Output}\" from node \"{nodeFrom}\" to \"{mapping.Value}\"" );

					data.NodeToConnectTo.Graph = ShaderGraphPlus;
					data.NodeToConnectTo.ConnectNode( mapping.Value, mapping.Key.Output, mapping.Key.Identifier );
				}
				else
				{
					SGPLog.Error( $"Could not find node with identifier \"{mapping.Key.Identifier}\"" );
				}
			}
		}
	}

	private static readonly List<int> SupportedVersions = new List<int>()
	{
		1,
	};
}

struct ConnectionData
{
	public string InputName { get; set; }
	public Type InputType { get; set; }
	public string InputNodeIdentifier { get; set; }

	public string OutputName { get; set; }
	public Type OutputType { get; set; }
	public string OutputNodeIdentifier { get; set; }
}

internal struct NodeConnectionDataNew
{
	public string ConnectionFrom;
	public string ConnectionTo;
	public string SourseIdentifier;
	public string DestinationIdentifier;

	public NodeConnectionDataNew( string connectionFrom, string connectionTo, string sourceIdentifier, string destinationIdentifier )
	{
		ConnectionFrom = connectionFrom;
		ConnectionTo = connectionTo;
		SourseIdentifier = sourceIdentifier;
		DestinationIdentifier = destinationIdentifier;
	}

}
