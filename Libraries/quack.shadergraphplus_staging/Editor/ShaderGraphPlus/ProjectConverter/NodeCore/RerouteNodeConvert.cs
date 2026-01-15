using VanillaGraph = Editor.ShaderGraph;
using VanillaNodes = Editor.ShaderGraph.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus.Internal;

internal class RerouteNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Reroute );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldReroute = oldNode as VanillaNodes.Reroute;

		//SGPLog.Info( "Convert reroute node" );

		var newNode = new ReroutePlus();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.Comment = oldReroute.Comment;


		newNodes.Add( newNode );

		return newNodes;
	}
}
