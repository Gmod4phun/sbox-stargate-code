using Editor;

namespace ShaderGraphPlus.Nodes;

[Title( "Matrix Transpose" ), Category( "Math/Matrix" ), Icon( "table_convert" )]
public sealed class MatrixTransposeNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.MatrixNode;

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Input, Title( "Matrix" )]
	[Hide]
	public NodeInput Input { get; set; }

	[Output, Title( "Matrix" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var inputResult = compiler.Result( Input );

		if ( !inputResult.IsValid )
		{
			return new NodeResult( ResultType.Float2x2, $"transpose( float2x2( 0, 0, 0, 0 ) )" );
		}

		if ( !inputResult.ResultType.IsMatrixType() )
		{
			return NodeResult.Error( $"Input must be a matrix type!" );
		}

		return new NodeResult( inputResult.ResultType, $"transpose( {inputResult.Code} )" );
	};
}
