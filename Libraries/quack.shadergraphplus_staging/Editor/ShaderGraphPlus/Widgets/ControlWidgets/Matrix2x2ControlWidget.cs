using Editor;

namespace ShaderGraphPlus;

[CustomEditor( typeof( Float2x2 ) )]
sealed class Float2x2ControlWidget : ControlObjectWidget
{
	public Float2x2ControlWidget( SerializedProperty property ) : base( property, true )
	{
		//property.TryGetAsObject( out SerializedObject so );

		MinimumSize = Theme.RowHeight * 2 + 2;

		Layout = Layout.Column();
		Layout.Spacing = 2;

		var Row1Layout = Layout.AddRow();
		Row1Layout.Spacing = 4;
		{
			AddField( SerializedObject, Row1Layout, "M11" );
			Row1Layout.AddStretchCell( 0 );

			AddField( SerializedObject, Row1Layout, "M12" );
		}

		var Row2Layout = Layout.AddRow();
		Row2Layout.Spacing = 4;
		{
			AddField( SerializedObject, Row2Layout, "M21" );
			Row2Layout.AddStretchCell( 0 );

			AddField( SerializedObject, Row2Layout, "M22" );
		}
	}

	protected override void OnPaint()
	{
		// nothing
	}

	public void Rebuild()
	{

	}

	void AddField( SerializedObject serializedObject, Layout layout, string propertyName )
	{
		var property = serializedObject.GetProperty( propertyName );
		var controlWidget = ControlWidget.Create( property );

		layout.Add( controlWidget );
		layout.Add( new Label( propertyName ) { MinimumHeight = Theme.RowHeight, FixedWidth = 25 } );
		layout.AddSpacingCell( 4 );
	}

}
