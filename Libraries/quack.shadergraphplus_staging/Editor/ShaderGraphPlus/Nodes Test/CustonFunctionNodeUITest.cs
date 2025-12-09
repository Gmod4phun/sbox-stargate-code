using Editor;

namespace ShaderGraphPlus.Nodes;

[Title( "Custom Function UI Test" ), Category( "Dev" ), Icon( "code" )]
[InternalNode]
public class CustonFunctionNodeUITest : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Input, Hide]
	public NodeInput MyInput { get; set; }

	public string MyName { get; set; }

	[CustomCodeEdit]
	public string Body { get; set; } = "";

	public CustonFunctionNodeUITest()
	{
		ExpandSize = new Vector2( 180, 180 );
	}


	//public override void OnPaint( Rect rect )
	//{	
	//	var boarderSize = 8.0f; 
	//	var codeViewRect = rect.Shrink( boarderSize );
	//	var codeViewMarginRect = codeViewRect.Shrink( 4 );
	//
	//	Paint.ClearPen();
	//	Paint.SetBrush( Theme.ControlBackground );
	//	Paint.DrawRect( codeViewRect );
	//
	//
	//	// Custom node rendering
	//	Paint.SetPen( Color.White );
	//	Paint.SetFont( "Cascadia Code", 8 );
	//	Paint.DrawText( codeViewMarginRect, Body, TextFlag.LeftTop | TextFlag.WordWrap );
	//}

	public override NodeUI CreateUI( GraphView view )
	{
		//return base.CreateUI( view );

		return new CustomCodeNodeUI( view, this );
	}
}


[System.AttributeUsage( AttributeTargets.Property )]
internal sealed class CustomCodeEditAttribute : Attribute
{
	public CustomCodeEditAttribute()
	{
	}
}

[CustomEditor( typeof( string ), WithAllAttributes = new[] { typeof( CustomCodeEditAttribute ) } )]
internal sealed class CustomCodeTextTest : ControlObjectWidget
{
	// CustomCodeTextEdit


	public CustomCodeTextTest( SerializedProperty property ) : base( property, true )
	{
		Layout = Layout.Row();
		Layout.Spacing = 2;

		var value = property.GetValue<string>();

		var textEdit = new CustomCodeTextEdit();
		textEdit.PlainText = value;

		textEdit.TextChanged += TextChanged;

		Layout.Add( textEdit );
	}

	private void TextChanged( string text )
	{
		SerializedProperty.SetValue( text );
	}

	protected override void OnPaint()
	{
		// Overriding and doing nothing here will prevent the default background from being painted
	}
}

internal sealed class CustomCodeTextEdit : TextEdit
{
	public CustomCodeTextEdit( Widget parent = null ) : base( parent )
	{
		SetStyles( "font-family: Cascadia Code, monospace; font-size: 12px; white-space: pre; tab-size: 16;" );
		TabSize = 16;
	}
}
