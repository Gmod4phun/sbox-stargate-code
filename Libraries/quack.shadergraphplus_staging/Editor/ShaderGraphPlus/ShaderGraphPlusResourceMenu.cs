using Editor;

namespace ShaderGraphPlus.AssetBrowser;

internal static class CreateShaderGraphPlusAsset
{
	internal static void AddShaderGraphPlusOption( Menu parent, DirectoryInfo folder )
	{
		parent.AddOption( $"New {ShaderGraphPlusGlobals.AssetTypeName}", "account_tree", () =>
		{
			var ProjectCreator = new ProjectCreator();
			ProjectCreator.DeleteOnClose = true;
			ProjectCreator.FolderEditPath = folder.FullName;
			ProjectCreator.Show();
		} );
	}

	[Event( "folder.contextmenu", Priority = 100 )]
	internal static void OnShaderGraphPlusAssetFolderContext( FolderContextMenu e )
	{
		// Remove broken option
		var otherMenu = e.Menu.FindOrCreateMenu( "New" ).FindOrCreateMenu( "Other" );
		otherMenu.RemoveOption( ShaderGraphPlusGlobals.AssetTypeName );

		if ( e.Target != null )
		{
			var menu = e.Menu.FindOrCreateMenu( "New" ).FindOrCreateMenu( "Shader" );
			AddShaderGraphPlusOption( menu, e.Target );
		}
	}
}

internal static class CreateShaderGraphPlusSubgraphAsset
{
	internal static void Create( string targetPath )
	{
		var template_path = ShaderGraphPlusFileSystem.Root.GetFullPath( "templates" );
		var sourceFile = $"{template_path}/$name.{ShaderGraphPlusGlobals.SubgraphAssetTypeExtension}";

		if ( !System.IO.File.Exists( sourceFile ) )
			return;

		// assure extension
		targetPath = System.IO.Path.ChangeExtension( targetPath, ShaderGraphPlusGlobals.SubgraphAssetTypeExtension );

		System.IO.File.Copy( sourceFile, targetPath );
		var asset = AssetSystem.RegisterFile( targetPath );

		MainAssetBrowser.Instance?.Local.UpdateAssetList();
	}

	internal static void AddShaderGraphPlusOption( Menu parent, DirectoryInfo folder )
	{
		parent.AddOption( $"New {ShaderGraphPlusGlobals.SubgraphAssetTypeName}", "account_tree", () =>
		{
			var fd = new FileDialog( null );
			fd.Title = $"Create {ShaderGraphPlusGlobals.SubgraphAssetTypeName}";
			fd.Directory = folder.FullName;
			fd.DefaultSuffix = $".{ShaderGraphPlusGlobals.SubgraphAssetTypeExtension}";
			fd.SelectFile( $"untitled.{ShaderGraphPlusGlobals.SubgraphAssetTypeExtension}" );
			fd.SetFindFile();
			fd.SetModeSave();
			fd.SetNameFilter( $"{ShaderGraphPlusGlobals.SubgraphAssetTypeName} (*.{ShaderGraphPlusGlobals.SubgraphAssetTypeExtension})" );

			if ( !fd.Execute() )
				return;

			Create( fd.SelectedFile );
		} );
	}

	[Event( "folder.contextmenu", Priority = 101 )]
	internal static void OnShaderGraphPlusAssetFolderContext( FolderContextMenu e )
	{
		// Remove broken option
		var otherMenu = e.Menu.FindOrCreateMenu( "New" ).FindOrCreateMenu( "Other" );
		otherMenu.RemoveOption( ShaderGraphPlusGlobals.SubgraphAssetTypeName );

		if ( e.Target != null )
		{
			var menu = e.Menu.FindOrCreateMenu( "New" ).FindOrCreateMenu( "Shader" );
			AddShaderGraphPlusOption( menu, e.Target );
		}
	}
}
