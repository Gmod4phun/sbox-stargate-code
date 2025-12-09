using NodeEditorPlus;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

public sealed class SubgraphOutputNodeType : ClassNodeType
{
	private string Name;
	private SubgraphPortType OutputType;

	public SubgraphOutputNodeType( TypeDescription type, SubgraphPortType outputType, string name = "" ) : base( type )
	{
		Name = name;
		OutputType = outputType;
	}

	public override IGraphNode CreateNode( INodeGraph graph )
	{
		var node = base.CreateNode( graph );
		if ( node is SubgraphOutput subgraphOutput )
		{
			subgraphOutput.OutputName = Name;
			subgraphOutput.OutputType = OutputType;
		}
		return node;
	}
}
