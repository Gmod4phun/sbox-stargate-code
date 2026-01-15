
namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Example node demonstrating how to setup a node that uses a void function.
/// </summary>
[Title( "Test Void Func" ), Category( "Dev" ), Icon( "straighten" )]
[InternalNode]
public sealed class TestVoidFuncNode : VoidFunctionBase
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide]
	public override bool CanPreview => false;

	[Title( "InA" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput InputA { get; set; }

	[JsonIgnore, Hide]
	public string OutA { get; set; } = "";

	[JsonIgnore, Hide]
	public string OutB { get; set; } = "";

	public float DefaultInputA { get; set; } = 0.0f;

	[Hide]
	public string VoidOscillator => @"
void VoidOscillator( float flTime, float flFrequency, float flPhase, float flStrength, out float amplitude )
{
	float period, currentPhase;
	amplitude = 0.0f;

	if( flFrequency > 0.0001f )
	{
		period = 1.0f / flFrequency;
		currentPhase = ( fmod( flTime, period ) * flFrequency ) + flPhase / 255.0f;
		amplitude = flStrength * sin( currentPhase * 3.1415926535897932f * 2.0f );
	}
	else
	{
		amplitude = flStrength;
	}
}
";

	public override void Register( GraphCompiler compiler )
	{
		compiler.RegisterInclude( $"TestFuncs.hlsl" );

		var func = compiler.RegisterHLSLFunction( VoidOscillator, "VoidOscillator" );
		var funcCall = compiler.ResultHLSLFunction( func, $"" );
	}

	public override void BuildFunctionCall( GraphCompiler compiler, ref List<VoidFunctionArgument> args, ref string functionName, ref string functionCall )
	{
		args.Add( new VoidFunctionArgument( nameof( InputA ), nameof( DefaultInputA ), "$in0", VoidFunctionArgumentType.Input, ResultType.Float ) );
		args.Add( new VoidFunctionArgument( nameof( OutA ), "$out0", VoidFunctionArgumentType.Output, ResultType.Vector2 ) );
		args.Add( new VoidFunctionArgument( nameof( OutB ), "$out1", VoidFunctionArgumentType.Output, ResultType.Color ) );

		functionName = $"TestFunc";
		functionCall = $"{functionName}( {args[0].VarName}, {args[1].VarName}, {args[2].VarName} )";
	}

	[Output( typeof( Vector2 ) )]
	[Title( "OutA" )]
	[Hide]
	public NodeResult.Func ResultA => ( GraphCompiler compiler ) => new NodeResult( ResultType.Vector2, OutA, constant: false );

	[Output( typeof( Color ) )]
	[Title( "OutB" )]
	[Hide]
	public NodeResult.Func ResultB => ( GraphCompiler compiler ) => new NodeResult( ResultType.Color, OutB, constant: false );

}
