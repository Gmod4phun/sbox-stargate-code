

using Editor;
using static ShaderGraphPlus.GraphCompiler;

namespace ShaderGraphPlus;

public class Output : Widget
{
	private IssueListView _issueListView;

	public IEnumerable<GraphCompiler.GraphIssue> Errors { set { _issueListView.SetItems( value.Cast<object>() ); } }

	private List<GraphCompiler.GraphIssue> _graphIssues;
	public List<GraphCompiler.GraphIssue> GraphIssues
	{
		get => _graphIssues;
		set
		{
			_graphIssues = value;
			//_issueListView.AddItems( value.Cast<object>() );
			_issueListView.SetItems( value.Cast<object>() );
		}
	}

	public Action<BaseNodePlus> OnNodeSelected { get; set; }

	public void ClearErrors()
	{
		foreach ( var error in _issueListView.Items.Where( x => x is GraphIssue issue && issue.IsWarning == false ) )
		{
			_issueListView.RemoveItem( error );
		}
	}

	internal void ClearWarnings()
	{
		foreach ( var warning in _issueListView.Items.Where( x => x is GraphIssue issue && issue.IsWarning == true ) )
		{
			_issueListView.RemoveItem( warning );
		}
	}

	public Output( Widget parent ) : base( parent )
	{
		Name = "Output";
		WindowTitle = "Output";
		SetWindowIcon( "notes" );

		Layout = Layout.Column();

		_issueListView = new( this );
		Layout.Add( _issueListView );
	}
}

public class IssueListView : ListView
{
	private Output _output;

	public IssueListView( Output parent ) : base( parent )
	{
		_output = parent;
		ItemActivated = ( a ) =>
		{
			if ( a is not GraphCompiler.GraphIssue issueInfo )
				return;

			if ( issueInfo.Node != null && issueInfo.Node is not DummyNode )
			{
				_output.OnNodeSelected?.Invoke( issueInfo.Node );
			}
		};

		ItemContextMenu = OpenItemContextMenu;
		ItemSize = new Vector2( 0, 48 );
		ItemSpacing = 0;
		Margin = 0;
	}

	private void OpenItemContextMenu( object item )
	{
		if ( item is not GraphCompiler.GraphIssue )
			return;

		if ( item is GraphCompiler.GraphIssue issue )
		{
			OnOpenItemContextMenuError( item, issue );
		}

	}

	private void OnOpenItemContextMenuError( object item, GraphIssue error )
	{
		var m = new Menu();

		if ( error.Node != null && error.Node is not DummyNode )
		{
			var nodeName = DisplayInfo.ForType( error.Node.GetType() ).Name;

			m.AddOption( "Go to Error", "arrow_upward", () => _output.OnNodeSelected?.Invoke( error.Node ) );
			m.AddOption( "Copy Error", "content_copy", () => EditorUtility.Clipboard.Copy( $"{error.Message}\n{nodeName} #{error.Node.Identifier}" ) );
		}
		else
		{
			m.AddOption( "Copy Error", "content_copy", () => EditorUtility.Clipboard.Copy( $"{error.Message}" ) );
		}

		m.OpenAt( Editor.Application.CursorPosition );
	}

	protected override void OnPaint()
	{
		Paint.ClearPen();
		Paint.SetBrush( Theme.WindowBackground );
		Paint.DrawRect( LocalRect );

		base.OnPaint();
	}

	public void OnPaintError( VirtualWidget item, GraphCompiler.GraphIssue error )
	{

		var color = Theme.Red;

		Paint.SetBrush( color.WithAlpha( Paint.HasMouseOver ? 0.1f : 0.03f ) );
		Paint.ClearPen();
		Paint.DrawRect( item.Rect.Shrink( 0, 1 ) );

		Paint.Antialiasing = true;
		Paint.SetPen( color.WithAlpha( Paint.HasMouseOver ? 1 : 0.7f ), 3.0f );
		Paint.ClearBrush();

		var iconRect = item.Rect.Shrink( 12, 0 );
		iconRect.Width = 24;

		Paint.DrawIcon( iconRect, "error", 24 );

		var rect = item.Rect.Shrink( 48, 8, 0, 8 );

		Paint.SetPen( Color.White.WithAlpha( Paint.HasMouseOver ? 1 : 0.8f ), 3.0f );
		Paint.DrawText( rect, error.Message, (error.Node != null ? TextFlag.LeftTop : TextFlag.LeftCenter) | TextFlag.SingleLine );

		if ( error.Node != null && error.Node is not DummyNode )
		{
			var nodeName = DisplayInfo.ForType( error.Node.GetType() ).Name;
			Paint.SetPen( Color.White.WithAlpha( Paint.HasMouseOver ? 0.5f : 0.4f ), 3.0f );
			Paint.DrawText( rect, $"{nodeName}", TextFlag.LeftBottom | TextFlag.SingleLine );
		}
	}

	public void OnPaintWarning( VirtualWidget item, GraphCompiler.GraphIssue warning )
	{
		var color = Theme.Yellow;

		Paint.SetBrush( color.WithAlpha( Paint.HasMouseOver ? 0.1f : 0.03f ) );
		Paint.ClearPen();
		Paint.DrawRect( item.Rect.Shrink( 0, 1 ) );

		Paint.Antialiasing = true;
		Paint.SetPen( color.WithAlpha( Paint.HasMouseOver ? 1 : 0.7f ), 3.0f );
		Paint.ClearBrush();

		var iconRect = item.Rect.Shrink( 12, 0 );
		iconRect.Width = 24;

		Paint.DrawIcon( iconRect, "error", 24 );

		var rect = item.Rect.Shrink( 48, 8, 0, 8 );

		Paint.SetPen( Color.White.WithAlpha( Paint.HasMouseOver ? 1 : 0.8f ), 3.0f );
		Paint.DrawText( rect, warning.Message, (warning.Node != null ? TextFlag.LeftTop : TextFlag.LeftCenter) | TextFlag.SingleLine );

		if ( warning.Node != null )
		{
			var nodeName = DisplayInfo.ForType( warning.Node.GetType() ).Name;
			Paint.SetPen( Color.White.WithAlpha( Paint.HasMouseOver ? 0.5f : 0.4f ), 3.0f );
			Paint.DrawText( rect, $"{nodeName}", TextFlag.LeftBottom | TextFlag.SingleLine );
		}
	}

	protected override void PaintItem( VirtualWidget item )
	{
		if ( item.Object is not GraphCompiler.GraphIssue )
			return;

		if ( item.Object is GraphCompiler.GraphIssue issue )
		{
			if ( issue.IsWarning )
			{
				OnPaintWarning( item, issue );
			}
			else
			{
				OnPaintError( item, issue );
			}
		}

	}
}
