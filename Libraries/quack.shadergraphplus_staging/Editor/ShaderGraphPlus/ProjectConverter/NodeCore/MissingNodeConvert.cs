using VanillaGraph = Editor.ShaderGraph;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;

namespace ShaderGraphPlus.Internal;

internal class MissingNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaGraph.MissingNode );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldMissingNodeNode = oldNode as VanillaGraph.MissingNode;


		var newNode = new Nodes.MissingNode();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.Content = oldMissingNodeNode.Content;

		newNodes.Add( newNode );

		return newNodes;
	}
}
