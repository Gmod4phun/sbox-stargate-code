
namespace ShaderGraphPlus.Nodes;

/// <summary>
/// 
/// </summary>
[Title( "Instance Id" ), Category( "Variables" ), Icon( "123" )]
public sealed class InstanceIdNode : ShaderNodePlus, IPreRegisterNodeData
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.StageInputNode;

	public void PreRegister( GraphCompiler compiler )
	{
		compiler.RegisterVertexInput( "uint", "vInstanceID", "SV_InstanceID" );
		compiler.RegisterPixelInput( "uint", "vInstanceID", "SV_InstanceID" );
	}

	[Output, Title( "Result" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return new NodeResult( ResultType.Float, "i.vInstanceID" );
	};
}
