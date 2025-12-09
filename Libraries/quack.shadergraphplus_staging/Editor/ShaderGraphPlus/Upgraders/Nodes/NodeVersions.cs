using System.Text.Json.Nodes;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

public record NodeVersionRegistryEntry( Type type, int version );

/// <summary>
/// TODO : See if is even a good idea to store the current node versions here.
/// </summary>
internal class NodeVersions
{
	internal static List<NodeVersionRegistryEntry> NodeVersionRegistry { get; private set; }

	static NodeVersions()
	{
		Update();
	}

	[Event( "hotloaded" )]
	static void Update()
	{
		NodeVersionRegistry = new()
		{
			{ new NodeVersionRegistryEntry( typeof( BoolParameterNode ), 2 )},
			{ new NodeVersionRegistryEntry( typeof( IntParameterNode ), 2 )},
			{ new NodeVersionRegistryEntry( typeof( FloatParameterNode ), 2 )},
			{ new NodeVersionRegistryEntry( typeof( Float2ParameterNode ), 2 )},
			{ new NodeVersionRegistryEntry( typeof( Float3ParameterNode ), 2 )},
			{ new NodeVersionRegistryEntry( typeof( ColorParameterNode ), 2 )},
		};
	}

	internal static void RegisterUserNodes()
	{
		// TODO
	}
}
