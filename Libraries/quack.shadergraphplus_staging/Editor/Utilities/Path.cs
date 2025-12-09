using Editor;

namespace ShaderGraphPlus.Utilities;

public static class Path
{

	public static string ChooseExistingPath( string path1, string path2 )
	{
		if ( Directory.Exists( path1 ) )
		{
			return path1;
		}
		else if ( Directory.Exists( path2 ) )
		{
			return path2;
		}
		else if ( Directory.Exists( path1 ) || Directory.Exists( path2 ) )
		{
			Log.Error( $"Both path 1 & path 2 exist!" );
			return null;
		}
		else
		{
			return null; // Neither path exists
		}
	}

	// Opens a specified textfile in Notepad.
	public static void OpenInNotepad( string path )
	{
		Process p = new Process();
		ProcessStartInfo psi = new ProcessStartInfo( "Notepad.exe", path );
		p.StartInfo = psi;
		p.Start();
	}

	/// <summary>
	/// Returns the absolute path of the current project.
	/// </summary>
	public static string GetProjectRootPath()
	{
		return GetProjectAbsolutePath();
	}

	/// <summary>
	/// Absolute path to the location of the .sbproj file of the project.
	/// </summary>
	public static string GetProjectAbsolutePath()
	{
		return Sandbox.Project.Current.GetRootPath().Replace( '\\', '/' );
	}

	/// <summary>
	/// Absolute path to a file or directory thats within a mounted library project.
	/// </summary>
	public static string GetLibaryAbsolutePath( string path )
	{
		return Editor.FileSystem.Libraries.GetFullPath( path ).Replace( '\\', '/' );
	}

	public static string GetProjectCodePath()
	{
		return Sandbox.Project.Current.GetCodePath().Replace( '\\', '/' );
	}

	public static string GetShaderPath( Asset asset )
	{
		var shaderPath = string.Empty;

		var path = System.IO.Path.ChangeExtension( asset.AbsolutePath, ".shader" );
		var _asset = AssetSystem.FindByPath( path );

		Log.Info( $"Shader Path : {asset.Path}" );
		shaderPath = asset.Path;

		return shaderPath;
	}
}
