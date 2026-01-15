
namespace ShaderGraphPlus.Nodes;

// TODO : For later.
/*
[Title( "Sample ScreenSpace AO" ), Category( "" ), Icon( "colorize" )]
public sealed class SampleScreenSpaceAONode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Input( typeof( Vector3 ) ), Title( "ScreenPos" ), Hide]
	public NodeInput ScreenPosition { get; set; }

	[Output, Title( "Result" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var screenPostion = compiler.Result( ScreenPosition );

		var result = "";
		if ( !screenPostion.IsValid )
		{
			result = $"ScreenSpaceAmbientOcclusion::Sample( {(compiler.IsVs ? $"float4( i.vPositionPs.xyz, 1.0f )" : $"float4( i.vPositionSs.xyz, 1.0f )")} )";

		}
		else
		{
			result = $"ScreenSpaceAmbientOcclusion::Sample( {screenPostion.Code} )";
		}

		return new NodeResult( ResultType.Vector3, result );
	};
}
*/
