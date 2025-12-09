using NodeEditorPlus;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

public sealed class NamedRerouteDeclarationNodeType : ClassNodeType
{
	private string Name;

	public NamedRerouteDeclarationNodeType( TypeDescription type, string name = "" ) : base( type )
	{
		Name = name;
	}

	public override IGraphNode CreateNode( INodeGraph graph )
	{
		var node = base.CreateNode( graph );
		if ( node is NamedRerouteDeclarationNode namedRerouteDeclarationNode )
		{
			namedRerouteDeclarationNode.Name = Name;

			return namedRerouteDeclarationNode;
		}
		return node;
	}
}

public sealed class NamedRerouteNodeType : ClassNodeType
{
	private string Name;

	public NamedRerouteNodeType( TypeDescription type, string name = "" ) : base( type )
	{
		Name = name;
	}

	public override IGraphNode CreateNode( INodeGraph graph )
	{
		var node = base.CreateNode( graph );
		if ( node is NamedRerouteNode namedRerouteNode )
		{
			namedRerouteNode.Name = Name;

			return namedRerouteNode;
		}
		return node;
	}
}
