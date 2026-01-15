using NodeEditorPlus;

namespace ShaderGraphPlus;

public sealed class SubgraphInputNodeType : ClassNodeType
{
	public BaseBlackboardParameter BlackboardParameter { get; private set; }

	private readonly string Name;
	private readonly Type TargetBlackboardParameterType;
	private readonly bool InitFromBlackBoardParameter;

	public SubgraphInputNodeType( TypeDescription type, Type targetBlackboardType, string name = "" ) : base( type )
	{
		Name = name;
		TargetBlackboardParameterType = targetBlackboardType;
		InitFromBlackBoardParameter = false;
	}

	public SubgraphInputNodeType( TypeDescription type, BaseBlackboardParameter blackboardParameter, string name = "" ) : base( type )
	{
		Name = name;
		InitFromBlackBoardParameter = true;
		BlackboardParameter = blackboardParameter;
	}

	public override IGraphNode CreateNode( INodeGraph graph )
	{
		var node = (BaseNodePlus)base.CreateNode( graph );

		if ( node is SubgraphInput subgraphInput )
		{
			if ( !InitFromBlackBoardParameter )
			{
				BlackboardParameter = (BaseBlackboardParameter)EditorTypeLibrary.Create( TargetBlackboardParameterType.Name, TargetBlackboardParameterType );
				BlackboardParameter.Name = Name;
				BlackboardParameter.Identifier = Guid.NewGuid();

				node = BlackboardParameter.InitializeNode();
			}
			else
			{
				node = BlackboardParameter.InitializeNode();
			}
		}

		return node;
	}
}
