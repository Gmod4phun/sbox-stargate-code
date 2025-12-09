using Editor;

namespace ShaderGraphPlus;

internal class BlackboardView : Widget
{
	private Button.Primary _addButton;
	private Button.Danger _deleteButton;
	private BlackboardParameterList _parameterListView;
	private BaseBlackboardParameter _selectedParameter;

	private readonly MainWindow _window;
	private readonly UndoStack _undoStack;
	private readonly Dictionary<string, IBlackboardParameterType> AvailableParameters = new( StringComparer.OrdinalIgnoreCase );

	private ShaderGraphPlus _graph;
	public ShaderGraphPlus Graph
	{
		get => _graph;
		set
		{
			if ( value == null ) return;
			if ( _graph == value ) return;

			_graph = value;

			RebuildBuildFromGraph();
		}
	}

	public Action OnDirty { get; set; }

	/// <summary>
	/// Invoked when a blackboard parameter is selected in the BlackboardView.
	/// </summary>
	public Action<BaseBlackboardParameter> OnParameterSelected { get; set; }

	/// <summary>
	/// Invoked when a blackboard parameter changes.
	/// </summary>
	public Action<BaseBlackboardParameter> OnParameterChanged { get; set; }

	/// <summary>
	/// Invoked when a blackboard parameter is created.
	/// </summary>
	public Action<BaseBlackboardParameter> OnParameterCreated { get; set; }

	/// <summary>
	/// Invoked when a blackboard parameter is deleated.
	/// </summary>
	public Action<BaseBlackboardParameter> OnParameterDeleted { get; set; }

	public BlackboardView( Widget parent, MainWindow window ) : base( parent )
	{
		Layout = Layout.Row();
		FocusMode = FocusMode.TabOrClickOrWheel;

		_window = window;
		_undoStack = window.UndoStack;

		var canvas = new Widget( null );
		canvas.Layout = Layout.Row();
		canvas.Layout.Spacing = 8;
		canvas.Layout.Spacing = 4;

		var leftColumn = canvas.Layout.AddColumn( 1, false );
		leftColumn.Spacing = 8;
		leftColumn.Spacing = 4;

		var leftColumnTopLayout = leftColumn.AddRow( 1, false );
		leftColumnTopLayout.Spacing = 8;
		leftColumnTopLayout.Spacing = 4;

		leftColumnTopLayout.AddStretchCell();

		_deleteButton = new Button.Danger( "Delete", "delete" );
		_deleteButton.Enabled = false;
		_deleteButton.ToolTip = $"Delete selected blackboard parameter";
		_deleteButton.Clicked += () =>
		{
			OnDeleteSelectedBlackboardParameter();
		};

		leftColumnTopLayout.Add( _deleteButton );

		_addButton = new Button.Primary( "Add", "new_label" );
		_addButton.Enabled = true;
		_addButton.ToolTip = $"Add new blackboard parameter";
		_addButton.Clicked += () =>
		{
			var popup = new BlackboardPopupParameterTypeSelector( this, GetRelevantParameters() );
			popup.OnSelect += ( t ) =>
			{
				OnAddParameter( t );
			};
			popup.OpenAtCursor();
		};

		leftColumnTopLayout.Add( _addButton );

		_parameterListView = leftColumn.Add( new BlackboardParameterList( null ), 1 );
		_parameterListView.ItemClicked = ( item ) =>
		{
			OnItemClicked( (BaseBlackboardParameter)item );
		};
		_parameterListView.ItemSelected = ( item ) =>
		{
			OnItemSelected( (BaseBlackboardParameter)item );
		};
		_parameterListView.ItemDrag = ( a ) =>
		{
			var parameter = a as BaseBlackboardParameter;

			var drag = new Drag( this );
			drag.Data.Object = parameter;
			drag.Execute();

			return true;
		};

		/*
		var rightColumn = canvas.Layout.AddColumn( 1, false );
		rightColumn.Spacing = 8;
		rightColumn.Spacing = 4;
		rightColumn.SizeConstraint = SizeConstraint.SetMaximumSize;

		_controlSheet = new ControlSheet();
		_controlSheet.SizeConstraint = SizeConstraint.SetMaximumSize;
		_controlSheet.SetColumnStretch( 2, 0 );
		_controlSheet.SetMinimumColumnWidth( 0, 400 );
		rightColumn.Add( _controlSheet );
		rightColumn.AddStretchCell();
		*/

		Layout.Add( canvas );
	}

	internal IDisposable UndoScope( string name )
	{
		PushUndo( name );
		return new Sandbox.Utility.DisposeAction( () => PushRedo() );
	}

	public void PushUndo( string name )
	{
		SGPLog.Info( $"Push Undo ({name})" );
		_undoStack.PushUndo( name, Graph.UndoStackSerialize() );
		_window.OnUndoPushed();
	}

	public void PushRedo()
	{
		SGPLog.Info( "Push Redo" );
		_undoStack.PushRedo( Graph.UndoStackSerialize() );
		_window.SetDirty();
	}

	public void AddParameterType<T>() where T : BaseBlackboardParameter
	{
		AddParameterType( EditorTypeLibrary.GetType<T>() );
	}

	public void AddParameterType( TypeDescription type )
	{
		var parameterType = new ClassParameterType( type );

		AvailableParameters.TryAdd( parameterType.Identifier, parameterType );
	}

