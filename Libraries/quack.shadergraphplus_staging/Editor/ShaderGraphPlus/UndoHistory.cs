
using Editor;

namespace ShaderGraphPlus;

public class UndoHistory : Widget
{
	public IEnumerable<string> History { set => _listView.SetItems( value ); }

	private int _undoLevel;
	public int UndoLevel
	{
		get => _undoLevel;
		set
		{
			_undoOption.Enabled = _undoStack.CanUndo;
			_redoOption.Enabled = _undoStack.CanRedo;
			_clearOption.Enabled = _undoStack.CanUndo || _undoStack.CanRedo;
			_undoOption.Text = _undoStack.UndoName;
			_redoOption.Text = _undoStack.RedoName;
			_undoOption.StatusTip = _undoStack.UndoName;
			_redoOption.StatusTip = _undoStack.RedoName;

			if ( _undoLevel == value )
				return;

			_undoLevel = value;

			Update();
		}
	}

	private readonly UndoHistoryListView _listView;

	private UndoStack _undoStack;
	private Option _undoOption;
	private Option _redoOption;
	private Option _clearOption;

	public Action OnUndo { get; set; }
	public Action OnRedo { get; set; }
	public Action<int> OnHistorySelected { get; set; }

	public UndoHistory( Widget parent, UndoStack undoStack ) : base( parent )
	{
		_undoStack = undoStack;

		Name = "Undo History";
		WindowTitle = "Undo History";
		SetWindowIcon( "history" );

		Layout = Layout.Column();

		var toolBar = new ToolBar( this, "UndoHistoryToolBar" );
		_undoOption = toolBar.AddOption( "Undo", "undo", () => OnUndo?.Invoke() );
		_redoOption = toolBar.AddOption( "Redo", "redo", () => OnRedo?.Invoke() );
		toolBar.AddSeparator();
		_clearOption = toolBar.AddOption( "Clear History", "playlist_remove", Clear );
		_clearOption.StatusTip = "Clear History";

		Layout.Add( toolBar );

		_listView = new UndoHistoryListView( this );
		Layout.Add( _listView, 1 );

		History = _undoStack.Names;
		UndoLevel = _undoStack.UndoLevel;
	}

	private void Clear()
	{
		_undoStack.Clear();

		History = _undoStack.Names;
		UndoLevel = 0;
	}
}

public class UndoHistoryListView : ListView
{
	private readonly UndoHistory _history;

	public UndoHistoryListView( UndoHistory parent ) : base( parent )
	{
		_history = parent;

		ItemSize = new Vector2( 0, Theme.RowHeight );
		ItemSpacing = 0;
		Margin = 2;
	}

	protected override void OnPaint()
	{
		Paint.ClearPen();
		Paint.SetBrush( Theme.WindowBackground );
		Paint.DrawRect( LocalRect );

		base.OnPaint();
	}

	protected override void PaintItem( VirtualWidget item )
	{
		if ( item.Object is not string undoName )
			return;

		var rect = item.Rect.Shrink( 8, 0, 0, 0 );

		Paint.ClearPen();

		if ( Paint.HasMouseOver )
		{
			Paint.SetBrush( Theme.WindowBackground.Lighten( 0.25f ) );
			Paint.DrawRect( item.Rect );
		}

		if ( item.Row >= _history.UndoLevel )
		{
			Paint.SetDefaultFont( italic: true );
			Paint.SetPen( Color.White.WithAlpha( Paint.HasMouseOver ? 0.5f : 0.4f ), 3.0f );
		}
		else
		{
			Paint.SetPen( Color.White.WithAlpha( Paint.HasMouseOver ? 0.9f : 0.8f ), 3.0f );
		}

		if ( item.Row == _history.UndoLevel - 1 )
		{
			rect = item.Rect.Shrink( Theme.RowHeight, 0, 0, 0 );
			Paint.SetPen( Theme.Blue, 3.0f );
			Paint.DrawIcon( new Rect( item.Rect.Position, Theme.RowHeight ), "arrow_right", Theme.RowHeight );
		}

		Paint.DrawText( rect, undoName, TextFlag.LeftCenter | TextFlag.SingleLine );
	}

	protected override bool OnItemPressed( VirtualWidget pressedItem, MouseEvent e )
	{
		_history.OnHistorySelected?.Invoke( pressedItem.Row );

		return true;
	}
}
