using Editor;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace NodeEditorPlus;

public interface INodeGraph
{
	public IEnumerable<IGraphNode> Nodes { get; }

	void AddNode( IGraphNode node );
	void RemoveNode( IGraphNode node );

	string SerializeNodes( IEnumerable<IGraphNode> nodes );
	IEnumerable<IGraphNode> DeserializeNodes( string serialized );
}

public interface INodeType
{
	public static bool DefaultMatches( INodeType nodeType, NodeQuery query )
	{
		return DefaultGetScore( nodeType, query ) != null;
	}

	public static int? DefaultGetScore( INodeType nodeType, NodeQuery query )
	{
		if ( !nodeType.IsCommon && query.Filter.Count == 0 ) return null;
		if ( query.GetScore( nodeType.Path ) is not { } score ) return null;

		var plugType = GetPlugType( query.Plug );
		var matchesPlug = query.Plug switch
		{
			IPlugIn plugIn => nodeType.TryGetOutput( plugType, out _ ),
			IPlugOut plugOut => nodeType.TryGetInput( plugType, out _ ),
			_ => true
		};

		return matchesPlug ? score : null;
	}

	private static Type GetPlugType( IPlug? plug )
	{
		var plugType = plug?.Type ?? typeof( object );

		if ( plugType == typeof( object ) )
		{
			if ( plug is IPlugOut plugOut && plugOut.Node is IRerouteNode reroute )
			{
				var connected = reroute.Inputs.FirstOrDefault()?.ConnectedOutput;
				if ( connected is not null )
				{
					return GetPlugType( connected );
				}
			}
		}

		return plugType;
	}

	/// <summary>
	/// If true, include this type in the node menu even without a search filter.
	/// </summary>
	bool IsCommon => true;
	Menu.PathElement[] Path { get; }
	bool TryGetInput( Type valueType, [NotNullWhen( true )] out string? name );
	bool TryGetOutput( Type valueType, [NotNullWhen( true )] out string? name );
	IGraphNode CreateNode( INodeGraph graph );

	public bool Matches( NodeQuery query ) => GetScore( query ) is not null;
	public int? GetScore( NodeQuery query ) => DefaultGetScore( this, query );
}

public interface IGraphNode
{
	event Action Changed;

	string Identifier { get; }
	string Subtitle { get; }
	DisplayInfo DisplayInfo { get; }

	bool CanClone { get; }
	bool CanRemove { get; }

	Vector2 Position { get; set; }
	Vector2 ExpandSize { get; }

	bool AutoSize { get; }

	public IEnumerable<IPlugIn> Inputs { get; }
	public IEnumerable<IPlugOut> Outputs { get; }

	public string? ErrorMessage { get; }
	public bool IsReachable { get; }

	Pixmap? Thumbnail { get; }
	void OnPaint( Rect rect );
	void OnDoubleClick( MouseEvent e );
	bool HasTitleBar { get; }
	bool HasSubtitle { get; }
	bool HasError { get; set; }
	bool HasWarning { get; set; }

	NodeUI CreateUI( GraphView view );
	Color GetNodeTitleColor( GraphView view );
	Color GetNodeBodyTintColor( GraphView view );
	Menu? CreateContextMenu( NodeUI node );

	Action? GoToDefinition => null;
}

public interface IRerouteNode : IGraphNode
{
	string? Comment { get; set; }
}

public interface IPlug
{
	IGraphNode Node { get; }
	string Identifier { get; }
	Type Type { get; set; } // { get; }

	DisplayInfo DisplayInfo { get; }
	ValueEditor CreateEditor( NodeUI node, NodePlug plug );
	Menu? CreateContextMenu( NodeUI node, NodePlug plug );

	void OnDoubleClick( NodeUI node, NodePlug plug, MouseEvent e );

	bool ShowLabel { get; }
	bool AllowStretch { get; }
	bool ShowConnection { get; }
	bool InTitleBar { get; }
	bool IsReachable { get; }

	string ErrorMessage { get; }
}

public interface IPlugIn : IPlug
{
	IPlugOut? ConnectedOutput { get; set; }
	float? GetHandleOffset( string name );
	void SetHandleOffset( string name, float? value );
}

public interface IPlugOut : IPlug
{

}

/// <summary>
/// Describes the colors of this node.
/// </summary>
/// <param name="PrimaryColor">Primary base color of a node.</param>
/// <param name="HeaderLeftColor">Leftmost color of a node header. </param>
/// <param name="HeaderRightColor">Rightmost color of a node header. </param>
public record struct NodeThemeConfig( Color PrimaryColor, Color HeaderLeftColor, Color HeaderRightColor );
