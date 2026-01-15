namespace ShaderGraphPlus.Internal;

using VanillaGraph = Editor.ShaderGraph;
using VanillaNodes = Editor.ShaderGraph.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;

internal abstract class BaseNodeConvert
{
	public virtual Type NodeTypeToConvert { get; }

	public BaseNodeConvert()
	{

	}

	public virtual IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode vanillaNode )
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Key is the old <see cref="NodeInput"/> property name and Value is the new <see cref="NodeInput"/> property name.
	/// </summary>
	public virtual Dictionary<string, string> GetNodeInputNameMappings()
	{
		return new Dictionary<string, string>();
	}
}
