
namespace ShaderGraphPlus.Nodes;

/// <summary>
/// 
/// </summary>
[Title( "Depth Fade" ), Category( "Effects" ), Icon( "join_inner" )]
public sealed class DepthFadeNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.FunctionNode;

	[Hide]
	public static string DepthFade => @"
float DepthFade( float3 vWorldPos, float3 vCameraPositionWs, float3 vCameraDirWs, float2 vUv, float flDepthOffset, float flFalloff )
{
	float3 l_1 = vWorldPos - vCameraPositionWs;
	float l_2 = dot( l_1, normalize( vCameraDirWs ) );

	float depth = Depth::GetLinear( vUv );
	float l_3 = depth - l_2;
	
	return pow( saturate( l_3 / flDepthOffset ), flFalloff );
}
";

	[Title( "Depth Offset" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput DepthOffset { get; set; }

	[Title( "Falloff" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Falloff { get; set; }

	public float DefaultDepthOffset { get; set; } = 1.0f;
	public float DefaultFalloff { get; set; } = 0.5f;

	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var coords = "i.vPositionSs.xy";
		var worldPosition = $"i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz";
		var cameraPosition = "g_vCameraPositionWs";
		var cameraDirection = "g_vCameraDirWs";

		var depthoffset = compiler.ResultOrDefault( DepthOffset, DefaultDepthOffset );
		var falloff = compiler.ResultOrDefault( Falloff, DefaultFalloff );

		string func = compiler.RegisterHLSLFunction( DepthFade, "DepthFade" );
		string funcCall = compiler.ResultHLSLFunction( func, $"{worldPosition}, {cameraPosition}, {cameraDirection}, {coords}, {depthoffset}, {falloff}" );

		return new NodeResult( ResultType.Float, funcCall );
	};
}
