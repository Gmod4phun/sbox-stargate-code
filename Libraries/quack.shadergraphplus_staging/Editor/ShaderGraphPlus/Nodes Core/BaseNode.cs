using Editor;

namespace ShaderGraphPlus;

[System.AttributeUsage( AttributeTargets.Class )]
internal class SubgraphOnlyAttribute : Attribute
{
	public SubgraphOnlyAttribute()
	{
	}
}

public abstract class BaseNodePlus : IGraphNode, ISGPJsonUpgradeable
{
	public event Action Changed;

	/// <summary>
	/// Current version of this node. Used by ISGPJsonUpgradeable
	/// </summary>
	[Hide]
	public abstract int Version { get; }

	[Hide, Browsable( false )]
	public string Identifier { get; set; }

	[JsonIgnore, Hide, Browsable( false )]
	public virtual string Subtitle { get; }

	[JsonIgnore, Hide, Browsable( false )]
	public virtual DisplayInfo DisplayInfo { get; }

	[JsonIgnore, Hide, Browsable( false )]
	public bool CanClone => true;

	[JsonIgnore, Hide, Browsable( false )]
	public virtual bool CanRemove => true;

	[JsonIgnore, Hide, Browsable( false )]
	public virtual bool CanPreview => true;

	[JsonIgnore, Hide, Browsable( false )]
	public virtual bool CanAddToGraph => true;

	[Hide, Browsable( false )]
	public Vector2 Position { get; set; }

	[JsonIgnore, Hide]
	public INodeGraph _graph;

	[JsonIgnore, Hide, Browsable( false )]
	internal int PreviewID { get; set; }

	[JsonIgnore, Hide, Browsable( false )]
	public bool Processed { get; set; } = false;

	[JsonIgnore, Hide, Browsable( false )]
	public bool UpgradedToNewNode { get; set; } = false;

	[JsonIgnore, Hide, Browsable( false )]
	public INodeGraph Graph
	{
		get => _graph;
		set
		{
			_graph = value;
			FilterInputsAndOutputs();
		}
	}

	[JsonIgnore, Hide, Browsable( false )]
	public Vector2 ExpandSize { get; set; }

	[JsonIgnore, Hide, Browsable( false )]
	public bool AutoSize => false;

	[JsonIgnore, Hide, Browsable( false )]
	public virtual IEnumerable<IPlugIn> Inputs { get; protected set; }

	[JsonIgnore, Hide, Browsable( false )]
	public virtual IEnumerable<IPlugOut> Outputs { get; protected set; }

	[JsonIgnore, Hide, Browsable( false )]
	public string ErrorMessage => null;

	[JsonIgnore, Hide, Browsable( false )]
	public bool IsReachable => true;

	[Hide, Browsable( false )]
	public Dictionary<string, float> HandleOffsets { get; set; } = new();

	public BaseNodePlus()
	{
		DisplayInfo = DisplayInfo.For( this );
		NewIdentifier();

		(Inputs, Outputs) = GetPlugs( this );
	}

	public override string ToString()
	{
		return $"{DisplayInfo.Fullname}.{Identifier}";
	}

	public void Update()
	{
		Changed?.Invoke();
	}

	public virtual void OnFrame()
	{

	}

	public string NewIdentifier()
	{
		Identifier = Guid.NewGuid().ToString();
		return Identifier;
	}

	public virtual NodeUI CreateUI( GraphView view )
	{
		return new NodeUI( view, this, false );
	}

	public Color GetNodeBodyTintColor( GraphView view )
	{
		return NodeBodyTintColor;
	}

	public Color GetNodeTitleColor( GraphView view )
	{
		return NodeTitleColor;
	}

	public virtual Menu CreateContextMenu( NodeUI node )
	{
		return null;
	}

	public virtual void DebugInfo( Menu menu )
	{
		var debugInfoHeading = menu.AddHeading( "Node Debug Info" );

		menu.AddWidget( new Label( $"Node ID : {this.Identifier}" ) );
		if ( this is IBlackboardSyncableNode blackboardSyncable )
		{
			menu.AddWidget( new Label( $"Blackboard ID : {blackboardSyncable.BlackboardParameterIdentifier}" ) ).AdjustSize();
		}
		menu.AddWidget( new Label( $"Preview ID : {this.PreviewID}" ) );
		menu.AddWidget( new Label( $"IsReachable? : {this.IsReachable}" ) );
		menu.AddWidget( new Label( $"CanPreview? : {this.CanPreview}" ) );
	}

