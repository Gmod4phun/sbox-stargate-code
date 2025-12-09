
namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Basic round gradient.
/// </summary>
[Title( "Round Gradient" ), Category( "Procedural/Gradients" ), Icon( "gradient" )]
public sealed class RoundGradientNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.FunctionNode;

	[Title( "UV" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	/// <summary>
	/// The center position of the round gradient.
	/// </summary>
	[Title( "Center" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput CenterPos { get; set; }

	/// <summary>
	/// The radius of the round gradient.
	/// </summary>
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Radius { get; set; }

	/// <summary>
	/// How dense you want the round gradient to be.
	/// </summary>
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Density { get; set; }

	[Input( typeof( bool ) )]
	[Hide]
	public NodeInput Invert { get; set; }

	public Vector2 DefaultCenterPos { get; set; } = new Vector2( 0.5f, 0.5f );
	public float DefaultRadius { get; set; } = 0.25f;
	public float DefaultDensity { get; set; } = 2.33f;
	public bool DefaultInvert { get; set; } = false;

	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var incoords = compiler.Result( Coords );
		var center = compiler.ResultOrDefault( CenterPos, DefaultCenterPos );
		var radius = compiler.ResultOrDefault( Radius, DefaultRadius );
		var density = compiler.ResultOrDefault( Density, DefaultDensity );
		var invert = compiler.ResultOrDefault( Invert, DefaultInvert );


		var coords = "";

		if ( compiler.Graph.Domain is ShaderDomain.PostProcess )
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "CalculateViewportUv( i.vPositionSs.xy )";
		}
		else
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "i.vTextureCoords.xy";
		}

		return new NodeResult( ResultType.Float, compiler.ResultHLSLFunction( "RoundGradient", $"{coords}",
			$"{center}", $"{radius}", $"{density}", $"{invert}"
		) );
	};

}
