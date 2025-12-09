using Editor.NodeEditor;
using Sandbox;
using Sandbox.Utility.Svg;

namespace ShaderGraphPlus;

public class ProjectUpgrader
{

	internal JsonSerializerOptions SerializerOptions;

	public ProjectUpgrader( JsonSerializerOptions serializerOptions )
	{
		SerializerOptions = serializerOptions;
	}

	public void UpgradeProjectRoot()
	{
		// Upgrade the root of the project basically eveything but the nodes.
	}

	public void UpgradeNodes()
	{
		// Upgrade all the nodes.
	}
}

