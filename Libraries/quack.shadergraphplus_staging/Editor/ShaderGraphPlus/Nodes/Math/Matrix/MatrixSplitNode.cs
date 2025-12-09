
namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Split out each input matrix row or column into a vector.
/// </summary>
[Title( "Matrix Split" ), Category( "Math/Matrix" ), Icon( "alt_route" )]
public sealed class MatrixSplitNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.MatrixNode;

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Hide]
	public override string Title => $"{DisplayInfo.For( this ).Name} ( {Mode} )";

	[Input, Title( "Matrix" )]
	[Hide]
	public NodeInput Input { get; set; }

	public MatrixNodeMode Mode { get; set; } = MatrixNodeMode.RowMajor;

	private NodeResult GetResult( NodeResult inputResult, int rowIndex, int columnIndex )
	{
		var result = "";
		var resultType = ResultType.Invalid;

		if ( inputResult.ResultType == ResultType.Float2x2 )
		{
			resultType = ResultType.Vector2;

			if ( rowIndex > 1 && columnIndex > 1 )
			{
				result = $"float2( 0.0f, 0.0f )";
			}
			else
			{
				if ( Mode == MatrixNodeMode.RowMajor )
				{
					result = $"float2( {inputResult}[{rowIndex}].x, {inputResult}[{rowIndex}].y )";
				}
				else
				{
					result = $"float2( {inputResult}[0][{columnIndex}], {inputResult}[1][{columnIndex}] )";
				}
			}
		}
		else if ( inputResult.ResultType == ResultType.Float3x3 )
		{
			resultType = ResultType.Vector3;

			if ( rowIndex > 2 && columnIndex > 2 )
			{
				result = $"float3( 0.0f, 0.0f, 0.0f )";
			}
			else
			{
				if ( Mode == MatrixNodeMode.RowMajor )
				{
					result = $"float3( {inputResult}[{rowIndex}].x, {inputResult}[{rowIndex}].y, {inputResult}[{rowIndex}].z )";
				}
				else
				{
					result = $"float3( {inputResult}[0][{columnIndex}], {inputResult}[1][{columnIndex}], {inputResult}[2][{columnIndex}] )";
				}
			}

		}
		else if ( inputResult.ResultType == ResultType.Float4x4 )
		{
			resultType = ResultType.Vector4;

			if ( Mode == MatrixNodeMode.RowMajor )
			{
				result = $"float4( {inputResult}[{rowIndex}].x, {inputResult}[{rowIndex}].y, {inputResult}[{rowIndex}].z, {inputResult}[{rowIndex}].w )";
			}
			else
			{
				result = $"float4( {inputResult}[0][{columnIndex}], {inputResult}[1][{columnIndex}], {inputResult}[2][{columnIndex}], {inputResult}[3][{columnIndex}] )";
			}
		}

		return new NodeResult( resultType, result );
	}



	[Output, Title( "M0" )]
	[Hide]
	public NodeResult.Func ResultA => ( GraphCompiler compiler ) =>
	{
		var inputResult = compiler.Result( Input );

		if ( !inputResult.IsValid )
		{
			return new NodeResult( ResultType.Vector4, "float4( 0.0f, 0.0f, 0.0f, 0.0f )", constant: true );
		}

		if ( !inputResult.ResultType.IsMatrixType() )
		{
			return NodeResult.Error( $"Input must be a matrix type!" );
		}

		return GetResult( inputResult, 0, 0 );
	};

	[Output, Title( "M1" )]
	[Hide]
	public NodeResult.Func ResultB => ( GraphCompiler compiler ) =>
	{
		var inputResult = compiler.Result( Input );

		if ( !inputResult.IsValid )
		{
			return new NodeResult( ResultType.Vector4, "float4( 0.0f, 0.0f, 0.0f, 0.0f )", constant: true );
		}

		if ( !inputResult.ResultType.IsMatrixType() )
		{
			return NodeResult.Error( $"Input must be a matrix type!" );
		}

		return GetResult( inputResult, 1, 1 );
	};

	[Output, Title( "M2" )]
	[Hide]
	public NodeResult.Func ResultC => ( GraphCompiler compiler ) =>
	{
		var inputResult = compiler.Result( Input );

		if ( !inputResult.IsValid )
		{
			return new NodeResult( ResultType.Vector4, "float4( 0.0f, 0.0f, 0.0f, 0.0f )", constant: true );
		}

		if ( !inputResult.ResultType.IsMatrixType() )
		{
			return NodeResult.Error( $"Input must be a matrix type!" );
		}

		return GetResult( inputResult, 2, 2 );
	};

	[Output, Title( "M3" )]
	[Hide]
	public NodeResult.Func ResultD => ( GraphCompiler compiler ) =>
	{
		var inputResult = compiler.Result( Input );

		if ( !inputResult.IsValid )
		{
			return new NodeResult( ResultType.Vector4, "float4( 0.0f, 0.0f, 0.0f, 0.0f )", constant: true );
		}

		if ( !inputResult.ResultType.IsMatrixType() )
		{
			return NodeResult.Error( $"Input must be a matrix type!" );
		}

		return GetResult( inputResult, 3, 3 );
	};
}