	[JsonIgnore, Hide, Browsable( false )]
	public virtual Pixmap Thumbnail { get; }

	[JsonIgnore, Hide, Browsable( false )]
	public virtual Color NodeBodyTintColor { get; set; } = Color.Parse( "#303030" )!.Value.Lighten( 2.0f );

	[JsonIgnore, Hide, Browsable( false )]
	public virtual Color NodeTitleColor { get; set; } = Color.Gray;

	public virtual void OnPaint( Rect rect )
	{

	}

	public virtual void OnDoubleClick( MouseEvent e )
	{

	}

	[JsonIgnore, Hide, Browsable( false )]
	public bool HasTitleBar => true;

	[JsonIgnore, Hide, Browsable( false )]
	public bool HasSubtitle => !string.IsNullOrWhiteSpace( Subtitle );

	private bool _hasError;
	[JsonIgnore, Hide, Browsable( false )]
	public bool HasError
	{
		get => _hasError;
		set
		{
			_hasError = value;
			Update();
		}
	}

	[JsonIgnore, Hide, Browsable( false )]
	public bool HasWarning { get; set; } = false;

	[System.AttributeUsage( AttributeTargets.Property )]
	public class InputAttribute : Attribute
	{
		/// <summary>
		/// Type of the port.
		/// </summary>
		public System.Type Type;

		/// <summary>
		/// Order of the port.
		/// </summary>
		public int Order;

		public InputAttribute( Type type = null, int order = 0 )
		{
			Type = type;
			Order = order;
		}
	}

	[System.AttributeUsage( AttributeTargets.Property )]
	public class InputDefaultAttribute : Attribute
	{
		public string Input;

		public InputDefaultAttribute( string input )
		{
			Input = input;
		}
	}

	[System.AttributeUsage( AttributeTargets.Property )]
	public class OutputAttribute : Attribute
	{
		/// <summary>
		/// Type of the port.
		/// </summary>
		public System.Type Type;

		/// <summary>
		/// Order of the port.
		/// </summary>
		public int Order;

		public OutputAttribute( Type type = null, int order = 0 )
		{
			Type = type;
			Order = order;
		}
	}

	[System.AttributeUsage( AttributeTargets.Property )]
	public class HideOutputAttribute : Attribute
	{
		public System.Type Type;

		public HideOutputAttribute( Type type = null )
		{
			Type = type;
		}
	}

	[System.AttributeUsage( AttributeTargets.Property )]
	public class NodeValueEditorAttribute : Attribute
	{
		public string ValueName;

		public NodeValueEditorAttribute( string valueName )
		{
			ValueName = valueName;
		}
	}

	[System.AttributeUsage( AttributeTargets.Property )]
	public class RangeAttribute : Attribute
	{
		public string Min;
		public string Max;
		public string Step;

		public RangeAttribute( string min, string max, string step )
		{
			Min = min;
			Max = max;
			Step = step;
		}
	}

	/// <summary>
	/// Connects a <see cref="NodeResult.Func"/> property from another node to the <see cref="NodeInput"/> property on this <see cref="BaseNodePlus"/> instance.
	/// </summary>
	/// <param name="inputName">The name of the <see cref="NodeInput"/> property on this <see cref="BaseNodePlus"/> instance. That the specified <see cref="NodeResult.Func"/> property from another node in the graph will be connected to.</param>
	/// <param name="targetOutputName">The name of the <see cref="NodeResult.Func"/> property from another node in the graph that will be connected to a <see cref="NodeInput"/> property on this <see cref="BaseNodePlus"/> instance.</param>
	/// <param name="targetOutputNodeIdentifier">The Identifier of another <see cref="BaseNodePlus"/> from the graph that we are connecting from.</param>
	internal void ConnectNode( string inputName, string targetOutputName, string targetOutputNodeIdentifier )
	{
		if ( Graph == null )
		{
			throw new Exception( $"Graph property on node \"{this}\" is null!!!" );
		}

		var graph = Graph as ShaderGraphPlus;
		var targetOutputNode = graph.Nodes.Where( x => x.Identifier == targetOutputNodeIdentifier ).FirstOrDefault();

		if ( targetOutputNode != null )
		{
			var plugIn = Inputs.Where( x => x.Identifier == inputName ).FirstOrDefault();
			var targetOutputPlug = targetOutputNode.Outputs.FirstOrDefault( x => x.Identifier == targetOutputName );

			if ( plugIn == null )
			{
				SGPLog.Error( $"{graph.Path} Cannot find input with name \"{inputName}\" on node \"{this}\"" );
				return;
			}

			if ( targetOutputPlug == null )
			{
				SGPLog.Error( $"{graph.Path} Cannot find output with name \"{targetOutputName}\" on node \"{targetOutputNode}\"" );
				return;
			}

			//SGPLog.Info( $"{graph.Path} Connecting \"{inputName}\" of graph node \"{this}\" to output \"{targetOutputName}\" of graph node \"{targetOutputNode}\"" );

			plugIn.ConnectedOutput = targetOutputPlug;
		}
		else
		{
			SGPLog.Error( $"{graph.Path} Cannot find node with Identifier \"{targetOutputNodeIdentifier}\"" );
		}
	}

