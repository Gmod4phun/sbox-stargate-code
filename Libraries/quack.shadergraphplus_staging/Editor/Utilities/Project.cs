using Editor;

namespace ShaderGraphPlus.Utilities;

public static class Project
{
	/// <summary>
	/// Uses Regex pattern matching to fetch the Ident from the project's executing assembly name.
	/// </summary>
	public static string GetIdentFromExecutingAssemblyName()
	{
		string executingAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

		string pattern = @"^[^.]+\.[^.]+\.([^.]+)\.";

		Match match = Regex.Match( executingAssemblyName, pattern );

		var result = "";

		if ( match.Success )
		{
			result = match.Groups[1].Value;
		}

		return result;
	}


	/// <summary>
	/// Gets the org ident of the matching library package ident.
	/// </summary>
	public static string GetLibraryOrgIdent( string ident )
	{
		var libraryOrg = "";

		foreach ( var library in LibrarySystem.All )
		{
			if ( library.Project.Package.Ident == ident )
			{
				libraryOrg = library.Project.Package.Org.Ident;
			}
		}

		return libraryOrg;
	}

	/// <summary>
	/// Gets the matching library package ident.
	/// </summary>
	public static string GetLibraryPackageIdent( string ident )
	{
		var libraryIdent = "";

		foreach ( var library in LibrarySystem.All )
		{
			if ( library.Project.Package.Ident == ident )
			{
				libraryIdent = library.Project.Package.Ident;
			}
		}
		return libraryIdent;
	}
}
