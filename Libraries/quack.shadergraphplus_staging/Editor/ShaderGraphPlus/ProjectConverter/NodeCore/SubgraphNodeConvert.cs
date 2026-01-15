using Editor;
using VanillaGraph = Editor.ShaderGraph;
using VanillaNodes = Editor.ShaderGraph.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;

namespace ShaderGraphPlus.Internal;

internal class SubgraphNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaGraph.SubgraphNode );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldSubgraphNode = oldNode as VanillaGraph.SubgraphNode;

		var newNode = new SubgraphNode();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.SubgraphPath = oldSubgraphNode.SubgraphPath.Replace( ".shdrfunc", $".{ShaderGraphPlusGlobals.SubgraphAssetTypeExtension}" );

		var fullPath = Editor.FileSystem.Content.GetFullPath( oldSubgraphNode.SubgraphPath );

		var subgraph = new VanillaGraph.ShaderGraph();
		subgraph.Deserialize( Editor.FileSystem.Content.ReadAllText( oldSubgraphNode.SubgraphPath ) );
		subgraph.Path = oldSubgraphNode.SubgraphPath;

		var subgraphPlus = new ShaderGraphPlus();
		var projectConverter = new ProjectConverter( subgraph, subgraphPlus, true );

		var conversionResult = projectConverter.Convert();

		if ( !projectConverter.Errored )
		{
			fullPath = fullPath.Replace( ".shdrfunc", $".{ShaderGraphPlusGlobals.SubgraphAssetTypeExtension}" );

			System.IO.File.WriteAllText( fullPath, conversionResult.Serialize() );

			var asset = AssetSystem.RegisterFile( fullPath );

			if ( asset == null )
			{
				SGPLog.Error( $"Unable to register asset at path \"{fullPath}\"" );
			}
			else
			{
				SGPLog.Info( $"Registerd subgraphplus asset at path \"{fullPath}\"" );
			}

			newNode.OnNodeCreated();

			newNodes.Add( newNode );

			return newNodes;
		}
		else
		{
			SGPLog.Error( $"Unable to convert shadergraph function at path \"{fullPath}\"" );

			return newNodes;
		}


	}
}
