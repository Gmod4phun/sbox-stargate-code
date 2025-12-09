
using NodeEditorPlus;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

public sealed class ParameterNodeType : ClassNodeType
{
	public BaseBlackboardParameter BlackboardParameter { get; private set; }

	private readonly string Name;
	private readonly Type BlackboardParameterType;

	public ParameterNodeType( TypeDescription type, Type targetBlackboardType, string name ) : base( type )
	{
		Name = name;
		BlackboardParameterType = targetBlackboardType;
	}

	public override IGraphNode CreateNode( INodeGraph graph )
	{
		base.CreateNode( graph );
		return BlackboardParameter.InitializeNode();
	}
}
