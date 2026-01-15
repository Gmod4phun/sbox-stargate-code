
namespace ShaderGraphPlus.Nodes;

[Title( "World To View" ), Category( "Variables/Matrix" ), Icon( "apps" )]
public sealed class WorldToViewNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.GlobalVariableNode;

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Output( typeof( Float4x4 ) ), Title( "Matrix" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return new NodeResult( ResultType.Float4x4, "g_matWorldToView", true );
	};
}
