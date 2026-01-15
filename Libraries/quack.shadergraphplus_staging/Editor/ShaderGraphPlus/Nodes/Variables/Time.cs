
namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Current time
/// </summary>
[Title( "Time" ), Category( "Variables" ), Icon( "timer" )]
public sealed class Time : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.GlobalVariableNode;

	[JsonIgnore]
	public float Value => RealTime.Now;

	[Output( typeof( float ) ), Title( "Time" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return new NodeResult( ResultType.Float, compiler.IsPreview ? "g_flPreviewTime" : "g_flTime", compiler.IsNotPreview );
	};
}
