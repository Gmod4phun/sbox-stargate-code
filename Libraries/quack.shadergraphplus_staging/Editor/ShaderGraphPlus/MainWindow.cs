using Editor;
using Editor.ShaderGraph;
using NodeEditorPlus;
using Sandbox.Rendering;
using ShaderGraphPlus.Diagnostics;
using ShaderGraphPlus.Nodes;
using System.Collections.Generic;

namespace ShaderGraphPlus;

[EditorForAssetType( ShaderGraphPlusGlobals.SubgraphAssetTypeExtension )]
public class MainWindowFunc : MainWindow, IAssetEditor
{
	public override bool IsSubgraph => true;
	public override string FileType => $"{ShaderGraphPlusGlobals.AssetTypeName} Sub-Graph";
	public override string FileExtension => ShaderGraphPlusGlobals.SubgraphAssetTypeExtension;

	void IAssetEditor.SelectMember( string memberName )
	{
		throw new NotImplementedException();
	}
}

[EditorForAssetType( ShaderGraphPlusGlobals.AssetTypeExtension )]
[EditorApp( ShaderGraphPlusGlobals.AssetTypeName, "gradient", "edit shaders" )]
public class MainWindowShader : MainWindow, IAssetEditor
{
	public override bool IsSubgraph => false;
	public override string FileType => ShaderGraphPlusGlobals.AssetTypeName;
	public override string FileExtension => ShaderGraphPlusGlobals.AssetTypeExtension;

	void IAssetEditor.SelectMember( string memberName )
	{
		throw new NotImplementedException();
	}
}

public class MainWindow : DockWindow
{
	public virtual bool IsSubgraph => false;
	public virtual string FileType => ShaderGraphPlusGlobals.AssetTypeName;
	public virtual string FileExtension => ShaderGraphPlusGlobals.AssetTypeExtension;

	private ShaderGraphPlus _graph;
	private ShaderGraphPlusView _graphView;
	private BlackboardView _blackboardView;
	private Asset _asset;

	public string AssetPath => _asset?.Path;

	private Widget _graphCanvas;
	private Widget _blackboardCanvas;
	private Properties _properties;
	private Preview3DPanel _preview3D;
	private Output _output;
	private UndoHistory _undoHistory;
	private PaletteWidget _palette;
	private TextView _generatedCodeTextView;

	private readonly UndoStack _undoStack = new();

	private Option _undoOption;
	private Option _redoOption;

	private Option _undoMenuOption;
	private Option _redoMenuOption;

	private Option AutoCompileOption;

	private Option _nodeDebugInfoOption;

	public UndoStack UndoStack => _undoStack;

	private bool _dirty = false;
	private bool _autoCompile = true;

	private string _generatedCode;

	private readonly Dictionary<string, bool> _boolAttributes = new();
	private readonly Dictionary<string, int> _intAttributes = new();
	private readonly Dictionary<string, float> _floatAttributes = new();
	private readonly Dictionary<string, Vector2> _float2Attributes = new();
	private readonly Dictionary<string, Vector3> _float3Attributes = new();
	private readonly Dictionary<string, Color> _float4Attributes = new();
	private readonly Dictionary<string, SamplerState> _samplerStateAttributes = new();
	private readonly Dictionary<string, Texture> _textureAttributes = new();
	private readonly Dictionary<string, int> _dynamicComboIntAttributes = new();
	private readonly Dictionary<string, int> _shaderFeatures = new();

	//private readonly List<BaseNodePlus> _compiledNodes = new();

	private bool _isCompiling = false;
	private bool _isPendingCompile = false;
	private RealTimeSince _timeSinceCompile;

	private Menu _recentFilesMenu;
	private readonly List<string> _recentFiles = new();
	private Option _fileHistoryBack;
	private Option _fileHistoryForward;
	private Option _fileHistoryHome;

	private List<string> _fileHistory = new();
	private int _fileHistoryPosition = 0;

	private string _defaultDockState;

	public bool CanOpenMultipleAssets => true;
	public bool EnableNodePreview => _preview3D.Preview.EnableNodePreview;

	private ProjectCreator ProjectCreator { get; set; }

	private Dictionary<string, ShaderFeatureBase> ShaderFeatures = new();
	private List<GraphCompiler.GraphIssue> BlackboardIssues { get; set; } = new();

	public MainWindow()
	{
		DeleteOnClose = true;

		Title = FileType;
		Size = new Vector2( 1700, 1050 );

		_graph = new();
		_graph.IsSubgraph = IsSubgraph;

		CreateToolBar();

		_recentFiles = Editor.FileSystem.Temporary.ReadJsonOrDefault( "shadergraphplus_recentfiles.json", _recentFiles )
			.Where( x => System.IO.File.Exists( x ) ).ToList();

		CreateUI();
		Show();
		CreateNew();

		OpenProjectCreationDialog();
	}

	private void OpenProjectCreationDialog()
	{
		ProjectCreator = new ProjectCreator();
		ProjectCreator.DeleteOnClose = true;

		var initialPath = $"{Project.Current.GetAssetsPath().Replace( "\\", "/" )}/Shaders";
		if ( !Directory.Exists( initialPath ) )
		{
			Directory.CreateDirectory( initialPath );
		}

		ProjectCreator.FolderEditPath = initialPath;
		ProjectCreator.Show();
		ProjectCreator.OnProjectCreated += OpenProject;

	}

	public void AssetOpen( Asset asset )
	{
		if ( asset == null || string.IsNullOrWhiteSpace( asset.AbsolutePath ) )
			return;

		if ( ProjectCreator != null )
		{
			// We dont need the project creator when opening an existing asset. So lets forceably close it.
			ProjectCreator.Close();
			ProjectCreator = null;
		}


		Open( asset.AbsolutePath );
	}

	private void RestoreShader()
	{
		if ( !_preview3D.IsValid() )
			return;

		_preview3D.Material = Material.Load( "materials/core/shader_editor.vmat" );
	}

	public void OnNodeSelected( BaseNodePlus node )
	{
		var oldTarget = _properties.Target;
		_properties.Target = node != null ? node : _graph;

		if ( _properties.Target is not BooleanFeatureSwitchNode && _properties.Target is not EnumFeatureSwitchNode && _properties.Target is IBlackboardSyncableNode blackboardSyncableTest )
		{
			// For now only select a blackboard parameter when _graphView only has 1 selection.
			if ( _graphView.SelectedItems.Count() == 1 )
			{
				if ( blackboardSyncableTest.BlackboardParameterIdentifier != default )
				{
					var blackboardParameter = _graph.FindParameterByGuid( blackboardSyncableTest.BlackboardParameterIdentifier );
					_blackboardView.SetSelectedItem( blackboardParameter );
					_properties.Target = blackboardParameter;
				}
			}
			else
			{
				_properties.Target = _graph;
			}
		}

		if ( _properties.Target is ShaderGraphPlus && oldTarget is BaseBlackboardParameter )
		{
			_blackboardView.ClearSeletedItem();
		}
		else if ( _properties.Target is BaseNodePlus && oldTarget is BaseBlackboardParameter )
		{
			_blackboardView.ClearSeletedItem();
		}

		if ( EnableNodePreview && (node != null && node.CanPreview) )
		{
			SGPLog.Info( $"Node PreviewID is `{node.PreviewID}`", EnableNodePreview );

			_preview3D.SetStage( node.PreviewID );
		}
		else
		{
			SGPLog.Info( $"Graph is now the Target.", EnableNodePreview );
			_preview3D.SetStage( ShaderGraphPlusGlobals.GraphCompiler.NoNodePreviewID );
		}

		//_preview.SetStage( _compiledNodes.IndexOf( node ) + 1 );
	}