	public static (IEnumerable<IPlugIn> Inputs, IEnumerable<IPlugOut> Outputs) GetPlugs( BaseNodePlus node )
	{
		var type = node.GetType();
		var inputs = new List<BasePlugIn>();
		var outputs = new List<BasePlugOut>();

		var inputProperties = type.GetProperties().OrderBy( x =>
			(x.GetCustomAttribute<InputAttribute>() is InputAttribute input) ? input.Order : 0 );

		foreach ( var propertyInfo in inputProperties )
		{
			if ( propertyInfo.GetCustomAttribute<InputAttribute>() is { } inputAttrib )
			{
				inputs.Add( new BasePlugIn( node, new( propertyInfo ), inputAttrib.Type ?? typeof( object ) ) );
			}
		}

		var outputProperties = type.GetProperties().OrderBy( x =>
			(x.GetCustomAttribute<OutputAttribute>() is OutputAttribute output) ? output.Order : 0 );

		foreach ( var propertyInfo in outputProperties )
		{
			if ( propertyInfo.GetCustomAttribute<OutputAttribute>() is { } outputAttrib )
			{
				outputs.Add( new BasePlugOut( node, new( propertyInfo ), outputAttrib.Type ?? typeof( object ) ) );
			}
		}

		return (inputs, outputs);
	}

	private void FilterInputsAndOutputs()
	{
		if ( _graph is not null )
		{
			if ( Graph is ShaderGraphPlus sg && !sg.IsSubgraph && this is IParameterNode pn )
			{
				Inputs = new List<IPlugIn>();
			}
		}
	}

}

public record BasePlug( BaseNodePlus Node, PlugInfo Info, Type Type ) : IPlug
{
	IGraphNode IPlug.Node => Node;

	Type IPlug.Type { get; set; } = Type;

	public string Identifier => Info.Name;
	public DisplayInfo DisplayInfo => Info.DisplayInfo;

	public ValueEditor CreateEditor( NodeUI node, NodePlug plug )
	{
		var editor = Info.CreateEditor( node, plug, Type );
		if ( editor is not null ) return editor;

		// Default
		{
			var defaultEditor = new DefaultEditor( plug );
		}

		return null;
	}

	public Menu CreateContextMenu( NodeUI node, NodePlug plug )
	{
		return null;
	}

	public void OnDoubleClick( NodeUI node, NodePlug plug, MouseEvent e )
	{

	}

	public bool ShowLabel => true;
	public bool AllowStretch => true;
	public bool ShowConnection => IsReachable;
	public bool InTitleBar => false;

	public bool IsReachable
	{
		get
		{
			var conditional = Info.Property?.GetCustomAttribute<ConditionalVisibilityAttribute>();
			if ( conditional is not null )
			{
				if ( conditional.TestCondition( Node.GetSerialized() ) ) return false;
			}

			return true;
		}
	}

	public string ErrorMessage => null;

	public override string ToString()
	{
		return $"{Node.Identifier}.{Identifier}";
	}

}

