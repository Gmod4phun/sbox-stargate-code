using Editor;

namespace ShaderGraphPlus;

internal class ProjectTemplates : Widget
{
	public ProjectTemplatesListView ListView { get; set; }

	public ProjectTemplates( Widget parent ) : base( parent, false )
	{
		Layout = Layout.Column( false );
		Layout.Spacing = 8f;

		ListView = new ProjectTemplatesListView( this );
		ListView.SetSizeMode( SizeMode.Default, SizeMode.Default );
		ListView.Layout = Layout.Row( false );

		Layout.Add( ListView, 1 );
	}
}
