
namespace ShaderGraphPlus.Nodes;

public enum MatrixNodeMode
{
	/// <summary>
	/// Input vectors specify matrix rows from Left to Right.
	/// </summary>
	[Title( "Row Major" )]
	RowMajor,
	/// <summary>
	/// Input vectors specify matrix columns from Top to Bottom.
	/// </summary>
	[Title( "Column Major" )]
	ColumnMajor
}

/// <summary>
/// Constructs square matrix values from the four input vectors.
/// </summary>
[Title( "Matrix Construction" ), Category( "Math/Matrix" ), Icon( "construction" )]
public sealed class MatrixConstructionNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.MatrixNode;

	[Hide]
	public override string Title => $"{DisplayInfo.For( this ).Name} ( {Mode} )";

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Title( "M0" )]
	[Input( typeof( Vector4 ) )]
	[Hide]
	public NodeInput InputVectorA { get; set; }

	[Title( "M1" )]
	[Input( typeof( Vector4 ) )]
	[Hide]
	public NodeInput InputVectorB { get; set; }

	[Title( "M2" )]
	[Input( typeof( Vector4 ) )]
	[Hide]
	public NodeInput InputVectorC { get; set; }

	[Title( "M3" )]
	[Input( typeof( Vector4 ) )]
	[Hide]
	public NodeInput InputVectorD { get; set; }

	public MatrixNodeMode Mode { get; set; } = MatrixNodeMode.RowMajor;

	public Vector4 DefaultVectorA { get; set; } = Vector4.Zero;
	public Vector4 DefaultVectorB { get; set; } = Vector4.Zero;
	public Vector4 DefaultVectorC { get; set; } = Vector4.Zero;
	public Vector4 DefaultVectorD { get; set; } = Vector4.Zero;


	public MatrixConstructionNode() : base()
	{
		ExpandSize = new Vector2( 32, 0 );
	}

	[Output( typeof( Float4x4 ) ), Title( "4x4" )]
	[Hide]
	public NodeResult.Func ResultA => ( GraphCompiler compiler ) =>
	{
		var resultVector0 = compiler.ResultOrDefault( InputVectorA, DefaultVectorA ).Cast( 4 );
		var resultVector1 = compiler.ResultOrDefault( InputVectorB, DefaultVectorB ).Cast( 4 );
		var resultVector2 = compiler.ResultOrDefault( InputVectorC, DefaultVectorC ).Cast( 4 );
		var resultVector3 = compiler.ResultOrDefault( InputVectorD, DefaultVectorD ).Cast( 4 );

		var result = $"";

		if ( Mode == MatrixNodeMode.RowMajor )
		{
			var row0 = $"{resultVector0}.x, {resultVector0}.y, {resultVector0}.z, {resultVector0}.w";
			var row1 = $"{resultVector1}.x, {resultVector1}.y, {resultVector1}.z, {resultVector1}.w";
			var row2 = $"{resultVector2}.x, {resultVector2}.y, {resultVector2}.z, {resultVector2}.w";
			var row3 = $"{resultVector3}.x, {resultVector3}.y, {resultVector3}.z, {resultVector3}.w";

			result = $"{row0}, {row1}, {row2}, {row3}";
		}
		else
		{
			var row0 = $"{resultVector0}.x, {resultVector1}.x, {resultVector2}.x, {resultVector3}.x";
			var row1 = $"{resultVector0}.y, {resultVector1}.y, {resultVector2}.y, {resultVector3}.y";
			var row2 = $"{resultVector0}.z, {resultVector1}.z, {resultVector2}.z, {resultVector3}.z";
			var row3 = $"{resultVector0}.w, {resultVector1}.w, {resultVector2}.w, {resultVector3}.w";

			result = $"{row0}, {row1}, {row2}, {row3}";
		}

		return new NodeResult( ResultType.Float4x4, result );
	};


	[Output( typeof( Float3x3 ) ), Title( "3x3" )]
	[Hide]
	public NodeResult.Func ResultB => ( GraphCompiler compiler ) =>
	{
		var resultVector0 = compiler.ResultOrDefault( InputVectorA, DefaultVectorA ).Cast( 3 );
		var resultVector1 = compiler.ResultOrDefault( InputVectorB, DefaultVectorB ).Cast( 3 );
		var resultVector2 = compiler.ResultOrDefault( InputVectorC, DefaultVectorC ).Cast( 3 );

		var result = $"";

		if ( Mode == MatrixNodeMode.RowMajor )
		{
			var row0 = $"{resultVector0}.x, {resultVector0}.y, {resultVector0}.z";
			var row1 = $"{resultVector1}.x, {resultVector1}.y, {resultVector1}.z";
			var row2 = $"{resultVector2}.x, {resultVector2}.y, {resultVector2}.z";

			result = $"{row0}, {row1}, {row2}";
		}
		else
		{
			var row0 = $"{resultVector0}.x, {resultVector1}.x, {resultVector2}.x";
			var row1 = $"{resultVector0}.y, {resultVector1}.y, {resultVector2}.y";
			var row2 = $"{resultVector0}.z, {resultVector1}.z, {resultVector2}.z";

			result = $"{row0}, {row1}, {row2}";
		}

		return new NodeResult( ResultType.Float3x3, result );
	};

	[Output( typeof( Float2x2 ) ), Title( "2x2" )]
	[Hide]
	public NodeResult.Func ResultC => ( GraphCompiler compiler ) =>
	{
		var resultVector0 = compiler.ResultOrDefault( InputVectorA, DefaultVectorA ).Cast( 2 );
		var resultVector1 = compiler.ResultOrDefault( InputVectorB, DefaultVectorB ).Cast( 2 );

		var result = $"";

		if ( Mode == MatrixNodeMode.RowMajor )
		{
			var row0 = $"{resultVector0}.x, {resultVector0}.y";
			var row1 = $"{resultVector1}.x, {resultVector1}.y";

			result = $"{row0}, {row1}";
		}
		else
		{
			var row0 = $"{resultVector0}.x, {resultVector1}.x";
			var row1 = $"{resultVector0}.y, {resultVector1}.y";

			result = $"{row0}, {row1}";
		}

		return new NodeResult( ResultType.Float2x2, result );
	};

}
