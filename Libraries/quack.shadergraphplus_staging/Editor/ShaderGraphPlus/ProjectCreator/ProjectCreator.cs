using Editor;
using MaterialDesign;
using static Editor.WidgetGalleryWindow;

namespace ShaderGraphPlus;

internal class FieldTitle : Label
{
	public FieldTitle( string title )
	   : base( title, (Widget)null )
	{
	}

}

internal class FieldSubtitle : Label
{
	public FieldSubtitle( string title ) : base( title, null )
	{
		WordWrap = true;
	}
}

public class ProjectCreator : Dialog
{
	private Button OkayButton;

	private LineEdit TitleEdit;

	private FolderEdit FolderEdit;

	public string FolderEditPath
	{
		get => FolderEdit.Text;
		set
		{
			FolderEdit.Text = value;
		}
	}

	//private FieldSubtitle FolderFullPath;

	private ProjectTemplate ActiveTemplate;

	private ProjectTemplates Templates;

	//private ErrorBox FolderError;

	public Action<string> OnProjectCreated { get; set; }

	private bool NoTemplates { get; set; } = false;

	// TODO : Add in some extra options to the template metadata. Something like the ability to further configure the selected template such as shading model and the shader description.
	//

	private TemplateUserConfig templateUserConfig;
	private bool debugLayout = false;

	//private Layout headerLayout;

	public ProjectCreator( Widget parent = null ) : base( null, true )
	{
		// Set some basic window stuff.
		Window.Size = new Vector2( 800, 500 );
		Window.MaximumSize = Window.Size;
		Window.MinimumSize = Window.Size;
		Window.Title = "Create New Shadergraph Plus Project";
		Window.SetWindowIcon( MaterialIcons.Gradient );
		Window.SetModal( true, true );
		//Window.WindowFlags = WindowFlags.Dialog | WindowFlags.Customized | WindowFlags.WindowTitle | WindowFlags.CloseButton | WindowFlags.WindowSystemMenuHint;

		Init();
		OkayButton.Enabled = true;
	}


