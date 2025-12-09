
namespace ShaderGraphPlus;

/// <summary>
/// Wrapper to contain a ShaderFeature for use with OnAttribute.
/// </summary>
public class ShaderFeatureWrapper
{
	public string FeatureName { get; private set; }
	public int Value { get; private set; }

	public ShaderFeatureWrapper( string featureName, int value )
	{
		FeatureName = featureName;
		Value = value;
	}
}