	private void OpenGeneratedShader()
	{
		if ( _asset is null )
		{
			Save();
		}
		else
		{
			var path = System.IO.Path.ChangeExtension( _asset.AbsolutePath, ".shader" );
			var asset = AssetSystem.FindByPath( path );

			asset?.OpenInEditor();
		}
	}

	private void OpenTempGeneratedShader()
	{
		if ( !_dirty )
		{
			SetDirty();
		}

		var assetPath = $"shadergraphplus/{_asset?.Name ?? "untitled"}_shadergraphplus.generated.shader";
		var resourcePath = System.IO.Path.Combine( Editor.FileSystem.Temporary.GetFullPath( "/temp" ), assetPath );
		var asset = AssetSystem.FindByPath( resourcePath );

		asset?.OpenInEditor();
	}

	private void OpenShaderGraphProjectTxt()
	{
		if ( _asset is null )
		{
			Save();
		}
		else
		{
			var path = _asset.AbsolutePath;
			Utilities.Path.OpenInNotepad( path );
		}
	}

	private void Screenshot()
	{
		if ( _asset is null )
			return;

		var path = Editor.FileSystem.Root.GetFullPath( $"/screenshots/shadergraphplus/{_asset.Name}.png" );
		System.IO.Directory.CreateDirectory( System.IO.Path.GetDirectoryName( path ) );

		_graphView.Capture( $"screenshots/shadergraphplus/{_asset.Name}.png" );

		EditorUtility.OpenFileFolder( path );
	}

	protected virtual void Compile()
	{
		_shaderCompileErrors.Clear();

		if ( string.IsNullOrWhiteSpace( _generatedCode ) )
		{
			RestoreShader();

			return;
		}

		if ( _isCompiling )
		{
			_isPendingCompile = true;

			return;
		}

		var assetPath = $"shadergraphplus/{_asset?.Name ?? "untitled"}_shadergraphplus.generated.shader";
		var resourcePath = System.IO.Path.Combine( ".source2/temp", assetPath );

		Editor.FileSystem.Root.CreateDirectory( ".source2/temp/shadergraphplus" );
		Editor.FileSystem.Root.WriteAllText( resourcePath, _generatedCode );

		_isCompiling = true;
		_preview3D.IsCompiling = _isCompiling;

		RestoreShader();

		_timeSinceCompile = 0;

		CompileAsync( resourcePath );
	}

	private async void CompileAsync( string path )
	{
		var options = new Sandbox.Engine.Shaders.ShaderCompileOptions
		{
			ConsoleOutput = true
		};

		var result = await EditorUtility.CompileShader( Editor.FileSystem.Root, path, options );

		if ( result.Success )
		{
			var asset = AssetSystem.RegisterFile( Editor.FileSystem.Root.GetFullPath( path ) );

			while ( !asset.IsCompiledAndUpToDate )
			{
				await Task.Yield();
			}
		}

		MainThread.Queue( () => OnCompileFinished( result.Success ? 0 : 1 ) );
	}

	private readonly List<string> _shaderCompileErrors = new();

	private struct StatusMessage
	{
		public string Status { get; set; }
		public string Message { get; set; }
	}

	internal void OnOutputData( object sender, DataReceivedEventArgs e )
	{
		var str = e.Data;
		if ( str == null )
			return;

		MainThread.Queue( () =>
		{
			var trimmed = str.Trim();
			if ( trimmed.StartsWith( '{' ) && trimmed.EndsWith( '}' ) )
			{
				var status = Json.Deserialize<StatusMessage>( trimmed );
				if ( status.Status == "Error" )
				{
					if ( !string.IsNullOrWhiteSpace( status.Message ) )
					{
						var lines = status.Message.Split( '\n' );
						foreach ( var line in lines.Where( x => !string.IsNullOrWhiteSpace( x ) ) )
						{
							_shaderCompileErrors.Add( line );
						}
					}
				}
			}
		} );
	}

	internal void OnErrorData( object sender, DataReceivedEventArgs e )
	{
		if ( e == null || e.Data == null )
			return;

		MainThread.Queue( () =>
		{
			var error = $"{e.Data}";
			if ( !string.IsNullOrWhiteSpace( error ) )
				_shaderCompileErrors.Add( error );
		} );
	}

	private void OnCompileFinished( int exitCode )
	{
		_isCompiling = false;

		if ( _isPendingCompile )
		{
			_isPendingCompile = false;

			Compile();

			return;
		}

		if ( exitCode == 0 && _shaderCompileErrors.Count == 0 )
		{
			SGPLog.Info( $"Compile finished in {_timeSinceCompile}" );

			var shaderPath = $"shadergraphplus/{_asset?.Name ?? "untitled"}_shadergraphplus.generated.shader";

			// Reload the shader otherwise it's gonna be the old wank
			// Alternatively Material.Create could be made to force reload the shader
			Editor.ConsoleSystem.Run( $"mat_reloadshaders {shaderPath}" );

			var material = Material.Create( $"{_asset?.Name ?? "untitled"}_shadergraphplus_generated", shaderPath );

			_preview3D.Material = material;
		}
		else
		{
			SGPLog.Error( $"Compile failed in {_timeSinceCompile}" );

			_output.GraphIssues = _shaderCompileErrors.Select( x => new GraphCompiler.GraphIssue { Message = x } ).ToList();

			DockManager.RaiseDock( "Output" );

			RestoreShader();
			ClearAttributes();
		}

		_preview3D.IsCompiling = _isCompiling;
		_preview3D.PostProcessing = _graph.Domain == ShaderDomain.PostProcess;

		_shaderCompileErrors.Clear();
	}

	private void OnAttribute( string name, object value )
	{
		if ( value == null )
			return;

		switch ( value )
		{
			case Color v:
				_float4Attributes.Add( name, v );
				_preview3D?.SetAttribute( name, v );
				break;
			case Vector4 v:
				_float4Attributes.Add( name, v );
				_preview3D?.SetAttribute( name, (Color)v );
				break;
			case Vector3 v:
				_float3Attributes.Add( name, v );
				_preview3D?.SetAttribute( name, v );
				break;
			case Vector2 v:
				_float2Attributes.Add( name, v );
				_preview3D?.SetAttribute( name, v );
				break;
			case float v:
				_floatAttributes.Add( name, v );
				_preview3D?.SetAttribute( name, v );
				break;
			case int v:
				_intAttributes.Add( name, v );
				_preview3D?.SetAttribute( name, v );
				break;
			case bool v:
				_boolAttributes.Add( name, v );
				_preview3D?.SetAttribute( name, v );
				break;
			case Texture v:
				_textureAttributes.Add( name, v );
				_preview3D?.SetAttribute( name, v );
				break;
			case Sampler v:
				_samplerStateAttributes.Add( name, (SamplerState)v );
				_preview3D?.SetAttribute( name, (SamplerState)v );
				break;
			case ShaderFeatureWrapper v:
				if ( _shaderFeatures.TryAdd( v.FeatureName, v.Value ) )
				{
					_preview3D?.SetFeature( v.FeatureName, v.Value );
				}
				break;
			case DynamicComboWrapper v:
				if ( _dynamicComboIntAttributes.TryAdd( v.ComboName, v.Value ) )
				{
					_preview3D?.SetDynamicCombo( v.ComboName, v.Value );
				}
				break;
			default:
				throw new InvalidOperationException( $"Unsupported attribute type: \"{value.GetType()}\"" );
		}
	}

