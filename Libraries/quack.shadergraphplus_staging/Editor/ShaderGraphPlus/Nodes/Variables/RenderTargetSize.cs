
namespace ShaderGraphPlus.Nodes;

/// <summary>
///
/// </summary>
[Title( "Render Target Size" ), Category( "Variables" ), Icon( "texture" )]
public sealed class RenderTargetSizeNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.GlobalVariableNode;

	[Output( typeof( Vector2 ) ), Title( "Render Target Size" )]
	[Hide]
	public static NodeResult.Func RenderTargetSize => ( GraphCompiler compiler ) => new( ResultType.Vector2, "g_vRenderTargetSize" );
}
