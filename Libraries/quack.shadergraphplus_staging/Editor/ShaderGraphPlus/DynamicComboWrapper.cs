
namespace ShaderGraphPlus;

/// <summary>
/// Wrapper to contain a DynamicCombo for use with OnAttribute.
/// </summary>
internal class DynamicComboWrapper
{
	public string ComboName { get; private set; }
	public int Value { get; private set; }

	public DynamicComboWrapper( string comboName, int value )
	{
		ComboName = comboName;
		Value = value;
	}
}
