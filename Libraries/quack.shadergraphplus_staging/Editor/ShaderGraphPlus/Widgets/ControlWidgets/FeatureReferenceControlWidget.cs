using Editor;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

[CustomEditor( typeof( string ), WithAllAttributes = new[] { typeof( ShaderFeatureReferenceAttribute ) } )]
internal sealed class FeatureReferenceControlWidget : DropdownControlWidget<ShaderFeatureBase>
{
	ShaderGraphPlus Graph;

	// TODO : Get this ControlWidget working again.
	public FeatureReferenceControlWidget( SerializedProperty property ) : base( property )
	{
		//var target = property.Parent.Targets.OfType<ShaderGraphPlus>().FirstOrDefault();
		var target = property.Parent.Targets.FirstOrDefault();

		SGPLog.Info( $"SerializedProperty parent target is \"{target}\"" );

		Graph = null; // Shut up the engine saying this valu is unused.
		if ( Graph is null ) return;

		//if ( SerializedProperty.GetValue<ShaderFeatureInfo>().IsValid )
		//{
		//	var name = SerializedProperty.GetValue<ShaderFeatureInfo>().UserDefinedName;
		//	if ( Graph.Features.ContainsKey( name ) )
		//	{
		//		SerializedProperty.SetValue<ShaderFeatureInfo>( Graph.Features[name] );
		//	}
		//}
		if ( string.IsNullOrWhiteSpace( SerializedProperty.GetValue<string>() ) )
		{
			SerializedProperty.SetValue<string>( "None" );
		}
	}

	protected override IEnumerable<object> GetDropdownValues()
	{
		List<object> list = new();
		list.Add( "None" );

		foreach ( var feature in Graph.Parameters.Where( x => x is ShaderFeatureBooleanParameter || x is ShaderFeatureEnumParameter ) )
		{
			if ( feature is ShaderFeatureBooleanParameter boolFeatureParam )
			{
				var shaderFeatureBool = new ShaderFeatureBoolean()
				{
					Name = boolFeatureParam.Name,
					HeaderName = boolFeatureParam.HeaderName,
					Description = boolFeatureParam.Description,
				};
				var entry = new Entry();
				entry.Value = shaderFeatureBool;
				entry.Label = $"{shaderFeatureBool.GetFeatureString()}";
				entry.Description = "";
				list.Add( entry );
			}
			else if ( feature is ShaderFeatureEnumParameter boolEnumParam )
			{
				throw new NotImplementedException();
			}
		}

		return list;
	}

	protected override void OnPaint()
	{
		base.OnPaint();
	}
}
