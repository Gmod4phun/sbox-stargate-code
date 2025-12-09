
namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Sample a provided gradient.
/// </summary>
[Title( "Sample Gradient" ), Category( "Gradient" ), Icon( "gradient" )]
public sealed class SampleGradientNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.FunctionNode;

	[Title( "Gradient" )]
	[Input( typeof( Gradient ) )]
	[Hide]
	public NodeInput Gradient { get; set; }

	/// <summary>
	/// Point in time to sample gradient.
	/// </summary>
	[Title( "Time" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Time { get; set; }

	private NodeResult Component( string component, GraphCompiler compiler )
	{
		var result = compiler.Result( new NodeInput { Identifier = Identifier, Output = nameof( Result ) } );
		return result.IsValid ? new( ResultType.Float, $"{result}.{component}", true ) : new( ResultType.Float, "0.0f", true );
	}

	[Hide]
	[Output( typeof( Color ) ), Title( "RGBA" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var gradient = compiler.Result( Gradient );

		if ( !gradient.IsValid() )
		{
			return NodeResult.MissingInput( nameof( Gradient ) );
		}

		if ( gradient.ResultType != ResultType.Gradient )
		{
			return NodeResult.Error( $"Gradient input is not a gradient!" );
		}

		var time = compiler.ResultOrDefault( Time, 0.0f );

		return new NodeResult( ResultType.Color, $"Gradient::SampleGradient( {gradient.Code}, {time.Code} )", constant: false );

	};

	/// <summary>
	/// Red component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "R" )]
	public NodeResult.Func R => ( GraphCompiler compiler ) => Component( "r", compiler );

	/// <summary>
	/// Green component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "G" )]
	public NodeResult.Func G => ( GraphCompiler compiler ) => Component( "g", compiler );

	/// <summary>
	/// Blue component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "B" )]
	public NodeResult.Func B => ( GraphCompiler compiler ) => Component( "b", compiler );

	/// <summary>
	/// Alpha (Opacity) component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "A" )]
	public NodeResult.Func A => ( GraphCompiler compiler ) => Component( "a", compiler );
}
