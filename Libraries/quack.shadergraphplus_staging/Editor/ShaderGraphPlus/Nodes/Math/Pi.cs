
namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Pi math constant with multiplier.
/// </summary>
[Title( "Pi" ), Category( "Constants" ), Order( 8 )]
public class PiNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.ConstantValueNode;

	[Title( "Multiplier" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Multiplier { get; set; }

	[Hide]
	public override string Title => DefaultMultiplier == 0 ? $"{DisplayInfo.For( this ).Name}" : $"{DefaultMultiplier} * {DisplayInfo.For( this ).Name}";

	public float DefaultMultiplier { get; set; } = 0.0f;

	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var multiplier = compiler.ResultOrDefault( Multiplier, DefaultMultiplier );

		return new NodeResult( ResultType.Float, $"{multiplier} * 3.14159265359" );
	};
}
