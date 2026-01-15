using System.Text.Json.Nodes;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

internal static class ParameterNodeUpgraders
{
	private static void SetEnumTypeUpgrader_v2( JsonObject json )
	{
		var name = json["Name"].ToString();

		if ( !string.IsNullOrWhiteSpace( name ) )
		{

		}
		else
		{
		}

	}

	[SGPJsonUpgrader( typeof( BoolParameterNode ), 2 )]
	public static void BoolNodeUpgrader_v2( JsonObject json )
	{
		if ( !json.ContainsKey( "Name" ) )
		{
			return;
		}

		try
		{
			SetEnumTypeUpgrader_v2( json );
		}
		catch
		{
		}
	}

	[SGPJsonUpgrader( typeof( IntParameterNode ), 2 )]
	public static void IntNodeUpgrader_v2( JsonObject json )
	{
		if ( !json.ContainsKey( "Name" ) )
		{
			return;
		}

		try
		{
			SetEnumTypeUpgrader_v2( json );
		}
		catch
		{
		}
	}

	[SGPJsonUpgrader( typeof( FloatParameterNode ), 2 )]
	public static void FloatNodeUpgrader_v2( JsonObject json )
	{
		if ( !json.ContainsKey( "Name" ) )
		{
			return;
		}

		try
		{
			SetEnumTypeUpgrader_v2( json );
		}
		catch
		{
		}
	}

	[SGPJsonUpgrader( typeof( Float2ParameterNode ), 2 )]
	public static void Float2NodeUpgrader_v2( JsonObject json )
	{
		if ( !json.ContainsKey( "Name" ) )
		{
			return;
		}

		try
		{
			SetEnumTypeUpgrader_v2( json );
		}
		catch
		{
		}
	}

	[SGPJsonUpgrader( typeof( Float3ParameterNode ), 2 )]
	public static void Float3NodeUpgrader_v2( JsonObject json )
	{
		if ( !json.ContainsKey( "Name" ) )
		{
			return;
		}

		try
		{
			SetEnumTypeUpgrader_v2( json );
		}
		catch
		{
		}
	}

	[SGPJsonUpgrader( typeof( ColorParameterNode ), 2 )]
	public static void Float4NodeUpgrader_v2( JsonObject json )
	{
		if ( !json.ContainsKey( "Name" ) )
		{
			return;
		}

		try
		{
			SetEnumTypeUpgrader_v2( json );
		}
		catch
		{
		}
	}
}