	private void Init()
	{

		// Start laying stuff out.
		//Layout = Layout.Row();

		debugLayout = false;

		Layout = Layout.Column();
		Layout.Spacing = 3;

		// Header
		/*
        {
            if (debugLayout)
            {
                Layout headerBody = Layout.AddColumn(2, false);
                headerLayout = headerBody;
                headerBody.Add(new ColouredLabel(Theme.Green, $"Header Layout \n InnerRect Size : {headerBody.InnerRect.Size} \n OuterRect Size : {headerBody.OuterRect.Size} \n Margin : {headerBody.Margin.Position.x} \n Spacing : {headerBody.Spacing}"), 2);

            }
            else
            {
                Layout headerBody = Layout.AddColumn(2, false);
                headerBody.Add(new ColouredLabel(Theme.Red, "Templates"), 2);
                headerBody.Add(new Label.Subtitle("Templates"), 2);
                headerBody.AddStretchCell(64);
            }
        }
        */

		// Templates ListView & Template setup
		{
			var row = Layout.AddRow( 8 );

			row.AddColumn();

			// Templates ListView
			if ( debugLayout )
			{
				Layout listViewBody = row.AddColumn( 2, false );
				listViewBody.Margin = 20f;
				listViewBody.Spacing = 8f;
				listViewBody.Add( new ColouredLabel( Theme.Red, $"Templates List View Layout \n InnerRect Size : {listViewBody.InnerRect.Size} \n OuterRect Size : {listViewBody.OuterRect.Size} \n Margin : {listViewBody.Margin.Position.x} \n Spacing : {listViewBody.Spacing}" ), 2 );
			}
			else
			{
				Layout listViewBody = row.AddColumn( 2, false );
				listViewBody.Margin = 20f;
				listViewBody.Spacing = 8f;

				listViewBody.AddSpacingCell( 16f );

				listViewBody.Add( new FieldTitle( "Templates" ) );

				listViewBody.AddSpacingCell( 16f );

				listViewBody.AddSeparator();

				ProjectTemplates templates = listViewBody.Add( new ProjectTemplates( this ), 2 );

				Templates = templates;

				// Template list view for all the projects in the templates folder.
				ProjectTemplatesListView listView = Templates.ListView;

				listView.ItemSelected = (Action<object>)Delegate.Combine( listView.ItemSelected, delegate ( object item )
				{
					ActiveTemplate = item as ProjectTemplate;
				} );

				ActiveTemplate = templates.ListView.ChosenTemplate; // Set the intital selected template.

				if ( !Diagnostics.Assert.Check( ActiveTemplate, null ) )
				{
					Log.Info( $"Active template : {ActiveTemplate.TemplatePath}" );
				}

				//listViewBody.AddSpacingCell(128f);

			}

			row.AddColumn();

			if ( debugLayout )
			{
				Layout setupBody = row.AddColumn( 2, false );
				setupBody.Margin = 20f;
				setupBody.Spacing = 8f;
				setupBody.Add( new ColouredLabel( Theme.Blue, $"Template Setup Layout \n InnerRect Size : {setupBody.InnerRect.Size} \n OuterRect Size : {setupBody.OuterRect.Size} \n Margin : {setupBody.Margin.Position.x} \n Spacing : {setupBody.Spacing}" ), 2 );
			}
			else
			{
				Layout setupBody = row.AddColumn( 2, false );
				setupBody.Margin = 20f;
				setupBody.Spacing = 8f;

				setupBody.AddSpacingCell( 16f );

				setupBody.Add( new FieldTitle( "Shader Graph Plus Project Setup" ) );

				setupBody.AddSpacingCell( 16f );

				setupBody.AddSeparator();

				setupBody.Add( new FieldTitle( "Name" ) );
				{
					TitleEdit = setupBody.Add( new LineEdit( "", null )
					{
						PlaceholderText = "Garry's Project"
					} );
					TitleEdit.Text = DefaultProjectName();
					TitleEdit.ToolTip = "Name of your Shader Graph Plus project.";
					TitleEdit.TextEdited += delegate
					{
						Validate();
					};
				}

				setupBody.AddSpacingCell( 16 );

				// Folder Edit.
				setupBody.Add( new FieldTitle( "Location" ) );
				{
					FolderEdit = setupBody.Add( new FolderEdit( null ) );
					FolderEdit.PlaceholderText = "";
					FolderEdit.ToolTip = "Absolute path to where the Shader Graph Plus project will be saved to.";
					FolderEdit.TextEdited += delegate
					{
						Validate();
					};
					FolderEdit folderEdit = FolderEdit;
					folderEdit.FolderSelected = (Action<string>)Delegate.Combine( folderEdit.FolderSelected, (Action<string>)delegate
					{
						Validate();
					} );
				}

				setupBody.AddSpacingCell( 16 );

				// Additional per-template config. 

				setupBody.Add( new FieldTitle( "Config" ) );
				{

					templateUserConfig = new TemplateUserConfig();

					var canvas = new Widget( null );
					canvas.Layout = Layout.Row();
					canvas.Layout.Spacing = 32;

					var so = templateUserConfig.GetSerialized();
					var cs = new ControlSheet();
					//canvas.MinimumWidth = 350;

					cs.AddProperty( templateUserConfig, x => x.Description );

					setupBody.Add( cs );
				}


				// Create button & any errors.
				{
					OkayButton = new Button.Primary( "Create", "add_box", null );
					OkayButton.Clicked = CreateProject;

					var footer = Layout.AddRow( 2, false );
					footer.Margin = 16;
					footer.Spacing = 8;
					footer.AddStretchCell();

					// Handle situations where there is no templates found.
					if ( !Diagnostics.Assert.Check( Templates.ListView.Items.Count(), 0 ) )
					{
						ActiveTemplate = Templates.ListView.SelectedItems.First() as ProjectTemplate;
					}
					else
					{
						NoTemplates = true;
						Layout error = footer.AddColumn( 2, false );
						error.Spacing = 8f;
						error.AddStretchCell( 0 );
						var errorlabel = new Label( "No Templates found!" );
						errorlabel.Color = Color.Red;
						error.Add( errorlabel );
					}

					footer.Add( OkayButton );
				}

				setupBody.AddSpacingCell( 16f );
			}
		}

		Validate();
	}

