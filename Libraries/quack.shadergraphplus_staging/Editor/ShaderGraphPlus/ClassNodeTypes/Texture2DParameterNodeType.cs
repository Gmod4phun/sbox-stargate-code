using NodeEditorPlus;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

public sealed class Texture2DParameterNodeType : ClassNodeType
{
	string Name;
	string ImagePath;

	public Texture2DParameterNodeType( TypeDescription type, string name, string imagePath ) : base( type )
	{
		Name = name;
		ImagePath = imagePath;
	}

	public override IGraphNode CreateNode( INodeGraph graph )
	{
		var node = base.CreateNode( graph );
		var shaderGraph = graph as ShaderGraphPlus;
		if ( node is Texture2DParameterNode texture2DParameterNode )
		{
			texture2DParameterNode.UI = texture2DParameterNode.UI with { Name = Name, DefaultTexture = ImagePath };
		}

		return node;
	}
}
