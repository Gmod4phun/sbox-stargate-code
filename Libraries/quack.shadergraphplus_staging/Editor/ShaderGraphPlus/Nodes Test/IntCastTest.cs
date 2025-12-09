
namespace ShaderGraphPlus.Nodes;

[Title( "Int Cast Test" ), Category( "Dev" ), Icon( "arrow" ), Hide]
[InternalNode]
public sealed class IntCastTest : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Input( typeof( int ) ), Hide]
	public NodeInput Input { get; set; }

	[Sandbox.Range( 1, 4 )]
	public int CastType { get; set; } = 1;

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = compiler.ResultOrDefault( Input, 1 );

		var castResult = input.Cast( CastType );

		SGPLog.Info( $"Casted int to \"{castResult}\"", compiler.IsPreview );

		var resultType = ResultType.Invalid;
		switch ( CastType )
		{
			case 1:
				resultType = ResultType.Float;
				break;
			case 2:
				resultType = ResultType.Vector2;
				break;
			case 3:
				resultType = ResultType.Vector3;
				break;
			case 4:
				resultType = ResultType.Color;
				break;
		}

		return new NodeResult( resultType, castResult );
	};

}
