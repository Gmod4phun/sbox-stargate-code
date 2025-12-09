using Editor;

namespace ShaderGraphPlus;

// TODO : Add Line Numbers to the left of the text area. Also add an option to toggle LineNumbers on or off.

public class TextView : Widget
{
	public Action TextChanged;

	private string Text = "";
	private TextEdit TextEdit;

	public TextView( Widget parent, string windowTitle, string text ) : base( parent )
	{
		Name = windowTitle;
		WindowTitle = windowTitle;
		Text = text;
		Parent = parent;
		SetWindowIcon( "edit" );

		Layout = Layout.Row();

		// TODO
		var LineNumbers = Layout.AddRow( 1 );
		LineNumbers.AddSpacingCell( 16f );

		//Layout.Add( LineNumbers );

		TextEdit = new TextEdit( this );
		TextEdit.ReadOnly = true;
		Text = text;
		TextEdit.PlainText = Text;
		TextEdit.VerticalScrollbarMode = ScrollbarMode.On;
		TextEdit.TextChanged += x =>
		{
			TextChanged?.Invoke();
		};

		TextEdit.SetStyles( $"font-size: 12px; font-weight: regular; color: {Theme.TextControl.Hex};" );

		Layout.Add( TextEdit );
	}


	public string GetTextContents()
	{
		return Text;
	}

	public void SetTextContents( string text )
	{
		if ( !string.IsNullOrWhiteSpace( text ) )
		{
			text.ReplaceLineEndings();
			text = text.Replace( "    ", "\t" );
		}

		Text = text;
		TextEdit.PlainText = Text;
	}
}
