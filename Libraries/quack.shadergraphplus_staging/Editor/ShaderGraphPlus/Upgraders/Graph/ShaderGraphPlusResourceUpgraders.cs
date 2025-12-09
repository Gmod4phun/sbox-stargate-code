using System.Text.Json.Nodes;

namespace ShaderGraphPlus;

internal static class ShaderGraphPlusResourceUpgraders
{
	[SGPJsonUpgrader( typeof( ShaderGraphPlus ), 3 )]
	public static void ShaderGraphPlusUpgrader_v3( JsonObject json )
	{
		try
		{
			SGPLog.Info( "Running ShaderGraphPlus v3 JsonUpgrader" );

			if ( json.ContainsKey( "MaterialDomain" ) )
			{
				var currentDomain = json["MaterialDomain"].Deserialize<ShaderDomain>( ShaderGraphPlus.SerializerOptions() );
				json["Domain"] = JsonSerializer.SerializeToNode( currentDomain, ShaderGraphPlus.SerializerOptions() );

				json.Remove( "MaterialDomain" );
			}

		}
		catch
		{
		}
	}
}