	private void RegisterShaderFeatures( out List<GraphCompiler.GraphIssue> registrationIssues )
	{
		ShaderFeatures.Clear();
		registrationIssues = new();

		var features = _graph.Parameters.OfType<IShaderFeatureParameter>();

		foreach ( var feature in features )
		{
			ShaderFeatureBase shaderFeature = default;

			if ( feature is ShaderFeatureBooleanParameter boolFeatureParam )
			{
				shaderFeature = new ShaderFeatureBoolean()
				{
					Name = boolFeatureParam.Name,
					HeaderName = boolFeatureParam.HeaderName,
					Description = boolFeatureParam.Description,
				};
			}
			else if ( feature is ShaderFeatureEnumParameter enumFeatureParam )
			{
				shaderFeature = new ShaderFeatureEnum()
				{
					Name = enumFeatureParam.Name,
					HeaderName = enumFeatureParam.HeaderName,
					Description = enumFeatureParam.Description,
					Options = enumFeatureParam.Options,
				};
			}

			if ( shaderFeature.IsValid && !ShaderFeatures.ContainsKey( shaderFeature.Name ) )
			{
				ShaderFeatures.Add( shaderFeature.Name, shaderFeature );
			}
		}
	}

	private string GeneratePreviewCode()
	{
		if ( BlackboardIssues.Any() )
		{
			_output.GraphIssues = BlackboardIssues;
			DockManager.RaiseDock( "Output" );

			_generatedCode = null;
			_generatedCodeTextView.SetTextContents( "" );

			RestoreShader();
			return null;
		}

		if ( _autoCompile )
		{
			ClearAttributes();
		}

		// Go ahead preregister anything before iterating over all the nodes in the graph.
		RegisterShaderFeatures( out List<GraphCompiler.GraphIssue> registrationIssues );

		if ( registrationIssues.Any() )
		{
			_output.GraphIssues = registrationIssues;

			DockManager.RaiseDock( "Output" );

			_generatedCode = null;
			_generatedCodeTextView.SetTextContents( "" );

			RestoreShader();

			return null;
		}

		var resultNode = _graph.Nodes.OfType<BaseResult>().FirstOrDefault();
		var compiler = new GraphCompiler( _asset, _graph, ShaderFeatures, true );
		var iErroringNodeErrors = new List<GraphCompiler.GraphIssue>();
		var iWarningNodeWarnings = new List<GraphCompiler.GraphIssue>();

		if ( _autoCompile )
		{
			compiler.OnAttribute = OnAttribute;
		}

		foreach ( var node in _graph.Nodes.OfType<BaseNodePlus>() )
		{
			// Nodes used to fuck up attributes. Uncomment these two lines later
			// if its discoverd that they still do. Then fix the issue for good.
			/*
			// TEMP!!! 
			if ( node is BooleanFeatureSwitchNode || node is EnumFeatureSwitchNode  )
				continue;
			*/

			// Assign a PreviewID to any Previewable node.
			if ( node.CanPreview )
			{
				node.PreviewID = compiler.PreviewID++;
			}

			if ( node is IWarningNode warningNode )
			{
				var warnings = warningNode.GetWarnings();
				if ( warnings.Count > 0 )
				{
					foreach ( var warning in warnings )
					{
						iWarningNodeWarnings.Add( new() { Message = warning, Node = node, IsWarning = true } );
					}
				}
			}

			if ( node is IErroringNode erroringNode )
			{
				var errors = erroringNode.GetErrors();
				if ( errors.Count > 0 )
				{
					foreach ( var error in errors )
					{
						iErroringNodeErrors.Add( new() { Message = error, Node = node, IsWarning = false } );
					}
				}
			}

			var property = node.GetType().GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static )
				.FirstOrDefault( x => x.GetGetMethod() != null && x.PropertyType == typeof( NodeResult.Func ) );

			if ( property == null )
				continue;

			var output = property.GetCustomAttribute<BaseNodePlus.OutputAttribute>();
			if ( output == null )
				continue;

			var result = compiler.Result( new NodeInput { Identifier = node.Identifier, Output = property.Name } );
			if ( !result.IsValid() )
				continue;

			if ( node is SamplerNode samplerNode )
			{
				samplerNode.Processed = true;
			}

			var componentType = result.ComponentType;
			if ( componentType == null )
				continue;

			// While we're here, let's check the output plugs and update their handle configs to the result type
			var nodeUI = _graphView.FindNode( node );
			if ( !nodeUI.IsValid() )
				continue;

			var plugOut = nodeUI.Outputs.FirstOrDefault( x => ((BasePlug)x.Inner).Info.Property == property );
			if ( !plugOut.IsValid() )
				continue;

			plugOut.PropertyType = componentType;

			// We also have to update everything so they get repainted

			nodeUI.Update();

			foreach ( var input in nodeUI.Inputs )
			{
				if ( !input.IsValid() || !input.Connection.IsValid() )
					continue;

				input.Connection.Update();
			}
		}

		//foreach ( var node in _graph.Nodes.OfType<Texture2DSamplerBase>() )
		//{
		//	node.SyncNode();
		//}

		//_compiledNodes.Clear();
		//_compiledNodes.AddRange( compiler.Nodes );

		if ( _properties.IsValid() && _properties.Target is BaseNodePlus targetNode && targetNode.CanPreview )
		{
			_preview3D.SetStage( targetNode.PreviewID );
			//_preview.SetStage( _compiledNodes.IndexOf( targetNode ) + 1 );
		}
		else
		{
			_preview3D.SetStage( ShaderGraphPlusGlobals.GraphCompiler.NoNodePreviewID );
		}

		if ( resultNode != null && !IsSubgraph )
		{
			UpdateNodeUI( resultNode );
		}
		else if ( IsSubgraph )
		{
			foreach ( var subgraphOutput in _graph.Nodes.OfType<SubgraphOutput>() )
			{
				UpdateNodeUI( subgraphOutput );
			}
		}

		var code = compiler.Generate();

		#region Errors & Warnings

		iErroringNodeErrors.AddRange( compiler.Errors );
		iWarningNodeWarnings.AddRange( compiler.Warnings );

		if ( iWarningNodeWarnings.Any() ) //&& iErroringNodeErrors.Any() )
		{
			// Add any iErroringNodeErrors to the end of the iWarningNodeWarning list.
			if ( iErroringNodeErrors.Any() )
			{
				iWarningNodeWarnings.AddRange( iErroringNodeErrors );
				_output.GraphIssues = iWarningNodeWarnings;

				DockManager.RaiseDock( "Output" );

				_generatedCode = null;
				_generatedCodeTextView.SetTextContents( "" );

				RestoreShader();

				return null;
			}
			else // No Errors to add :) not great not terrible...
			{
				_output.GraphIssues = iWarningNodeWarnings;

				DockManager.RaiseDock( "Output" );
			}
		}
		else // No warnings? clear em.
		{
			_output.ClearWarnings();
		}

