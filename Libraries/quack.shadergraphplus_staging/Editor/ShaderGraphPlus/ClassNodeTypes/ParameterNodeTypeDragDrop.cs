using NodeEditorPlus;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

public sealed class ParameterNodeTypeDragDrop : ClassNodeType
{
	BaseBlackboardParameter BaseBlackboardValue;

	public ParameterNodeTypeDragDrop( TypeDescription type, BaseBlackboardParameter value ) : base( type )
	{
		BaseBlackboardValue = value;
	}

	public override IGraphNode CreateNode( INodeGraph graph )
	{
		base.CreateNode( graph );
		return BaseBlackboardValue.InitializeNode();
	}
}
