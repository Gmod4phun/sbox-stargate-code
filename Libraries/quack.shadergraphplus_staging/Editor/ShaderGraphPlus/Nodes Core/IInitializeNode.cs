namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Interface for nodes that want to setup anyting post node object deserializeation.
/// </summary>
public interface IInitializeNode
{
	/// <summary>
	/// Called after the node has been deserialized.
	/// </summary>
	public void InitializeNode();
}
