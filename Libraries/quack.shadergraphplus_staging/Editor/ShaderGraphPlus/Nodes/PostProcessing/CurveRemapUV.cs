
namespace ShaderGraphPlus.Nodes;

[Title( "Curve Remap UV" ), Category( "PostProcessing/UV" )]
public class curveRemapUVNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Hide]
	public static string CurveRemapUV => @"
float2 curveRemapUV(float2 vScreenUV, float2 vCurvature)
{
	// as we near the edge of our screen apply greater distortion using a cubic function
	vScreenUV = vScreenUV * 2.0 - 1.0;
	float2 offset = abs(vScreenUV.yx) / float2(vCurvature.x, vCurvature.y);
	vScreenUV = vScreenUV + vScreenUV * offset * offset;
	vScreenUV = vScreenUV * 0.5 + 0.5;

	return vScreenUV;
}
";

	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput ScreenUVs { get; set; }

	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Curveature { get; set; }

	public Vector2 DefaultCurvature { get; set; } = new Vector2( 3.0f, 3.0f );

	[Output( typeof( Vector2 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var coords = compiler.Result( ScreenUVs );
		var curvature = compiler.ResultOrDefault( Curveature, DefaultCurvature );

		//return new NodeResult( ResultType.Vector2, compiler.ResultFunction( compiler.GetFunction( curveRemapUV ), $"{(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vPositionSs.xy / g_vRenderTargetSize")}", $"{curvature}" ) );
		return new NodeResult( ResultType.Vector2, $"curveRemapUV({(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vPositionSs.xy / g_vRenderTargetSize")}, {curvature})" );

	};
}
