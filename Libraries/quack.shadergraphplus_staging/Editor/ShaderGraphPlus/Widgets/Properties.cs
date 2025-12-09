using Editor;

namespace ShaderGraphPlus;

public class Properties : Widget
{
	private ScrollArea scroller;
	private ControlSheet sheet;
	private string filterText;

	private object _target;
	public object Target
	{
		get => _target;
		set
		{
			if ( value == _target )
				return;

			_target = value;

			Editor.Clear( true );

			if ( value is null )
				return;

			var so = value.GetSerialized();
			so.OnPropertyChanged += x =>
			{
				PropertyUpdated?.Invoke( x );
			};

			sheet = new ControlSheet();
			sheet.AddObject( so, PropertyFilter );

			scroller = new ScrollArea( this );
			scroller.Canvas = new Widget();
			scroller.Canvas.Layout = Layout.Column();
			scroller.Canvas.VerticalSizeMode = SizeMode.CanGrow;
			scroller.Canvas.HorizontalSizeMode = SizeMode.Flexible;
			scroller.Canvas.Layout.Add( sheet );
			scroller.Canvas.Layout.AddStretchCell();

			Editor.Add( scroller );
		}
	}

	private readonly Layout Editor;

	public Action<SerializedProperty> PropertyUpdated { get; set; }

	public Properties( Widget parent ) : base( parent )
	{
		Name = "Properties";
		WindowTitle = "Properties";
		SetWindowIcon( "edit" );

		Layout = Layout.Column();

		var toolbar = new ToolBar( this );
		var filter = new LineEdit( toolbar ) { PlaceholderText = "Filter Properties.." };
		filter.TextEdited += OnFilterEdited;
		toolbar.AddWidget( filter );
		Layout.Add( toolbar );
		Layout.AddSeparator();

		Editor = Layout.AddRow( 1 );
		Layout.AddStretchCell();
	}

	private void OnFilterEdited( string filter )
	{
		filterText = filter;
		sheet.Clear( true );
		sheet.AddObject( _target.GetSerialized(), PropertyFilter );
		scroller.Update();
	}

	bool PropertyFilter( SerializedProperty property )
	{
		if ( property.HasAttribute<HideAttribute>() ) return false;
		if ( string.IsNullOrEmpty( filterText ) ) return true;
		if ( property.Name.ToLower().Contains( filterText.ToLower() ) ) return true;
		if ( property.DisplayName.ToLower().Contains( filterText.ToLower() ) ) return true;
		if ( property.TryGetAsObject( out var obj ) )
		{
			if ( property.TryGetAttribute<ConditionalVisibilityAttribute>( out var conditional ) )
			{
				if ( conditional.TestCondition( obj ) ) return false;
			}
			foreach ( var childProp in obj )
			{
				if ( childProp.HasAttribute<HideAttribute>() ) continue;
				if ( childProp.Name.ToLower().Contains( filterText.ToLower() ) || childProp.DisplayName.ToLower().Contains( filterText.ToLower() ) )
				{
					sheet.AddRow( childProp );
				}
			}
		}
		return false;
	}
}
