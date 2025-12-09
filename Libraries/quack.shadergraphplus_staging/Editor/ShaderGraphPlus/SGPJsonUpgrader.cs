using Sandbox.Internal;
using ShaderGraphPlus;
using System.Text.Json.Nodes;
using static ShaderGraphPlus.ShaderGraphPlus;

[AttributeUsage( AttributeTargets.Method )]
public class SGPJsonUpgraderAttribute : Attribute
{
	/// <summary>
	/// The version of this upgrade.
	/// </summary>
	public int Version { get; set; }

	/// <summary>
	/// The type we're targeting for this upgrade.
	/// </summary>
	public Type Type { get; set; }

	public SGPJsonUpgraderAttribute( Type type, int version )
	{
		Type = type;
		Version = version;
	}
}

internal static class SGPJsonUpgrader
{
	private static (MethodDescription Method, SGPJsonUpgraderAttribute Attribute)[] _methods;

	public static void UpdateUpgraders( TypeLibrary typeLibrary )
	{
		_methods = typeLibrary.GetMethodsWithAttribute<SGPJsonUpgraderAttribute>().ToArray();
	}

	public static void Upgrade( int version, JsonObject json, Type targetType )
	{
		if ( _methods == null )
		{
			return;
		}

		foreach ( var item2 in from x in _methods
							   where x.Attribute.Type == targetType
							   orderby x.Attribute.Version
							   where x.Attribute.Version > version
							   select x )
		{
			try
			{
				MethodDescription item = item2.Method;
				object[] parameters = new JsonObject[1] { json };
				item.Invoke( null, parameters );

				SGPLog.Info( $"Invoked json upgrader : {item.Name}", ConCommands.VerboseJsonUpgrader );
			}
			catch ( Exception exception )
			{
				Log.Warning( exception, $"A type version upgrader ( {item2.Attribute.Type}, version {item2.Attribute.Version}) threw an exception while trying to upgrade, so we halted the upgrade." );
				break;
			}
			finally
			{
				json[VersioningInfo.JsonPropertyName] = item2.Attribute.Version;
			}
		}

	}
}
