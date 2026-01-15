using NodeEditorPlus;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

public sealed class ConstantToParameterNodeType : ClassNodeType
{
	private readonly IConstantNode IConstantNode;
	private readonly string Name;

	public BaseBlackboardParameter BlackboardParameter { get; private set; }

	public ConstantToParameterNodeType( TypeDescription type, IConstantNode value, string name ) : base( type )
	{
		IConstantNode = value;
		Name = name;
	}

	public override IGraphNode CreateNode( INodeGraph graph )
	{
		var node = base.CreateNode( graph );
		var isSubgraph = ((ShaderGraphPlus)graph).IsSubgraph;

		if ( isSubgraph )
		{
			BlackboardParameter = IConstantNode.InitializeSubgraphInputParameter( Name );
		}
		else
		{
			BlackboardParameter = IConstantNode.InitializeMaterialParameter( Name );
		}

		var parameterNode = BlackboardParameter.InitializeNode();
		parameterNode.Identifier = IConstantNode.Identifier;

		return parameterNode;
	}
}
