using Editor;

namespace ShaderGraphPlus;

[CustomEditor( typeof( Float4x4 ) )]
sealed class Float4x4ControlWidget : ControlObjectWidget
{
	public Float4x4ControlWidget( SerializedProperty property ) : base( property, true )
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
			Row1Layout.AddStretchCell( 0 );

			AddField( SerializedObject, Row1Layout, "M13" );
			Row1Layout.AddStretchCell( 0 );

			AddField( SerializedObject, Row1Layout, "M14" );
		}

		var Row2Layout = Layout.AddRow();
		Row2Layout.Spacing = 4;
		{
			AddField( SerializedObject, Row2Layout, "M21" );
			Row2Layout.AddStretchCell( 0 );

			AddField( SerializedObject, Row2Layout, "M22" );
			Row2Layout.AddStretchCell( 0 );

			AddField( SerializedObject, Row2Layout, "M23" );
			Row2Layout.AddStretchCell( 0 );

			AddField( SerializedObject, Row2Layout, "M24" );
		}

		var Row3Layout = Layout.AddRow();
		Row3Layout.Spacing = 4;
		{
			AddField( SerializedObject, Row3Layout, "M31" );
			Row3Layout.AddStretchCell( 0 );

			AddField( SerializedObject, Row3Layout, "M32" );
			Row3Layout.AddStretchCell( 0 );

			AddField( SerializedObject, Row3Layout, "M33" );
			Row3Layout.AddStretchCell( 0 );

			AddField( SerializedObject, Row3Layout, "M34" );
		}

		var Row4Layout = Layout.AddRow();
		Row4Layout.Spacing = 4;
		{
			AddField( SerializedObject, Row4Layout, "M41" );
			Row4Layout.AddStretchCell( 0 );

			AddField( SerializedObject, Row4Layout, "M42" );
			Row4Layout.AddStretchCell( 0 );

			AddField( SerializedObject, Row4Layout, "M43" );
			Row4Layout.AddStretchCell( 0 );

			AddField( SerializedObject, Row4Layout, "M44" );
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
		layout.AddSpacingCell( 4.0f );
	}
}
