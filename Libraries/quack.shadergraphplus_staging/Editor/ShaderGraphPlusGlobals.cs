namespace ShaderGraphPlus;

[AttributeUsage( AttributeTargets.Property )]
internal sealed class HLSLAssetPathAttribute : Attribute
{
	internal HLSLAssetPathAttribute()
	{
	}
}

internal static class ShaderGraphPlusGlobals
{
	internal static class GraphCompiler
	{
		internal const int NoNodePreviewID = 0;
	}

	internal static class ControlWidgetCustomEditors
	{
		public const string UIGroupEditor = "shadergraphplus_UiGroupEditor";
		public const string ShaderFeatureEnumPreviewIndexEditor = "shadergraphplus_ShaderFeatureEnumPreviewIndexEditor";
		public const string NamedRerouteReferenceEditor = "shadergraphplus_NamedRerouteReferenceEditor";
		public const string PortTypeChoiceEditor = "shadergraphplus_PortTypeChoiceEditor";
	}

	internal const string AssetTypeName = "Shader Graph Plus";
	internal const string AssetTypeExtension = "sgrph";

	internal const string SubgraphAssetTypeName = "Shader Graph Plus Function";
	internal const string SubgraphAssetTypeExtension = "sgpfunc";
}
