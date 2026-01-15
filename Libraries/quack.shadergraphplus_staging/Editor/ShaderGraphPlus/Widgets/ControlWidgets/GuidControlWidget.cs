using Editor;

namespace ShaderGraphPlus;

//[CustomEditor( typeof( Guid ) )]
[CustomEditor( typeof( Guid ), NamedEditor = "sgp_guidreadonly" )]
public sealed class GuidControlWidget : ControlObjectWidget
{
	Widget Widget;
	Guid Guid;

	public GuidControlWidget( SerializedProperty property ) : base( property, true )
	{
		//PaintBackground = false;
		Layout = Layout.Row();
		Layout.Spacing = 2;

		Widget = new Widget( this );
		Widget.Size = Theme.RowHeight;
		Widget.VerticalSizeMode = SizeMode.CanGrow;
		Widget.HorizontalSizeMode = SizeMode.Flexible;
		Widget.OnPaintOverride = PaintReadonly;
		Widget.Cursor = CursorShape.Finger;
		Widget.ToolTip = $"Edit";
		Layout.Add( Widget, 1 );

		Guid = SerializedProperty.GetValue<Guid>();
	}

	bool PaintReadonly()
	{
		Editor.Paint.DrawText( LocalRect.Shrink( 8, 0, 0, 0 ), Guid.ToString(), TextFlag.LeftCenter );
		return true;
	}
}
