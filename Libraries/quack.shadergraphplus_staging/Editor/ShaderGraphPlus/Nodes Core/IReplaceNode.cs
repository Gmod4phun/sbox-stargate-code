namespace ShaderGraphPlus.Nodes;

/// <summary>
/// For nodes that are being replaced with something else.
/// </summary>
public interface IReplaceNode
{
	public bool ReplacementCondition { get; }
	public BaseNodePlus GetReplacementNode();
}
