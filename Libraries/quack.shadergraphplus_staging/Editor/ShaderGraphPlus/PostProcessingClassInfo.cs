namespace ShaderGraphPlus;

public struct PostProcessingComponentInfo : IValid
{
	public string ComponentTitle { get; set; }

	public string ComponentCategory { get; set; }

	public string Icon { get; set; }

	public int Order { get; set; }

	public bool IsValid => !string.IsNullOrWhiteSpace( ComponentTitle );

	public PostProcessingComponentInfo( int order )
	{
		ComponentTitle = "";
		ComponentCategory = "";
		Icon = "";
		Order = order;
	}
}