	protected override void OnPaint()
	{
#if false
		Paint.BilinearFiltering = true;
		Paint.ClearPen();
		
		Rect rect = new Rect(new Vector2(0, 0), new Vector2(Width, headerLayout.InnerRect.Size.y));
		var aPos = rect.TopLeft;
		var bPos = rect.BottomLeft;
		var aColor = Theme.Blue.WithAlpha(0);
		var bColor = Theme.Blue.WithAlpha(0.5f);
		Paint.SetBrushLinear(aPos, bPos, aColor, bColor);
		Paint.DrawRect(rect);
		
		Paint.RenderMode = RenderMode.Screen;
		var pos = new Vector2(64 + 16 + 16, 16 + 8);
		
		Paint.RenderMode = RenderMode.Normal;
		
		Paint.SetPen(Color.White.WithAlpha(0.9f));
		Paint.SetDefaultFont();
#else
		Paint.ClearPen();
		Paint.SetBrush( Theme.WindowBackground.Lighten( 0.4f ) );
		Paint.DrawRect( LocalRect );
#endif
	}

	private static string DefaultProjectName()
	{
		string name = "My Shadergraph Plus Project";
		int i = 1;
		//while (Path.Exists(Path.Combine(EditorPreferences.AddonLocation, ConvertToIdent(name))))
		//{
		name = $"My Project {i++}";
		//}
		return name;
	}

	private void Validate()
	{
		bool enabled = true;

		if ( string.IsNullOrWhiteSpace( FolderEdit.Text ) )
		{
			enabled = false;
		}

		if ( string.IsNullOrWhiteSpace( TitleEdit.Text ) )
		{
			enabled = false;
		}

		OkayButton.Enabled = enabled;
	}

	private void ConfigureTemplate( ShaderGraphPlus shaderGraphPlusTemplate )
	{
		//if (shaderGraphPlusTemplate.MaterialDomain is not MaterialDomain.PostProcess)
		//{
		//    shaderGraphPlusTemplate.BlendMode = templateUserConfig.blendmode;
		//}
		shaderGraphPlusTemplate.Description = templateUserConfig.Description;
		//shaderGraphPlusTemplate.ShadingModel = templateUserConfig.shadingmodel;
	}

	private ShaderGraphPlus ReadTemplate( string templatePath )
	{
		var shaderGraphPlusTemplate = new ShaderGraphPlus();
		shaderGraphPlusTemplate.Deserialize( System.IO.File.ReadAllText( ShaderGraphPlusFileSystem.Root.GetFullPath( $"{templatePath}/$name.{ShaderGraphPlusGlobals.AssetTypeExtension}" ) ) );

		// configure the template.
		ConfigureTemplate( shaderGraphPlusTemplate );

		shaderGraphPlusTemplate.SetMeta( "ProjectTemplate", null );

		return shaderGraphPlusTemplate;
	}

	private void CreateProject()
	{
		// No templates? then dont run the rest of the code...
		if ( NoTemplates )
		{
			return;
		}

		string shaderGraphProjectPath = $"{FolderEdit.Text}/";//ShaderGraphPlusFileSystem.FileSystem.GetFullPath($"Assets/{FolderEdit.Text}");
		Directory.CreateDirectory( shaderGraphProjectPath );

		//Log.Info($"Chosen Template is : {Templates.ListView.ChosenTemplate.TemplatePath}");

		string OutputPath = Path.Combine( shaderGraphProjectPath, $"{TitleEdit.Text}.{ShaderGraphPlusGlobals.AssetTypeExtension}" ).Replace( '\\', '/' );
		string txt = ReadTemplate( $"{Templates.ListView.ChosenTemplate.TemplatePath}" ).Serialize();
		File.WriteAllText( OutputPath, txt );

		// Register the generated project with the assetsystem.
		AssetSystem.RegisterFile( OutputPath );

		//Log.Info($"Creating ShaderGraphPlus project from : {Templates.ListView.ChosenTemplate.TemplatePath}");
		Utilities.EdtiorSound.Success();
		Close();

		OnProjectCreated?.Invoke( OutputPath );
	}


	[EditorEvent.Hotload]
	public void OnHotload()
	{
		Init();
	}
}
