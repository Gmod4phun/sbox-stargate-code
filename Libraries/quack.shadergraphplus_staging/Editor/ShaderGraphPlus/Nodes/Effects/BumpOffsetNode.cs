
namespace ShaderGraphPlus.Nodes;

[Title( "Bump Offset" ), Category( "Effects" ), Icon( "water" )]
public sealed class BumpOffsetNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.FunctionNode;

	[JsonIgnore, Hide, Browsable( false )]
	private const float HeightScale = 0.1f;

	[Hide]
	public static string BumpOffset => $@"
float2 BumpOffset( float flHeightMap, float flDepthScale, float flReferencePlane, float2 vTextureCoords, float3 vTangentViewVector )
{{
	float flHeight = flReferencePlane - flHeightMap;

	float2 vUVOffset = vTangentViewVector.xy * float2( flHeight, flHeight ) * float2( flDepthScale, flDepthScale ) * float2( {HeightScale}f, {HeightScale}f );
	float2 vDistortedUV = vTextureCoords.xy + vUVOffset;

	return vDistortedUV;
}}
";

	[Input( typeof( float ) )]
	[Title( "Height" )]
	[Hide]
	public NodeInput InputHeight { get; set; }

	[Input( typeof( float ) )]
	[Title( "Depth Scale" )]
	[Hide, NodeValueEditor( nameof( DefaultDepthScale ) )]
	public NodeInput InputDepthScale { get; set; }

	[Input( typeof( float ) )]
	[Title( "Reference Plane" )]
	[Hide, NodeValueEditor( nameof( DefaultReferencePlane ) )]
	public NodeInput InputReferencePlane { get; set; }

	[Input( typeof( Vector2 ) )]
	[Title( "Coords" )]
	[Hide]
	public NodeInput InputCoords { get; set; }

	[Title( "Height" )]
	public float DefaultHeight { get; set; } = 0.0f;

	[Title( "Depth Scale" )]
	[Sandbox.Range( 0.0f, 1.0f )]
	public float DefaultDepthScale { get; set; } = 0.125f;

	[Title( "Reference Plane" )]
	[Sandbox.Range( 0.0f, 1.0f )]
	public float DefaultReferencePlane { get; set; } = 0.42f;

	[Output( typeof( Vector2 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var worldPosition = $"i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz";
		var tangentViewVector = compiler.ResultHLSLFunction( "GetTangentViewVector", $"{worldPosition}, i.vNormalWs, i.vTangentUWs, i.vTangentVWs" );

		var inputHeight = compiler.ResultOrDefault( InputHeight, DefaultHeight );
		var inputDepthScale = compiler.ResultOrDefault( InputDepthScale, DefaultDepthScale );
		var inputReferencePlane = compiler.ResultOrDefault( InputReferencePlane, DefaultReferencePlane );
		var inputCoords = compiler.Result( InputCoords );

		string func = compiler.RegisterHLSLFunction( BumpOffset, "BumpOffset" );
		string funcCall = compiler.ResultHLSLFunction( func,
			$"{inputHeight}, " +
			$"{inputDepthScale}, " +
			$"{inputReferencePlane}, " +
			$"{(inputCoords.IsValid ? $"{inputCoords.Cast( 2 )}" : "i.vTextureCoords.xy")}, " +
			$"{tangentViewVector}"
		);

		return new NodeResult( ResultType.Vector2, funcCall );
	};
}