	public IBlackboardParameterType FindParameterType( Type type )
	{
		return AvailableParameters.TryGetValue( type.FullName!, out var parameterType ) ? parameterType : null;
	}

	public IEnumerable<IBlackboardParameterType> GetRelevantParameters()
	{
		return AvailableParameters.Values.Where( x =>
		{
			if ( x is ClassParameterType classParameterType )
			{
				var targetType = classParameterType.Type.TargetType;

				// Only show material parameters when not in a subgraph
				if ( Graph.IsSubgraph && targetType == typeof( BoolParameter ) ) return false;
				if ( Graph.IsSubgraph && targetType == typeof( IntParameter ) ) return false;
				if ( Graph.IsSubgraph && targetType == typeof( FloatParameter ) ) return false;
				if ( Graph.IsSubgraph && targetType == typeof( Float2Parameter ) ) return false;
				if ( Graph.IsSubgraph && targetType == typeof( Float3Parameter ) ) return false;
				if ( Graph.IsSubgraph && targetType == typeof( Float4Parameter ) ) return false;
				if ( Graph.IsSubgraph && targetType == typeof( ColorParameter ) ) return false;
				if ( Graph.IsSubgraph && targetType == typeof( Texture2DParameter ) ) return false;
				if ( Graph.IsSubgraph && targetType == typeof( TextureCubeParameter ) ) return false;

				// Only show subgraph input parameters when in a subgraph
				if ( !Graph.IsSubgraph && targetType == typeof( BoolSubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( IntSubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( FloatSubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( Float2SubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( Float3SubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( Float4SubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( ColorSubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( Float2x2SubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( Float3x3SubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( Float4x4SubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( GradientSubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( Texture2DSubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( TextureCubeSubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( SamplerStateSubgraphInputParameter ) ) return false;

				// Ignore these for now
				if ( Graph.IsSubgraph && targetType == typeof( ShaderFeatureBooleanParameter ) ) return false;
				if ( Graph.IsSubgraph && targetType == typeof( ShaderFeatureEnumParameter ) ) return false;
			}
			return true;
		} );
	}

	private void OnItemSelected( BaseBlackboardParameter parameter )
	{
		//SGPLog.Info( $"Selected item : {variable}" );
	}

	private void OnItemClicked( BaseBlackboardParameter parameter )
	{
		//SGPLog.Info( $"Clicked item : {variable}" );

		SetSelectedItem( parameter );

		OnParameterSelected?.Invoke( parameter );
	}

	private void OnAddParameter( IBlackboardParameterType type )
	{
		int id = Graph._parameters.Count;
		string name = $"Parameter{id}";

		var parameterInstance = (BaseBlackboardParameter)type.CreateParameter( Graph, name );

		Graph.AddParameter( parameterInstance );

		OnDirty?.Invoke();

		SetSelectedItem( parameterInstance );

		RebuildBuildFromGraph( true );

		OnParameterCreated?.Invoke( parameterInstance );
	}

	private void OnDeleteSelectedBlackboardParameter()
	{
		var parameter = _selectedParameter as BaseBlackboardParameter;

		if ( _selectedParameter != null )
		{
			_selectedParameter = null;
			OnParameterDeleted?.Invoke( parameter );
		}

		if ( !_graph.Parameters.Any() )
		{
			_deleteButton.Enabled = false;
		}
	}

	private void BuildFromParameters( IEnumerable<BaseBlackboardParameter> parameters, bool preserveCurrentSelection = false )
	{
		_parameterListView.SetItems( parameters.Cast<object>() );

		if ( _selectedParameter != null )
		{
			var selection = Graph.FindParameterByGuid( _selectedParameter.Identifier );

			SetSelectedItem( selection );
		}
	}

	public void RebuildBuildFromGraph( bool preserveCurrentSelection = false )
	{
		BuildFromParameters( ((ShaderGraphPlus)_graph).Parameters, preserveCurrentSelection );
	}

	public void SetSelectedItem( BaseBlackboardParameter parameter )
	{
		switch ( parameter )
		{
			case BoolParameter boolParameter:
				boolParameter.UI = boolParameter.UI with { ShowTypeProperty = false, ShowStepProperty = false };
				break;
			case IntParameter intParameter:
				intParameter.UI = intParameter.UI with { ShowTypeProperty = true, ShowStepProperty = false };
				break;
			case FloatParameter floatParameter:
				floatParameter.UI = floatParameter.UI with { ShowTypeProperty = true, ShowStepProperty = true };
				break;
			case Float2Parameter float2Parameter:
				float2Parameter.UI = float2Parameter.UI with { ShowTypeProperty = true, ShowStepProperty = true };
				parameter = float2Parameter;
				break;
			case Float3Parameter float3Parameter:
				float3Parameter.UI = float3Parameter.UI with { ShowTypeProperty = true, ShowStepProperty = true };
				parameter = float3Parameter;
				break;
			case Float4Parameter float4Parameter:
				float4Parameter.UI = float4Parameter.UI with { ShowTypeProperty = true, ShowStepProperty = true };
				break;
			case ColorParameter colorParameter:
				colorParameter.UI = colorParameter.UI with { ShowTypeProperty = false, ShowStepProperty = false };
				break;
		}

		_selectedParameter = parameter;

		_parameterListView.SelectItem( parameter );

		_deleteButton.Enabled = true;
	}

	public void ClearSeletedItem()
	{
		_selectedParameter = null;
		_parameterListView.Selection.Clear();

		_deleteButton.Enabled = false;
	}
}
