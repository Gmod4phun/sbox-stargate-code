using Editor;
using MaterialDesign;
using ShaderGraphPlus.Internal;
using static ShaderGraphPlus.ShaderGraphPlus;

namespace ShaderGraphPlus;

internal static class ShaderGraphPlusEditorMenus
{
	[Menu( "Editor", "Shader Graph Plus/Convert ShaderGraph projects to ShaderGraphPlus projects ( Experimental )" )]
	public static void ConvertShaderGraphToShaderGraphPlus()
	{
		var projectPaths = Directory.GetFiles( $"{Project.Current.GetAssetsPath()}/shaders", "*.shdrgrph", SearchOption.AllDirectories ).ToList();
		var subgraphProjectPaths = Directory.GetFiles( $"{Project.Current.GetAssetsPath()}/shaders", "*.shdrfunc", SearchOption.AllDirectories ).ToList();

		if ( subgraphProjectPaths != null )
		{
			projectPaths.AddRange( subgraphProjectPaths );
		}

		var projectItems = new List<ProjectItem>();
		foreach ( var path in projectPaths )
		{
			//SGPLog.Info( $"project at path \"{path}\"" );
			var extention = Path.GetExtension( path );
			projectItems.Add( new ProjectItem( path.Replace( '\\', '/' ), extention == ".shdrfunc" ) );
		}

		ProjectConverterDialog.DisplayDialog( projectItems );
	}


	/*
	[Menu( "Editor", "Shader Graph Plus/Update Subgraphs internal Path String" )]
	public static void UpdateSubgraphsInternalPath()
	{
		var projectPaths = Directory.GetFiles( $"{Project.Current.GetAssetsPath()}/shaders", $"*.{ShaderGraphPlusGlobals.SubgraphAssetTypeExtension}", SearchOption.AllDirectories );

		if ( projectPaths.Any() )
		{
			SGPLog.Info( $"Found \"{projectPaths.Count()}\" subgraphs" );

			foreach ( var projectPath in projectPaths )
			{
				var graph = new ShaderGraphPlus();
				var file = System.IO.File.ReadAllText( projectPath );

				graph.Deserialize( file );

				var asset = AssetSystem.FindByPath( projectPath );

				var oldPath = graph.Path;
				graph.Path = asset.RelativePath;
				graph.IsSubgraph = true;

				SGPLog.Info( $"Upgraded project subgraphPath from \"{oldPath}\" to \"{graph.Path}\"" );

				System.IO.File.WriteAllText( asset.AbsolutePath, graph.Serialize() );
				asset ??= AssetSystem.RegisterFile( asset.AbsolutePath );
			}
		}

		EditorUtility.DisplayDialog( "", $"Updated \"{projectPaths.Count()}\" subgraphs internal path property." );
	}
	*/
}

file class ProjectConverterDialog : Dialog
{
	private ProjectList _projectList;

	private Layout ListLayout;
	private Layout ButtonLayout;

	private Button ConvertButton;
	private Button CloseButton;

	public Action ConverButtonClick { get; set; }

	public ProjectConverterDialog( List<ProjectItem> projectItems ) : base( null, true )
	{
		Window.FixedWidth = 700f;
		Window.MaximumSize = Window.Size;
		Window.MinimumSize = Window.Size;
		Window.Title = "Convert ShaderGraph To ShaderGraphPlus ( Experimental )";
		Window.SetWindowIcon( MaterialIcons.Gradient );
		Window.SetModal( true, true );

		CreateUI();

		SetItems( projectItems );
	}

	private void CreateUI()
	{
		Layout = Layout.Column();
		Layout.Spacing = 3;

		Layout.AddSpacingCell( 8 );

		var label = Layout.Add( new Label() );
		label.Text = "Projects to convert";
		label.Alignment = TextFlag.Center;

		ListLayout = Layout.AddColumn();
		ListLayout.Add( _projectList = new ProjectList( this ) );
		ListLayout.Spacing = 8f;
		ListLayout.Margin = 16f;

		ButtonLayout = Layout.AddRow();
		ButtonLayout.Spacing = 8f;
		ButtonLayout.Margin = 16f;

		ConvertButton = ButtonLayout.Add( new Button.Primary( "Convert Projects" )
		{
			Clicked = delegate
			{
				ConverButtonClick?.Invoke();
				ConvertButton.Enabled = false;
			}
		} );

		CloseButton = ButtonLayout.Add( new Button( "Close" )
		{
			Clicked = delegate
			{
				Destroy();
				Close();
			}
		} );
	}

	private void SetItems( List<ProjectItem> projectItems )
	{
		_projectList.Projects = projectItems;
	}

	protected override bool OnClose()
	{
		return true;
	}

	public static void DisplayDialog( List<ProjectItem> projectItems )
	{
		var dialog = new ProjectConverterDialog( projectItems );

		foreach ( var project in projectItems )
		{
			var shaderGraph = new Editor.ShaderGraph.ShaderGraph();
			shaderGraph.Deserialize( System.IO.File.ReadAllText( project.Path ) );

			projectItems[projectItems.IndexOf( project )].NodeCount = shaderGraph.Nodes.Count();
		}

		dialog.SetItems( projectItems );

		dialog.ConverButtonClick = delegate
		{
			ConvertProjects( dialog );
		};

		dialog.SetModal( on: true, application: true );
		dialog.Hide();
		dialog.Show();
	}

	private static void ConvertProjects( ProjectConverterDialog dialog )
	{
		var projects = dialog._projectList.Projects;

		foreach ( var project in projects )
		{
			SGPLog.Info( $"Converting project at path \"{project.Path}\"" );

			var shaderGraph = new Editor.ShaderGraph.ShaderGraph();
			shaderGraph.Deserialize( System.IO.File.ReadAllText( project.Path ) );
			var shaderGraphPlus = new ShaderGraphPlus();

			var projectConverter = new ProjectConverter( shaderGraph, shaderGraphPlus, project.IsSubgraph );
			var conversionResult = projectConverter.Convert();

			if ( !projectConverter.Errored )
			{
				var extension = Path.GetExtension( project.Path );
				var targetExtension = extension switch
				{
					".shdrgrph" => $".{ShaderGraphPlusGlobals.AssetTypeExtension}",
					".shdrfunc" => $".{ShaderGraphPlusGlobals.SubgraphAssetTypeExtension}",
					_ => throw new NotImplementedException()
				};

				var shaderGraphPlusFullPath = project.Path.Replace( extension, targetExtension );

				System.IO.File.WriteAllText( shaderGraphPlusFullPath, conversionResult.Serialize() );

				var asset = AssetSystem.RegisterFile( shaderGraphPlusFullPath );

				if ( asset == null )
				{
					SGPLog.Error( $"Unable to register asset at path \"{shaderGraphPlusFullPath}\"" );
					Utilities.EdtiorSound.Failure();
				}
				else
				{
					Utilities.EdtiorSound.Success();
					projects[projects.IndexOf( project )].SetConverted();
				}
			}
			else
			{
				SGPLog.Error( $"Unable to convert shadergraph project at path \"{project.Path}\"" );
				Utilities.EdtiorSound.Failure();
			}
		}

		dialog.SetItems( projects );
	}
}

