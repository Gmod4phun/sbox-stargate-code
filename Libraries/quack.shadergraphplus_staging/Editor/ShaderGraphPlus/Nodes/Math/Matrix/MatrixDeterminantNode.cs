using Editor;

namespace ShaderGraphPlus.Nodes;

[Title( "Matrix Determinant" ), Category( "Math/Matrix" )]
public sealed class MatrixDeterminantNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.MatrixNode;

	[Input, Title( "Matrix" )]
	[Hide]
	public NodeInput Input { get; set; }

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Output, Title( "Matrix" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var inputResult = compiler.Result( Input );

		if ( !inputResult.IsValid )
		{
			return new NodeResult( ResultType.Float2x2, $"determinant( float4x4( 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f ) )" );
		}

		if ( !inputResult.ResultType.IsMatrixType() )
		{
			return NodeResult.Error( $"Input must be a matrix type!" );
		}

		return new NodeResult( inputResult.ResultType, $"determinant( {inputResult.Code} )" );
	};
}
