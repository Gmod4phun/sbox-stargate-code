using Editor;
using ShaderGraphPlus.Utilities;

namespace ShaderGraphPlus.Diagnostics;

#nullable enable

public static class Assert
{
	internal static string m_AssertSound => "sounds/editor/kl_fiddlesticks.wav";

	/// <summary>
	/// Throws an exception when the given object is not null.
	/// </summary>
	public static void CheckNull<T>( T obj, string message = "" )
	{
		if ( obj != null )
		{
			EditorUtility.PlayRawSound( Editor.FileSystem.Content.GetFullPath( m_AssertSound ) );
		}

		Sandbox.Diagnostics.Assert.IsNull( obj, message );
	}

	/// <summary>
	/// Throws an exception when the given object is null.
	/// </summary>
	public static void CheckNotNull<T>( T obj, string message = "" )
	{
		if ( obj == null )
		{
			EditorUtility.PlayRawSound( Editor.FileSystem.Content.GetFullPath( m_AssertSound ) );
		}

		Sandbox.Diagnostics.Assert.NotNull( obj, message );
	}

	/// <summary>
	/// Throws an exception when the 2 given objects are not equal to each other.
	/// </summary>
	public static void CheckAreEqual<T>( T a, T b, string message = "" )
	{
		if ( !object.Equals( a, b ) )
		{
			EditorUtility.PlayRawSound( Editor.FileSystem.Content.GetFullPath( m_AssertSound ) );
		}

		Sandbox.Diagnostics.Assert.AreEqual( a, b, message );
	}

	/// <summary>
	/// Throws an exception when the 2 given objects are equal to each other.
	/// </summary>
	public static void CheckAreNotEqual<T>( T a, T b, string message = "" )
	{
		if ( object.Equals( a, b ) )
		{
			EditorUtility.PlayRawSound( Editor.FileSystem.Content.GetFullPath( m_AssertSound ) );
		}

		Sandbox.Diagnostics.Assert.AreNotEqual( a, b, message );
	}

	/// <summary>
	/// Returns true if input a equals input b. 
	/// </summary>
	public static bool Check<T>( T a, T b )
	{
		if ( object.Equals( a, b ) )
		{
			EditorUtility.PlayRawSound( Editor.FileSystem.Content.GetFullPath( m_AssertSound ) );
			return true;
		}
		else
		{
			return false;
		}
	}
}

public static class Exeptions
{
	public static void SGPExeption( string? message )
	{
		EdtiorSound.OhFiddleSticks();
		throw new Exception( message );
	}
}