class ProjectItem
{
	public bool Converted { get; private set; } = false;
	public string Path { get; private set; }
	public bool IsSubgraph { get; private set; } = false;
	public int NodeCount { get; set; } = 0;


	public ProjectItem( string path, bool isSubgraph )
	{
		Path = path;
		IsSubgraph = isSubgraph;
	}

	public void SetConverted()
	{
		Converted = true;
	}
}

class ProjectList : Widget
{
	private List<ProjectItem> _projects;
	public List<ProjectItem> Projects
	{
		get
		{
			return _projects;
		}
		set
		{
			_projectListView.Clear();

			_projects = value;
			_projectListView.SetItems( _projects.Cast<object>() );
		}
	}

	private ProjectListView _projectListView;

	public void UpdateList( List<ProjectItem> items )
	{
		Projects = items;
	}

	public ProjectList( Widget parent ) : base( parent )
	{
		Name = "ShaderGraph Projects";
		WindowTitle = "ShaderGraph Projects";

		SetWindowIcon( "notes" );

		Layout = Layout.Column();

		_projectListView = new ProjectListView( this );
		Layout.Add( _projectListView );
	}

	class ProjectListView : ListView
	{
		private ProjectList _projectList;

		public ProjectListView( ProjectList parent ) : base( parent )
		{
			_projectList = parent;

			ItemClicked = ( item ) =>
			{
				SGPLog.Info( $"Clicked Item" );
			};

			ItemContextMenu = OpenItemContextMenu;
			ItemSize = new Vector2( 0, 48 );
			Margin = 0;
		}

		private void OpenItemContextMenu( object item )
		{

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
			if ( item.Object is ProjectItem projectItem )
			{
				var color = Theme.Text;

				if ( projectItem.Converted )
				{
					color = Theme.Green;
				}

				Paint.SetBrush( color.WithAlpha( Paint.HasMouseOver ? 0.1f : 0.03f ) );
				Paint.ClearPen();
				Paint.DrawRect( item.Rect.Shrink( 0, -1 ) );

				Paint.Antialiasing = true;
				Paint.SetPen( color.WithAlpha( Paint.HasMouseOver ? 1 : 0.7f ), 3.0f );
				Paint.ClearBrush();

				var iconRect = item.Rect.Shrink( 12, 0 );
				iconRect.Width = 24;

				Paint.DrawIcon( iconRect, "account_tree", 24 );

				var rect = item.Rect.Shrink( 48, 8, 0, 8 );

				Paint.SetPen( Color.White.WithAlpha( Paint.HasMouseOver ? 1 : 0.8f ), 3.0f );
				Paint.DrawText( rect, $"{projectItem.Path} - {projectItem.NodeCount} nodes", TextFlag.LeftCenter | TextFlag.SingleLine );

				//Paint.DrawText( rect, $"{projectItem.NodeCount} nodes", TextFlag.RightCenter | TextFlag.SingleLine );
			}
		}
	}
}
