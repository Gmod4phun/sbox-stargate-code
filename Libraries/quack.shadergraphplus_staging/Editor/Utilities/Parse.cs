namespace ShaderGraphPlus.Utilities;

public static class Parse
{
	public static object ParseVector( string vectorString )
	{
		string[] components = vectorString.Split( ',' );
		switch ( components.Length )
		{
			case 2:
				return new Vector2( float.Parse( components[0] ), float.Parse( components[1] ) );
			case 3:
				return new Vector3( float.Parse( components[0] ), float.Parse( components[1] ), float.Parse( components[2] ) );
			case 4:
				return new Vector4( float.Parse( components[0] ), float.Parse( components[1] ), float.Parse( components[2] ), float.Parse( components[3] ) );
			default:
				throw new ArgumentException( "Invalid vector string format" );
		}
	}
}