public record BasePlugIn( BaseNodePlus Node, PlugInfo Info, Type Type ) : BasePlug( Node, Info, Type ), IPlugIn
{
	IPlugOut IPlugIn.ConnectedOutput
	{
		get
		{
			if ( Info.Property is null )
			{
				return Info.ConnectedPlug;
			}

			if ( Info.Type != typeof( NodeInput ) )
			{
				return null;
			}

			var value = Info.GetInput( Node );

			if ( !value.IsValid )
			{
				return null;
			}


			var node = ((ShaderGraphPlus)Node.Graph).FindNode( value.Identifier );
			var output = node?.Outputs
				.FirstOrDefault( x => x.Identifier == value.Output );

			return output;
		}
		set
		{
			var property = Info.Property;
			if ( property is null )
			{
				Info.ConnectedPlug = value;
				return;
			}

			if ( property.PropertyType != typeof( NodeInput ) )
			{
				return;
			}

			if ( value is null )
			{
				property.SetValue( Node, default( NodeInput ) );
				return;
			}

			if ( value is not BasePlug fromPlug )
			{
				return;
			}

			property.SetValue( Node, new NodeInput
			{
				Identifier = fromPlug.Node.Identifier,
				Output = fromPlug.Identifier
			} );
		}
	}

	public float? GetHandleOffset( string name )
	{
		if ( Node.HandleOffsets.TryGetValue( name, out var value ) )
		{
			return value;
		}
		return null;
	}

	public void SetHandleOffset( string name, float? value )
	{
		if ( value is null ) Node.HandleOffsets.Remove( name );
		else Node.HandleOffsets[name] = value.Value;
	}
}

public record BasePlugOut( BaseNodePlus Node, PlugInfo Info, Type Type ) : BasePlug( Node, Info, Type ), IPlugOut;

public class PlugInfo
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public Type Type { get; set; }
	public DisplayInfo DisplayInfo { get; set; }
	public PropertyInfo Property { get; set; } = null;
	public IPlugOut ConnectedPlug { get; set; } = null;

	public PlugInfo()
	{
		DisplayInfo = new();
	}
	public PlugInfo( PropertyInfo property )
	{
		Name = property.Name;
		Type = property.PropertyType;
		var info = DisplayInfo.ForMember( Type );
		info.Name = property.Name;
		var titleAttr = property.GetCustomAttribute<TitleAttribute>();
		if ( titleAttr is not null )
		{
			info.Name = titleAttr.Value;
		}
		DisplayInfo = info;
		Property = property;
	}

	public NodeInput GetInput( BaseNodePlus node )
	{
		if ( Property is not null )
		{
			return (NodeInput)Property.GetValue( node )!;
		}

		return default;
	}

	public ValueEditor CreateEditor( NodeUI node, NodePlug plug, Type type )
	{
		var editor = Property?.GetCustomAttribute<BaseNodePlus.NodeValueEditorAttribute>();

		if ( editor is not null )
		{
			if ( type == typeof( int ) )
			{
				var slider = new IntValueEditor( plug ) { Title = DisplayInfo.Name, Node = node };
				slider.Bind( "Value" ).From( node.Node, editor.ValueName );

				var range = Property.GetCustomAttribute<BaseNodePlus.RangeAttribute>();
				if ( range != null )
				{
					slider.Bind( "Min" ).From( node.Node, range.Min );
					slider.Bind( "Max" ).From( node.Node, range.Max );
				}
				else if ( Property.GetCustomAttribute<MinMaxAttribute>() is MinMaxAttribute minMax )
				{
					slider.Min = (int)minMax.MinValue;
					slider.Max = (int)minMax.MaxValue;
				}

				return slider;
			}

			if ( type == typeof( float ) )
			{
				var slider = new FloatValueEditor( plug ) { Title = DisplayInfo.Name, Node = node };
				slider.Bind( "Value" ).From( node.Node, editor.ValueName );

				var range = Property.GetCustomAttribute<BaseNodePlus.RangeAttribute>();
				if ( range != null )
				{
					slider.Bind( "Min" ).From( node.Node, range.Min );
					slider.Bind( "Max" ).From( node.Node, range.Max );
					slider.Bind( "Step" ).From( node.Node, range.Step );
				}
				else if ( Property.GetCustomAttribute<MinMaxAttribute>() is MinMaxAttribute minMax )
				{
					slider.Min = minMax.MinValue;
					slider.Max = minMax.MaxValue;
				}

				return slider;
			}

			if ( type == typeof( Color ) )
			{
				var slider = new ColorValueEditor( plug ) { Title = DisplayInfo.Name, Node = node };
				slider.Bind( "Value" ).From( node.Node, editor.ValueName );

				return slider;
			}

			if ( type == typeof( Gradient ) )
			{
				var slider = new GradientValueEditor( plug ) { Title = DisplayInfo.Name, Node = node };
				slider.Bind( "Value" ).From( node.Node, editor.ValueName );

				return slider;
			}
		}
		return null;
	}
}
