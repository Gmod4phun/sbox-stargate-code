using Editor;
using ShaderGraphPlus.Nodes;
using static ShaderGraphPlus.ShaderGraphPlusGlobals;

namespace ShaderGraphPlus;

[CustomEditor( typeof( string ), NamedEditor = ControlWidgetCustomEditors.NamedRerouteReferenceEditor )]
internal sealed class NamedRerouteReferenceControlWidget : DropdownControlWidget<string>
{
	ShaderGraphPlus Graph;
	NamedRerouteNode Node;

	public NamedRerouteReferenceControlWidget( SerializedProperty property ) : base( property )
	{
		Node = property.Parent.Targets.OfType<NamedRerouteNode>().FirstOrDefault();
		Graph = (ShaderGraphPlus)Node.Graph;

		if ( Graph is null ) return;

		if ( string.IsNullOrWhiteSpace( SerializedProperty.GetValue<string>() ) )
		{
			SerializedProperty.SetValue<string>( "None" );
		}
	}

	protected override IEnumerable<object> GetDropdownValues()
	{
		List<object> list = new();
		list.Add( "None" );

		foreach ( var namedRerouteDeclaration in Graph.Nodes.OfType<NamedRerouteDeclarationNode>() )
		{
			var entry = new Entry();
			entry.Value = namedRerouteDeclaration.Name;
			entry.Label = namedRerouteDeclaration.Name;
			entry.Description = "";
			list.Add( entry );
		}

		return list;
	}

	protected override void OnPaint()
	{
		base.OnPaint();
	}
}
