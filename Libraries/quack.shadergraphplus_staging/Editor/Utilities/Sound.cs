using Editor;

namespace ShaderGraphPlus.Utilities;

public static class EdtiorSound
{
	public static void OhFiddleSticks()
	{
		EditorUtility.PlayRawSound( Editor.FileSystem.Content.GetFullPath( "sounds/editor/kl_fiddlesticks.wav" ) );
	}

	public static void Failure()
	{
		EditorUtility.PlayRawSound( Editor.FileSystem.Content.GetFullPath( "sounds/editor/kl_fiddlesticks.wav" ) );
	}

	public static void Success()
	{
		EditorUtility.PlayRawSound( Editor.FileSystem.Content.GetFullPath( "sounds/editor/success.wav" ) );
	}
}
