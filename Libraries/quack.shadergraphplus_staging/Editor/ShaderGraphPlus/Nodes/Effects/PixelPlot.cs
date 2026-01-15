
namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Creates a 
/// </summary>
[Title( "Pixel Plot" ), Category( "Effects" )]
public sealed class PixelPlotNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.FunctionNode;

	[Hide]
	public string PixelPlot => @"	
float4 PixelPlot( in Texture2D vColor, in SamplerState sSampler, float2 vUv , float2 vGridSize , float flBoarderThickness)
{

	float2 vGridBlock = 1 / vGridSize;

	float2 vUvGrid = floor(vUv * vGridSize) / vGridSize; // Divide By Gridsize so that uvspace is clamped to  0 to 1.

	float2 vGridBoarder = step(0.5 - flBoarderThickness, frac(vUv / vGridBlock)) *
						 step(frac(vUv / vGridBlock), 0.5 + flBoarderThickness);

	float4 result = vColor.Sample(sSampler,vUvGrid) * (vGridBoarder.x * vGridBoarder.y);

	return result;
}
";

	/// <summary>
	/// Coordinates to sample this texture
	/// </summary>
	[Title( "Coordinates" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	/// <summary>
	/// Texture object to apply the effect to.
	/// </summary>
	[Title( "TexObject" )]
	[Input( typeof( Texture2DObject ) )]
	[Hide]
	public NodeInput TexObject { get; set; }

	/// <summary>
	/// How the effect is filtered and wrapped when sampled
	/// </summary>
	[Title( "Sampler" )]
	[Input( typeof( Sampler ) )]
	[Hide]
	public NodeInput Sampler { get; set; }

	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput GridSize { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput BoarderThickness { get; set; }

	[InlineEditor( Label = false ), Group( "Sampler" )]
	public Sampler SamplerState { get; set; } = new Sampler();
	public Vector2 DefaultGridSize { get; set; } = new Vector2( 24.0f, 24.0f );
	public float DefaultBoarderThickness { get; set; } = 0.420f;




	public PixelPlotNode()
	{
		//ExpandSize = new Vector2( 0f, 12f );
	}

	/// <summary>
	/// Pixel Plot effect result.
	/// </summary>
	[Hide]
	[Output( typeof( Color ) ), Title( "Result" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var textureobject = compiler.Result( TexObject );
		var coords = compiler.Result( Coords );
		var Grid = compiler.ResultOrDefault( GridSize, DefaultGridSize );
		var Boarder = compiler.ResultOrDefault( BoarderThickness, DefaultBoarderThickness );

		if ( !textureobject.IsValid )
		{
			return NodeResult.MissingInput( nameof( Texture2DObject ) );
		}
		else if ( textureobject.ResultType is not ResultType.Texture2DObject )
		{
			return NodeResult.Error( $"Input to TexObject is not a texture object!" );
		}

		string func = compiler.RegisterHLSLFunction( PixelPlot, "PixelPlot" );
		string funcCall = compiler.ResultHLSLFunction( func, $"{textureobject}, {compiler.ResultSamplerOrDefault( Sampler, SamplerState )}, {(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vTextureCoords.xy")}, {Grid}, {Boarder}" );

		return new NodeResult( ResultType.Color, funcCall );
	};
}
