using ShaderGraphPlus.Nodes;
using VanillaGraph = Editor.ShaderGraph;
using VanillaNodes = Editor.ShaderGraph.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;

namespace ShaderGraphPlus.Internal;

internal class BranchNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Branch );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldBranchNode = oldNode as VanillaNodes.Branch;

		//SGPLog.Info( "Convert branch node" );

		if ( !string.IsNullOrWhiteSpace( oldBranchNode.Name ) )
		{
			var newNode = new SwitchNode();
			newNode.Identifier = oldNode.Identifier;
			newNode.Position = oldNode.Position;
			newNode.Name = oldBranchNode.Name;
			newNode.Enabled = oldBranchNode.Enabled;
			newNode.IsAttribute = oldBranchNode.IsAttribute;

			newNodes.Add( newNode );
		}
		else
		{
			var newNode = new ComparisonNode();
			newNode.Identifier = oldNode.Identifier;
			newNode.Position = oldNode.Position;
			newNode.Operator = oldBranchNode.Operator switch
			{
				VanillaNodes.Branch.OperatorType.Equal => ComparisonNode.OperatorType.Equal,
				VanillaNodes.Branch.OperatorType.NotEqual => ComparisonNode.OperatorType.NotEqual,
				VanillaNodes.Branch.OperatorType.GreaterThan => ComparisonNode.OperatorType.GreaterThan,
				VanillaNodes.Branch.OperatorType.LessThan => ComparisonNode.OperatorType.LessThan,
				VanillaNodes.Branch.OperatorType.GreaterThanOrEqual => ComparisonNode.OperatorType.GreaterThanOrEqual,
				VanillaNodes.Branch.OperatorType.LessThanOrEqual => ComparisonNode.OperatorType.LessThanOrEqual,
				_ => throw new NotImplementedException(),
			};

			newNodes.Add( newNode );
		}



		return newNodes;
	}
}
