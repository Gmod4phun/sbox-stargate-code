
namespace ShaderGraphPlus.Nodes;

[Title( "World Normals from Depth" ), Category( "Utility" )]
[InternalNode]
public sealed class WorldSpaceNormalFromDepth : ShaderNodePlus, IWarningNode
{
	[Hide]
	public override int Version => 1;

	[Title( "Screen Pos" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var incoords = compiler.Result( Coords );

		var coords = "";
		var defaultpos = $"{(compiler.IsVs ? $"i.vPositionPs.xy" : $"i.vPositionSs.xy")}";

		if ( compiler.Graph.Domain is ShaderDomain.PostProcess )
		{

			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : defaultpos;
		}
		else
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : defaultpos;
		}

		return new NodeResult( ResultType.Vector3, compiler.ResultHLSLFunction( "GetWorldSpaceNormal", $"{coords}" ) );
	};

	public List<string> GetWarnings()
	{
		var warnings = new List<string>();

		warnings.Add( $"\"World Normals from Depth\" node is depreciated and will be removed in a future update. Use \"Sample Normal GBuffer\" node instead " );

		return warnings;
	}
}