		if ( iErroringNodeErrors.Any() )
		{
			_output.GraphIssues = iErroringNodeErrors;

			DockManager.RaiseDock( "Output" );

			_generatedCode = null;
			//_generatedCodeTextView.SetTextContents( "" );

			RestoreShader();

			return null;
		}
		else // No errors :o? clear em :D.
		{
			_output.ClearErrors();
		}
		#endregion Errors & Warnings

		if ( _generatedCode != code )
		{
			_generatedCode = code;
			//_generatedCodeTextView.SetTextContents( code );

			if ( _autoCompile )
			{
				Compile();
			}
		}

		return code;
	}

	private void UpdateNodeUI( BaseNodePlus node )
	{
		var nodeUI = _graphView.FindNode( node );

		if ( nodeUI.IsValid() )
		{
			nodeUI.Update();

			foreach ( var input in nodeUI.Inputs )
			{
				if ( !input.IsValid() || !input.Connection.IsValid() )
					continue;

				input.Connection.Update();
			}
		}
	}

	private string GenerateShaderCode()
	{
		// Go ahead preregister anything before iterating over all the nodes in the graph.
		RegisterShaderFeatures( out _ );

		var compiler = new GraphCompiler( _asset, _graph, ShaderFeatures, false );
		return compiler.Generate();
	}

	public void OnUndoPushed()
	{
		_undoHistory.History = _undoStack.Names;
	}

	public void SetDirty( bool evaluate = true )
	{
		Update();

		_dirty = true;
		_graphCanvas.WindowTitle = $"{_asset?.Name ?? "untitled"}*";

		if ( evaluate )
			GeneratePreviewCode();
	}

	[EditorEvent.Frame]
	protected void Frame()
	{
		_undoOption.Enabled = _undoStack.CanUndo;
		_redoOption.Enabled = _undoStack.CanRedo;
		_undoMenuOption.Enabled = _undoStack.CanUndo;
		_redoMenuOption.Enabled = _undoStack.CanRedo;

		_undoOption.Text = _undoStack.UndoName;
		_redoOption.Text = _undoStack.RedoName;
		_undoMenuOption.Text = _undoStack.UndoName ?? "Undo";
		_redoMenuOption.Text = _undoStack.RedoName ?? "Redo";

		_undoOption.StatusTip = _undoStack.UndoName;
		_redoOption.StatusTip = _undoStack.RedoName;
		_undoMenuOption.StatusTip = _undoStack.UndoName;
		_redoMenuOption.StatusTip = _undoStack.RedoName;

		_undoHistory.UndoLevel = _undoStack.UndoLevel;

		CheckForChanges();
	}

	private void CheckForChanges()
	{
		bool wasDirty = false;
		foreach ( var node in _graph.Nodes )
		{
			if ( node is ShaderNodePlus shaderNode && shaderNode.IsDirty )
			{
				shaderNode.IsDirty = false;
				wasDirty = true;
			}
		}
		if ( wasDirty )
		{
			_graphView.ChildValuesChanged( null );
		}
	}

	[Shortcut( "editor.undo", "CTRL+Z" )]
	private void Undo()
	{
		if ( _undoStack.Undo() is UndoOp op )
		{
			Log.Info( $"Undo ({op.name})" );

			_redoOption.Enabled = _undoStack.CanUndo;

			_graph.ClearNodes();
			_graph.ClearParameters();

			_graph.DeserializeNodes( op.undoBuffer );
			_graph.DeserializeParameters( op.undoBuffer );

			_graphView.RebuildFromGraph();
			_blackboardView.RebuildBuildFromGraph();

			SetDirty();
		}
	}

	[Shortcut( "editor.redo", "CTRL+Y" )]
	private void Redo()
	{
		if ( _undoStack.Redo() is UndoOp op )
		{
			Log.Info( $"Redo ({op.name})" );

			_redoOption.Enabled = _undoStack.CanRedo;

			_graph.ClearNodes();
			_graph.ClearParameters();

			_graph.DeserializeNodes( op.redoBuffer );
			_graph.DeserializeParameters( op.redoBuffer );

			_graphView.RebuildFromGraph();
			_blackboardView.RebuildBuildFromGraph();

			SetDirty();
		}
	}

	private void SetUndoLevel( int level )
	{
		if ( _undoStack.SetUndoLevel( level ) is UndoOp op )
		{
			Log.Info( $"SetUndoLevel ({op.name})" );

			_graph.ClearNodes();
			_graph.ClearParameters();

			_graph.DeserializeNodes( op.redoBuffer );
			_graph.DeserializeParameters( op.redoBuffer );

			_graphView.RebuildFromGraph();
			_blackboardView.RebuildBuildFromGraph();

			SetDirty();
		}
	}

	[Shortcut( "editor.cut", "CTRL+X" )]
	private void CutSelection()
	{
		_graphView.CutSelection();
	}

	[Shortcut( "editor.copy", "CTRL+C" )]
	private void CopySelection()
	{
		_graphView.CopySelection();
	}

	[Shortcut( "editor.paste", "CTRL+V" )]
	private void PasteSelection()
	{
		_graphView.PasteSelection();
	}

	[Shortcut( "editor.duplicate", "CTRL+D", ShortcutType.Window )]
	private void DuplicateSelection()
	{
		_graphView.DuplicateSelection();
	}

	[Shortcut( "editor.select-all", "CTRL+A" )]
	private void SelectAll()
	{
		_graphView.SelectAll();
	}

	[Shortcut( "editor.clear-selection", "ESC", ShortcutType.Window )]
	private void ClearSelection()
	{
		_graphView.ClearSelection();
	}

	[Shortcut( "gameObject.frame", "F", ShortcutType.Window )]
	private void CenterOnSelection()
	{
		_graphView.CenterOnSelection();
	}

	private void CreateToolBar()
	{
		var toolBar = new ToolBar( this, "ShaderGraphPlusToolbar" );
		AddToolBar( toolBar, ToolbarPosition.Top );

		toolBar.AddOption( "New", "common/new.png", New ).StatusTip = "New Graph";
		toolBar.AddOption( "Open", "common/open.png", Open ).StatusTip = "Open Graph";
		toolBar.AddOption( "Save", "common/save.png", () => Save() ).StatusTip = "Save Graph";

		toolBar.AddSeparator();

		_undoOption = toolBar.AddOption( "Undo", "undo", Undo );
		_redoOption = toolBar.AddOption( "Redo", "redo", Redo );

		toolBar.AddSeparator();

		toolBar.AddOption( "Cut", "common/cut.png", CutSelection );
		toolBar.AddOption( "Copy", "common/copy.png", CopySelection );
		toolBar.AddOption( "Paste", "common/paste.png", PasteSelection );
		toolBar.AddOption( "Select All", "select_all", SelectAll );

		toolBar.AddSeparator();

		toolBar.AddOption( "Compile", "refresh", () => Compile() ).StatusTip = "Compile Graph";
		AutoCompileOption = toolBar.AddOption( "Toggle Auto Compile", "model_editor/auto_recompile.png", () =>
		{
			_autoCompile = !_autoCompile;

			if ( _autoCompile )
			{
				Compile();
				//SetDirty();
			}

			AutoCompileOption.Icon = $"{(_autoCompile ? "model_editor/auto_recompile.png" : "model_editor/supress_auto_recompile.png")}";
		} );
		AutoCompileOption.StatusTip = "Enable or Disable graph auto compile.";

		toolBar.AddOption( "Open Generated Shader", "common/edit.png", () => OpenGeneratedShader() ).StatusTip = "Open Generated Shader";
		toolBar.AddOption( "Take Screenshot", "photo_camera", Screenshot ).StatusTip = "Take Screenshot";

		_undoOption.Enabled = false;
		_redoOption.Enabled = false;
	}

	public void BuildMenuBar()
	{
		var file = MenuBar.AddMenu( "File" );
		file.AddOption( "New", "common/new.png", New, "editor.new" ).StatusTip = "New Graph";
		file.AddOption( "Open", "common/open.png", Open, "editor.open" ).StatusTip = "Open Graph";
		file.AddOption( "Save", "common/save.png", Save, "editor.save" ).StatusTip = "Save Graph";
		file.AddOption( "Save As...", "common/save.png", SaveAs, "editor.save-as" ).StatusTip = "Save Graph As...";

		file.AddSeparator();

		_recentFilesMenu = file.AddMenu( "Recent Files" );

		file.AddSeparator();

		file.AddOption( "Quit", null, Quit, "editor.quit" ).StatusTip = "Quit";

		var edit = MenuBar.AddMenu( "Edit" );
		_undoMenuOption = edit.AddOption( "Undo", "undo", Undo, "editor.undo" );
		_redoMenuOption = edit.AddOption( "Redo", "redo", Redo, "editor.redo" );
		_undoMenuOption.Enabled = _undoStack.CanUndo;
		_redoMenuOption.Enabled = _undoStack.CanRedo;

		edit.AddSeparator();
		edit.AddOption( "Cut", "common/cut.png", CutSelection, "editor.cut" );
		edit.AddOption( "Copy", "common/copy.png", CopySelection, "editor.copy" );
		edit.AddOption( "Paste", "common/paste.png", PasteSelection, "editor.paste" );
		edit.AddOption( "Select All", "select_all", SelectAll, "editor.select-all" );

		var debug = MenuBar.AddMenu( "Debug" );
		debug.AddSeparator();
		debug.AddOption( "Open Temp Shader", "common/edit.png", OpenTempGeneratedShader );
		debug.AddOption( "Open ShaderGraph Project in text editor", "common/edit.png", OpenShaderGraphProjectTxt );
		debug.AddSeparator();
		_nodeDebugInfoOption = debug.AddOption( "Toggle Node Debug Info", "common/setting.png", () =>
		{
			ConCommands.NodeDebugInfo = !ConCommands.NodeDebugInfo;

			if ( ConCommands.NodeDebugInfo )
			{
				_nodeDebugInfoOption.Icon = "common/widgetdebugger_focus.png";
			}
			else
			{
				_nodeDebugInfoOption.Icon = "common/widgetdebugger_none.png";
			}
		} );

		RefreshRecentFiles();

		var view = MenuBar.AddMenu( "View" );
		view.AboutToShow += () => OnViewMenu( view );
	}

	[Shortcut( "editor.quit", "CTRL+Q" )]
	void Quit()
	{
		Close();
	}
	void RefreshRecentFiles()
	{
		_recentFilesMenu.Enabled = _recentFiles.Count > 0;

		_recentFilesMenu.Clear();

		_recentFilesMenu.AddOption( "Clear recent files", null, ClearRecentFiles )
			.StatusTip = "Clear recent files";

		_recentFilesMenu.AddSeparator();

		const int maxFilesToDisplay = 10;
		int fileCount = 0;

		for ( int i = _recentFiles.Count - 1; i >= 0; i-- )
		{
			if ( fileCount >= maxFilesToDisplay )
				break;

			var filePath = _recentFiles[i];

			_recentFilesMenu.AddOption( $"{++fileCount} - {filePath}", null, () => PromptSave( () => Open( filePath ) ) )
				.StatusTip = $"Open {filePath}";
		}
	}

	private void OnViewMenu( Menu view )
	{
		view.Clear();
		view.AddOption( "Restore To Default", "settings_backup_restore", RestoreDefaultDockLayout );
		view.AddSeparator();

		foreach ( var dock in DockManager.DockTypes )
		{
			var o = view.AddOption( dock.Title, dock.Icon );
			o.Checkable = true;
			o.Checked = DockManager.IsDockOpen( dock.Title );
			o.Toggled += ( b ) => DockManager.SetDockState( dock.Title, b );
		}

		view.AddSeparator();

		var style = view.AddOption( "Grid-Aligned Wires", "turn_sharp_right" );
		style.Checkable = true;
		style.Checked = ShaderGraphPlusView.EnableGridAlignedWires;
		style.Toggled += b => ShaderGraphPlusView.EnableGridAlignedWires = b;

	}

	private void ClearRecentFiles()
	{
		if ( _recentFiles.Count == 0 )
			return;

		_recentFiles.Clear();

		RefreshRecentFiles();

		SaveRecentFiles();
	}

	private void AddToRecentFiles( string filePath )
	{
		filePath = filePath.ToLower();

		// If file is already recent, remove it so it'll become the most recent
		if ( _recentFiles.Contains( filePath ) )
		{
			_recentFiles.RemoveAll( x => x == filePath );
		}

		_recentFiles.Add( filePath );

		RefreshRecentFiles();
		SaveRecentFiles();
	}

	private void SaveRecentFiles()
	{
		Editor.FileSystem.Temporary.WriteJson( "shadergraphplus_recentfiles.json", _recentFiles );
	}

	private void PromptSave( Action action )
	{
		if ( !_dirty )
		{
			action?.Invoke();
			return;
		}

		var confirm = new PopupWindow(
			"Save Current Graph", "The open graph has unsaved changes. Would you like to save now?", "Cancel",
			new Dictionary<string, Action>()
			{
				{ "No", () => action?.Invoke() },
				{ "Yes", () => { if ( SaveInternal( false ) ) action?.Invoke(); } }
			}
		);

		confirm.Show();
	}

	[Shortcut( "editor.new", "CTRL+N" )]
	public void New()
	{
		PromptSave( CreateNew );
	}

	public void CreateNew()
	{
		_asset = null;
		_graph = new();
		_dirty = false;
		_graphView.Graph = _graph;
		_graphCanvas.WindowTitle = "untitled";
		_preview3D.Model = null;
		_preview3D.Tint = Color.White;
		_undoStack.Clear();
		_undoHistory.History = _undoStack.Names;
		_generatedCode = "";
		_generatedCodeTextView.SetTextContents( "" );
		_properties.Target = _graph;
		_blackboardView.Graph = _graph;

		_output.ClearErrors();
		_output.ClearWarnings();

		if ( !IsSubgraph )
		{
			var result = _graphView.CreateNewNode( _graphView.FindNodeType( typeof( Result ) ), 0 );
			_graphView.Scale = 1;
			_graphView.CenterOn( result.Size * 0.5f );
		}
		else
		{
			var result = _graphView.CreateNewNode( _graphView.FindNodeType( typeof( SubgraphOutput ) ), 0 );

			var subgraphOutput = result.Node as SubgraphOutput;
			subgraphOutput.OutputName = "Out0";
			subgraphOutput.OutputType = SubgraphPortType.Vector3;
			subgraphOutput.Preview = SubgraphOutputPreviewType.Albedo;

			_graphView.Scale = 1;
			_graphView.CenterOn( result.Size * 0.5f );
		}

		ClearAttributes();

		RestoreShader();
	}

	private void ClearAttributes()
	{
		_boolAttributes.Clear();
		_intAttributes.Clear();
		_floatAttributes.Clear();
		_float2Attributes.Clear();
		_float3Attributes.Clear();
		_float4Attributes.Clear();
		_textureAttributes.Clear();
		_samplerStateAttributes.Clear();
		_dynamicComboIntAttributes.Clear();
		_shaderFeatures.Clear();

		//_compiledNodes.Clear();

		_preview3D?.ClearAttributes();
	}

	public void Open()
	{
		var fd = new FileDialog( null )
		{
			Title = $"Open {FileType}",
			DefaultSuffix = $".{FileExtension}"
		};

		fd.SetNameFilter( $"{FileType} ( *.{FileExtension})" );

		if ( !fd.Execute() )
			return;

		PromptSave( () => Open( fd.SelectedFile ) );
	}

	private void OpenProject( string path )
	{
		Open( path );
	}

	public void Open( string path, bool addToPath = true )
	{
		var asset = AssetSystem.FindByPath( path );

		if ( asset == null )
			return;

		if ( asset == _asset )
		{
			Focus();
			return;
		}

		var graph = new ShaderGraphPlus();
		graph.Deserialize( System.IO.File.ReadAllText( path ), null, Path.GetFileName( path ) );
		graph.Path = asset.RelativePath;
		graph.IsSubgraph = IsSubgraph;

		_preview3D.Model = string.IsNullOrWhiteSpace( graph.Model ) ? null : Model.Load( graph.Model );
		_preview3D.LoadSettings( graph.PreviewSettings );

		_asset = asset;
		_graph = graph;
		_dirty = false;
		_graphView.Graph = _graph;
		_graphCanvas.WindowTitle = _asset.Name;
		_undoStack.Clear();
		_undoHistory.History = _undoStack.Names;
		_generatedCode = "";
		_generatedCodeTextView.SetTextContents( "" );
		_properties.Target = _graph;
		_blackboardView.Graph = _graph;

		if ( addToPath )
			AddFileHistory( path );

		_output.ClearErrors();
		_output.ClearWarnings();

		var center = Vector2.Zero;
		var resultNode = graph.Nodes.OfType<BaseResult>().FirstOrDefault();
		if ( resultNode != null )
		{
			var nodeUI = _graphView.FindNode( resultNode );
			if ( nodeUI.IsValid() )
			{
				center = nodeUI.Position + nodeUI.Size * 0.5f;
			}
		}

		_graphView.Scale = 1;
		_graphView.CenterOn( center );
		_graphView.RestoreViewFromCookie();

		ClearAttributes();

		AddToRecentFiles( path );

		foreach ( var parameter in graph.Parameters )
		{
			if ( parameter.CheckParameter( out var parameterIssues ) )
			{
				_graph.UpdateParameterNodes( parameter );
			}
			else
			{
				foreach ( var parameterIssue in parameterIssues )
				{
					AddBlackboardIssue( parameterIssue );
				}
			}
		}

		GeneratePreviewCode();
	}


	[Shortcut( "editor.save-as", "CTRL+SHIFT+S" )]
	public void SaveAs()
	{
		SaveInternal( true );
	}

	[Shortcut( "editor.save", "CTRL+S" )]
	public void Save()
	{
		SaveInternal( false );
	}

	private void AddFileHistory( string path )
	{
		var lastFileHistory = _fileHistory.LastOrDefault();
		if ( _fileHistoryPosition < _fileHistory.Count - 1 )
		{
			_fileHistory.RemoveRange( _fileHistoryPosition + 1, _fileHistory.Count - _fileHistoryPosition - 1 );
		}
		if ( path != lastFileHistory )
			_fileHistory.Add( path );
		_fileHistoryPosition = _fileHistory.Count - 1;

		UpdateFileHistoryButtons();
	}

	private void FileHistoryForward()
	{
		if ( _fileHistoryPosition < _fileHistory.Count - 1 )
		{
			_fileHistoryPosition++;
			PromptSave( () =>
			{
				Open( _fileHistory[_fileHistoryPosition], false );
				UpdateFileHistoryButtons();
			} );
		}
	}

	private void FileHistoryBack()
	{
		if ( _fileHistoryPosition > 0 )
		{
			_fileHistoryPosition--;
			PromptSave( () =>
			{
				Open( _fileHistory[_fileHistoryPosition], false );
				UpdateFileHistoryButtons();
			} );
		}
	}

	private void FileHistoryHome()
	{
		if ( _fileHistory.Count == 0 ) return;
		PromptSave( () =>
		{
			Open( _fileHistory.First() );
			UpdateFileHistoryButtons();
		} );
	}

	private void UpdateFileHistoryButtons()
	{
		_fileHistoryForward.Enabled = _fileHistoryPosition < _fileHistory.Count - 1;
		_fileHistoryBack.Enabled = _fileHistoryPosition > 0;
		_fileHistoryHome.Enabled = _asset.Path != _fileHistory.FirstOrDefault();
	}

	private bool SaveInternal( bool saveAs )
	{
		var savePath = _asset == null || saveAs ? GetSavePath() : _asset.AbsolutePath;
		if ( string.IsNullOrWhiteSpace( savePath ) )
			return false;

		_preview3D.SaveSettings( _graph.PreviewSettings );

		// Write serialized graph to asset file
		System.IO.File.WriteAllText( savePath, _graph.Serialize() );

		if ( saveAs )
		{
			// If we're saving as, we want to register the new asset
			_asset = null;
		}

		// Register asset if we haven't already
		_asset ??= AssetSystem.RegisterFile( savePath );

		if ( _asset == null )
		{
			SGPLog.Warning( $"Unable to register asset {savePath}" );

			return false;
		}

		MainAssetBrowser.Instance?.Local.UpdateAssetList();

		_dirty = false;
		_graphCanvas.WindowTitle = _asset.Name;

		if ( IsSubgraph )
		{
			Compile();

			_generatedCodeTextView.SetTextContents( _generatedCode );
		}
		else
		{
			var shaderPath = System.IO.Path.ChangeExtension( savePath, ".shader" );

			var code = GenerateShaderCode();

			if ( string.IsNullOrWhiteSpace( code ) )
				return false;

			_generatedCodeTextView.SetTextContents( code );

			// Write generated shader to file
			if ( System.IO.File.Exists( shaderPath ) )
			{
				FileInfo fileInfo = new FileInfo( shaderPath );

				if ( !fileInfo.IsReadOnly )
				{
					System.IO.File.WriteAllText( shaderPath, code );
				}
				else
				{
					SGPLog.Warning( $"Asset at path \"{_asset.Path}\" is read only!" );
				}
			}
			else
			{
				System.IO.File.WriteAllText( shaderPath, code );
			}

		}


		// Write generated post processing class to file within the current projects code folder.
		//if (_graph.MaterialDomain is MaterialDomain.PostProcess)
		//{
		//	// If the post processing class code is blank, dont bother generating the class.
		//	if ( !string.IsNullOrWhiteSpace( code.Item2 ) )
		//	{
		//        WritePostProcessingShaderClass( code.Item2 );
		//    }
		//	
		//}
		//

		AddToRecentFiles( savePath );

		EditorEvent.Run( "shadergraphplus.update.subgraph", _asset.RelativePath );

		return true;
	}

	private void WritePostProcessingShaderClass( string classCode )
	{
		var path = System.IO.Directory.CreateDirectory( $"{Utilities.Path.GetProjectCodePath()}/Components/PostProcessing" );

		File.WriteAllText( Path.Combine( path.FullName, $"{_asset.Name}_PostProcessing.cs" ), classCode );
	}

	private string GetSavePath()
	{
		var fd = new FileDialog( null )
		{
			Title = $"Save {FileType}",
			DefaultSuffix = $".{FileExtension}"
		};

		fd.SelectFile( $"untitled.{FileExtension}" );
		fd.SetFindFile();
		fd.SetModeSave();
		fd.SetNameFilter( $"{FileType} (*.{FileExtension})" );
		if ( !fd.Execute() )
			return null;

		return fd.SelectedFile;
	}

	public void CreateUI()
	{
		BuildMenuBar();

		DockManager.RegisterDockType( "Graph", "account_tree", null, false );
		DockManager.RegisterDockType( "Preview", "photo", null, false );
		DockManager.RegisterDockType( "Properties", "edit", null, false );
		DockManager.RegisterDockType( "Blackboard", "edit", null, false );
		DockManager.RegisterDockType( "Output", "notes", null, false );
		DockManager.RegisterDockType( "Console", "text_snippet", null, false );
		DockManager.RegisterDockType( "Undo History", "history", null, false );
		DockManager.RegisterDockType( "Palette", "palette", null, false );
		DockManager.RegisterDockType( "Generated Code", "", null, false );

		_graphCanvas = new Widget( this ) { WindowTitle = $"{(_asset != null ? _asset.Name : "untitled")}{(_dirty ? "*" : "")}" };
		_graphCanvas.Name = "Graph";
		_graphCanvas.SetWindowIcon( "account_tree" );
		_graphCanvas.Layout = Layout.Column();

		var graphToolBar = new ToolBar( _graphCanvas, "GraphToolBar" );
		graphToolBar.SetIconSize( 16 );
		_fileHistoryBack = graphToolBar.AddOption( null, "arrow_back" );
		_fileHistoryForward = graphToolBar.AddOption( null, "arrow_forward" );
		graphToolBar.AddSeparator();
		_fileHistoryHome = graphToolBar.AddOption( null, "common/home.png" );
		_fileHistoryBack.Triggered += FileHistoryBack;
		_fileHistoryForward.Triggered += FileHistoryForward;
		_fileHistoryHome.Triggered += FileHistoryHome;
		_fileHistoryBack.Enabled = false;
		_fileHistoryForward.Enabled = false;
		_fileHistoryHome.Enabled = false;

		var stretcher = new Widget( graphToolBar );
		stretcher.Layout = Layout.Row();
		stretcher.Layout.AddStretchCell( 1 );
		graphToolBar.AddWidget( stretcher );

		graphToolBar.AddWidget( new GamePerformanceBar( () => (1000.0f / PerformanceStats.LastSecond.FrameAvg).ToString( "n0" ) + "fps" ) { ToolTip = "Frames Per Second Average" } );
		graphToolBar.AddWidget( new GamePerformanceBar( () => PerformanceStats.LastSecond.FrameAvg.ToString( "0.00" ) + "ms" ) { ToolTip = "Frame Time Average (milliseconds)" } );
		graphToolBar.AddWidget( new GamePerformanceBar( () => PerformanceStats.ApproximateProcessMemoryUsage.FormatBytes() ) { ToolTip = "Approximate Memory Usage" } );

		_graphCanvas.Layout.Add( graphToolBar );

		_graphView = new ShaderGraphPlusView( _graphCanvas, this );
		_graphView.BilinearFiltering = false;

		var nodeTypes = EditorTypeLibrary.GetTypes<ShaderNodePlus>()
			.Where( x => !x.IsAbstract && !x.HasAttribute<HideAttribute>() ).OrderBy( x => x.Name );

		var blackboardParameterTypes = EditorTypeLibrary.GetTypes<BaseBlackboardParameter>()
			.Where( x => !x.IsAbstract ).OrderBy( x => x.Name );

		foreach ( var type in nodeTypes )
		{
			_graphView.AddNodeType( type );
		}

		foreach ( var type in blackboardParameterTypes )
		{
			_graphView.AddParameterType( type );
		}

		var subgraphs = AssetSystem.All.Where( x => x.Path.EndsWith( $".{ShaderGraphPlusGlobals.SubgraphAssetTypeExtension}", StringComparison.OrdinalIgnoreCase ) );
		foreach ( var subgraph in subgraphs )
		{
			// Skip any _c compiled subgraph files.
			if ( subgraph.CanRecompile )
			{
				_graphView.AddNodeType( subgraph.Path );
			}
		}

		_graphView.Graph = _graph;
		_graphView.OnChildValuesChanged += ( w ) => SetDirty();
		_graphView.OnNewParameterNodeCreated += () => OnNewParameterNodeCreated();
		_graphView.OnConstantNodeConvertedToParameter += () => OnConstantNodeConvertedToParamerter();
		_graphView.OnNodeRemoved += ( node ) => OnNodeRemoved( node );
		_graphCanvas.Layout.Add( _graphView, 1 );

		_output = new Output( this );
		_output.OnNodeSelected += ( node ) =>
		{
			var nodeUI = _graphView.SelectNode( node );

			_graphView.Scale = 1;
			_graphView.CenterOn( nodeUI.Center );
		};

		_preview3D = new Preview3DPanel( this, _graph.Model )
		{
			OnModelChanged = ( model ) => _graph.Model = model?.Name
		};

		foreach ( var value in _samplerStateAttributes )
		{
			_preview3D.SetAttribute( value.Key, value.Value );
		}

		foreach ( var value in _textureAttributes )
		{
			_preview3D.SetAttribute( value.Key, value.Value );
		}


		foreach ( var value in _float4Attributes )
		{
			_preview3D.SetAttribute( value.Key, value.Value );
		}

		foreach ( var value in _float3Attributes )
		{
			_preview3D.SetAttribute( value.Key, value.Value );
		}

		foreach ( var value in _float2Attributes )
		{
			_preview3D.SetAttribute( value.Key, value.Value );
		}

		foreach ( var value in _floatAttributes )
		{
			_preview3D.SetAttribute( value.Key, value.Value );
		}

		foreach ( var value in _boolAttributes )
		{
			_preview3D.SetAttribute( value.Key, value.Value );
		}

		foreach ( var value in _dynamicComboIntAttributes )
		{
			_preview3D.SetDynamicCombo( value.Key, value.Value );
		}

		foreach ( var value in _shaderFeatures )
		{
			_preview3D.SetFeature( value.Key, value.Value );
		}

		_properties = new Properties( this );
		_properties.Target = _graph;
		_properties.PropertyUpdated += OnPropertyUpdated;

		_blackboardCanvas = new Widget( this ) { WindowTitle = $"Blackboard" };
		_blackboardCanvas.Name = "Blackboard";
		_blackboardCanvas.SetWindowIcon( "list" );
		_blackboardCanvas.Layout = Layout.Row();
		_blackboardCanvas.Layout.Spacing = 8;
		_blackboardCanvas.Layout.Margin = 4;

		_blackboardView = new BlackboardView( _blackboardCanvas, this );

		foreach ( var type in blackboardParameterTypes )
		{
			_blackboardView.AddParameterType( type );
		}

		_blackboardView.Graph = _graph;
		_blackboardView.OnDirty += () => { SetDirty(); };
		_blackboardView.OnParameterSelected += ( p ) => { OnParameterSelected( p ); };
		//_blackboardView.OnParameterChanged += ( p ) => { OnParameterPropertyUpdated( p ); };
		_blackboardView.OnParameterCreated += ( p ) => { OnParameterPropertyCreated( p ); };
		_blackboardView.OnParameterDeleted += ( p ) => { OnParameterDeleted( p ); };
		_blackboardCanvas.Layout.Add( _blackboardView, 1 );

		_undoHistory = new UndoHistory( this, _undoStack );
		_undoHistory.OnUndo = Undo;
		_undoHistory.OnRedo = Redo;
		_undoHistory.OnHistorySelected = SetUndoLevel;

		_palette = new PaletteWidget( this, IsSubgraph );
		_generatedCodeTextView = new TextView( this, "Generated Code", "" );

		//DockManager.AddDock( null, _preview2D, DockArea.Left, DockManager.DockProperty.HideOnClose );
		DockManager.AddDock( null, _preview3D, DockArea.Inside, DockManager.DockProperty.HideOnClose );
		DockManager.AddDock( null, _graphCanvas, DockArea.Right, DockManager.DockProperty.HideCloseButton | DockManager.DockProperty.HideOnClose, 0.7f );
		DockManager.AddDock( _graphCanvas, _output, DockArea.Bottom, DockManager.DockProperty.HideOnClose, 0.25f );
		DockManager.AddDock( _preview3D, _properties, DockArea.Bottom, DockManager.DockProperty.HideOnClose, 0.5f );

		// Yuck, console is internal but i want it, what is the correct way?
		var console = EditorTypeLibrary.Create( "ConsoleWidget", typeof( Widget ), new[] { this } ) as Widget;
		DockManager.AddDock( _output, console, DockArea.Inside, DockManager.DockProperty.HideOnClose );
		DockManager.AddDock( _output, _blackboardCanvas, DockArea.Right, DockManager.DockProperty.HideOnClose, -0.5f );
		DockManager.AddDock( _output, _undoHistory, DockArea.Inside, DockManager.DockProperty.HideOnClose );
		DockManager.AddDock( _output, _palette, DockArea.Inside, DockManager.DockProperty.HideOnClose );
		DockManager.AddDock( _output, _generatedCodeTextView, DockArea.Inside, DockManager.DockProperty.HideOnClose );

		DockManager.RaiseDock( "Output" );
		DockManager.Update();

		_defaultDockState = DockManager.State;

		if ( StateCookie != "ShaderGraphPlus" )
		{
			StateCookie = "ShaderGraphPlus";
		}
		else
		{
			RestoreFromStateCookie();
		}
	}

	/// <summary>
	/// Called when a blackboard parameter is selected.
	/// </summary>
	private void OnParameterSelected( BaseBlackboardParameter parameter )
	{
		_graphView.ClearSelection();

		_properties.Target = parameter;
		_blackboardView.SetSelectedItem( parameter );
	}

	/// <summary>
	/// Called no items are known to be selected by GraphView
	/// and a blank area of the graph has been clicked on.
	/// </summary>
	public void OnGraphViewAreaClicked()
	{
		if ( _properties.Target is BaseBlackboardParameter )
		{
			OnNodeSelected( null );
			_blackboardView.ClearSeletedItem();
		}
	}

	private void OnNodeRemoved( BaseNodePlus node )
	{
		//if ( node is IBlackboardSyncable blackboardSyncable )
		//{
		//	_graph.RemoveParameter( blackboardSyncable.BlackboardParameterIdentifier );
		//	_blackboardView.RebuildBuildFromGraph( false );
		//}
	}

	private void OnParameterPropertyCreated( BaseBlackboardParameter blackboardParameter )
	{
		if ( _properties.Target != blackboardParameter )
		{
			_properties.Target = blackboardParameter;
		}
	}

	private void OnParameterDeleted( BaseBlackboardParameter blackboardParameter )
	{
		if ( _properties.Target == blackboardParameter )
		{
			_properties.Target = _graph;
		}

		_graph.RemoveParameter( blackboardParameter );

		var identifier = blackboardParameter.Identifier;

		foreach ( var node in _graph.Nodes )
		{
			if ( node is IBlackboardSyncableNode blackboardSyncable && blackboardSyncable.BlackboardParameterIdentifier == identifier && blackboardSyncable is BaseNodePlus baseNode )
			{
				_graph.RemoveNode( baseNode );

				if ( _properties.Target == baseNode )
					_properties.Target = _graph;

				_graphView.RebuildFromGraph();

				//break;
			}
		}

		SetDirty();

		_blackboardView.RebuildBuildFromGraph( false );
	}

	private void OnNewParameterNodeCreated()
	{
		_blackboardView.RebuildBuildFromGraph( true );
	}

	private void OnConstantNodeConvertedToParamerter()
	{
		_blackboardView.RebuildBuildFromGraph( true );
	}

	private void OnPropertyUpdated( SerializedProperty serializedProperty )
	{
		BlackboardIssues.Clear();
		_preview3D.PostProcessing = _graphView.Graph.Domain == ShaderDomain.PostProcess;

		if ( _properties.Target is BaseBlackboardParameter parameter )
		{
			// Dont update a node on the graph if we have any blackboard issues.
			if ( parameter.CheckParameter( out var parameterIssues ) )
			{
				_graph.UpdateParameterNodes( parameter );
			}
			else
			{
				foreach ( var parameterIssue in parameterIssues )
				{
					AddBlackboardIssue( parameterIssue );
				}
			}
		}

		if ( _properties.Target is BaseNodePlus node )
		{
			_graphView.UpdateNode( node );
		}

		var shouldEvaluate = _properties.Target is not CommentNode;
		SetDirty( shouldEvaluate );
	}

	protected override void RestoreDefaultDockLayout()
	{
		DockManager.State = _defaultDockState;

		SaveToStateCookie();
	}

	protected override bool OnClose()
	{
		if ( !_dirty )
		{
			return true;
		}

		var confirm = new PopupWindow(
			"Save Current Graph", "The open graph has unsaved changes. Would you like to save now?", "Cancel",
			new Dictionary<string, Action>()
			{
				{ "No", () => { _dirty = false; Close(); } },
				{ "Yes", () => { if ( SaveInternal( false ) ) Close(); } }
			}
		);

		confirm.Show();

		return false;
	}

	[EditorEvent.Hotload]
	public void OnHotload()
	{
		DockManager.Clear();
		MenuBar.Clear();

		CreateUI();
		Compile(); // Testing ONLY!

		_preview3D.LoadSettings( _graph.PreviewSettings );
	}

	[Event( "shadergraphplus.update.subgraph" )]
	public void OnSubgraphUpdate( string updatedPath )
	{
		foreach ( var node in _graph.Nodes )
		{
			if ( node is SubgraphNode subgraphNode )
			{
				subgraphNode.Subgraph = null;
				subgraphNode.OnNodeCreated();
			}
		}

		GeneratePreviewCode();
	}

	private void AddBlackboardIssue( string message )
	{
		BlackboardIssues.Add( new() { Node = null, Message = message } );
	}
}
