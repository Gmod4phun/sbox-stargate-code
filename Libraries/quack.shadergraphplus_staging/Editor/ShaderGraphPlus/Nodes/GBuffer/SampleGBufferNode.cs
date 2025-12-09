
namespace ShaderGraphPlus.Nodes;

public abstract class GBufferSampleNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.FunctionNode;

	[Input( typeof( Vector2 ) ), Title( "ScreenPos" ), Hide]
	public NodeInput ScreenPosition { get; set; }

	[Hide, JsonIgnore]
	public virtual string Buffer { get; }

	[Output, Title( "Result" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var screenPostion = compiler.Result( ScreenPosition );

		var result = "";
		if ( !screenPostion.IsValid )
		{
			result = $"{Buffer}::Sample( {(compiler.IsVs ? $"i.vPositionPs.xy" : $"i.vPositionSs.xy")} )";

		}
		else
		{
			result = $"{Buffer}::Sample( {screenPostion.Code} )";
		}

		return new NodeResult( ResultType.Vector3, result );
	};
}

/// <summary>
/// Sample the normal gbuffer.
/// </summary>
[Title( "Sample Normal GBuffer" ), Category( "GBuffer" ), Icon( "colorize" )]
public sealed class SampleNormalGBufferNode : GBufferSampleNode
{
	[Hide, JsonIgnore]
	public override string Buffer => "Normals";
}

/// <summary>
/// Sample the roughness gbuffer.
/// </summary>
[Title( "Sample Roughness GBuffer" ), Category( "GBuffer" ), Icon( "colorize" )]
public sealed class SampleRoughnessGBufferNode : GBufferSampleNode
{
	[Hide, JsonIgnore]
	public override string Buffer => "Roughness";
}
