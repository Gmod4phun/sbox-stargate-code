
namespace ShaderGraphPlus.Nodes;

[Title( "Oscillator" ), Category( "Utility" ), Icon( "waves" )]
public sealed class OscillatorNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.FunctionNode;

	[Hide]
	public string Oscillator => @"
float Oscillator( float flTime, float flFrequency, float flPhase, float flStrength )
{
	float period, amplitude, currentPhase;

	if(flFrequency > 0.0001f)
	{
		period = 1.0f/flFrequency;
		currentPhase = (fmod(flTime, period)*flFrequency) + flPhase/255.0f;
		amplitude = flStrength * sin(currentPhase * 3.1415926535897932f * 2.0f);
	}
	else
	{
		amplitude = flStrength;
	}

	return amplitude;
}
";

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Time { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Frequency { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Phase { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Strength { get; set; }

	public float DefaultFrequency { get; set; } = 1.0f;
	public float DefaultPhase { get; set; } = 0.0f;
	public float DefaultStrength { get; set; } = 10.0f;

	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var frequency = compiler.ResultOrDefault( Frequency, DefaultFrequency );
		var phase = compiler.ResultOrDefault( Phase, DefaultPhase );
		var strength = compiler.ResultOrDefault( Strength, DefaultStrength );
		var result_time = compiler.Result( Time );
		var time = "";

		if ( Time.IsValid() )
		{
			time = result_time.Code;
		}
		else
		{
			time = "g_flTime";
		}

		string func = compiler.RegisterHLSLFunction( Oscillator, "Oscillator" );
		string funcCall = compiler.ResultHLSLFunction( func, $"{time}, {frequency}, {phase}, {strength}" );

		return new NodeResult( ResultType.Float, funcCall );
	};
}
