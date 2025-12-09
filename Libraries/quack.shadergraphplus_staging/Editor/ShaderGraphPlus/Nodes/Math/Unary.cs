using Editor;

namespace ShaderGraphPlus.Nodes;

public enum ExponentBase
{
	BaseE,
	Base2,
}

public enum LogBase
{
	BaseE,
	Base2,
	Base10,
}

public enum DerivativePrecision
{
	Standard,
	Course,
	Fine
}

public abstract class Unary : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.UnaryNode;

	[Input]
	[Hide]
	public virtual NodeInput Input { get; set; }

	protected virtual string Op { get; }

	[Hide]
	protected virtual int? Components { get; set; } = null;

	public Unary() : base()
	{
		ExpandSize = new Vector3( -50, 0 );
	}

	[Output]
	[Hide]
	public virtual NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var result = compiler.ResultOrDefault( Input, 0.0f );

		ResultType resulttype = result.ResultType;

		if ( Components is not null )
		{
			switch ( Components )
			{
				case 1: resulttype = ResultType.Float; break;
				case 2: resulttype = ResultType.Vector2; break;
				case 3: resulttype = ResultType.Vector3; break;
				case 4: resulttype = ResultType.Color; break;
			}
		}


		return result.IsValid ? new NodeResult( resulttype, $"{Op}( {result} )" ) : default;
	};
}

public abstract class UnaryCurve : Unary
{
	[Input, Title( "" ), Hide]
	public override NodeInput Input { get => base.Input; set => base.Input = value; }

	[Output, Title( "" ), Hide]
	public override NodeResult.Func Result => base.Result;

	protected virtual float Evaluate( float x ) => 0.0f;

	public UnaryCurve() : base()
	{
		ExpandSize = new Vector2( -12 * 6, 12 );
	}

	public override void OnPaint( Rect rect )
	{
		rect = rect.Shrink( 20, 28, 20, 6 );
		Paint.SetBrush( Theme.TextControl );
		Paint.SetPen( Theme.TextControl, 2 );
		var inc = 1f / 16f;
		List<Vector2> points = new List<Vector2>();
		for ( var i = 0f; i < 1f; i += inc )
		{
			var x = rect.BottomLeft.x + rect.Width * i;
			var y = rect.BottomLeft.y - rect.Height * Evaluate( i );
			points.Add( new Vector2( x, y ) );
		}
		for ( int i = points.Count - 1; i >= 0; i-- )
		{
			points.Add( points[i] );
		}
		Paint.DrawPolygon( points.ToArray() );
	}
}

/// <summary>
/// Clamps the specified input value to the pecified minimum and maximum.
/// </summary>
[Title( "Clamp" ), Category( "Math/Unary" )]
public sealed class Clamp : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Hide, JsonIgnore]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.UnaryNode;

	[Input]
	[Hide]
	[Title( "Value" )]
	public NodeInput InputA { get; set; }

	[Input]
	[Hide]
	[Title( "Min" )]
	public NodeInput InputB { get; set; }

	[Input]
	[Hide]
	[Title( "Max" )]
	public NodeInput InputC { get; set; }

	public float DefaultMin { get; set; } = 0.0f;
	public float DefaultMax { get; set; } = 1.0f;

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var resultA = compiler.ResultOrDefault( InputA, 0.0f );
		var resultB = compiler.ResultOrDefault( InputB, DefaultMin );
		var resultC = compiler.ResultOrDefault( InputC, DefaultMax ).Cast( resultB.Components );

		return new NodeResult( ResultType.Float, $"clamp( {resultA}, {resultB}, {resultC} )" );
	};
}

/// <summary>
/// Returns the cosine of the input value.
/// </summary>
[Title( "Cosine" ), Category( "Math/Unary" )]
public sealed class Cosine : UnaryCurve
{
	protected override float Evaluate( float x )
	{
		return MathF.Cos( x * MathF.PI * 2 ) / 2 + 0.5f;
	}

	[Hide]
	protected override string Op => "cos";
}

/// <summary>
/// Returns the absolute value of the input value.
/// </summary>
[Title( "Abs" ), Category( "Math/Unary" )]
public sealed class Abs : Unary
{
	[Hide]
	protected override string Op => "abs";
}

/// <summary>
/// Returns the reciprocal of the square root of the input value.
/// </summary>
[Title( "Rsqrt" ), Category( "Math/Unary" )]
public sealed class Rsqrt : Unary
{
	[Input, Title( "" ), Hide]
	public override NodeInput Input { get => base.Input; set => base.Input = value; }

	[Output, Title( "" ), Hide]
	public override NodeResult.Func Result => base.Result;

	public Rsqrt() : base()
	{
		ExpandSize = new Vector2( -12 * 8, 12 );
	}

	public override void OnPaint( Rect rect )
	{
		rect = rect.Shrink( 2, 20, 0, 0 );
		Paint.SetPen( Theme.TextControl );
		Paint.SetFont( "Poppins Bold", 12 );
		Paint.DrawText( rect, " x" );
		List<Vector2> points = new()
		{
			rect.TopLeft + new Vector2(10, 20),
			rect.TopLeft + new Vector2(14, 20),
			rect.TopLeft + new Vector2(18, 30),
			rect.TopLeft + new Vector2(22, 12),
			rect.TopLeft + new Vector2(40, 12)
		};
		for ( int i = points.Count - 1; i >= 0; i-- )
		{
			points.Add( points[i] );
		}
		Paint.DrawPolygon( points.ToArray() );
	}

	[Hide]
	protected override string Op => "rsqrt";
}

/// <summary>
/// Returns the square root of the input value.
/// </summary>
[Title( "Sqrt" ), Category( "Math/Unary" )]
public sealed class Sqrt : Unary
{
	[Input, Title( "" ), Hide]
	public override NodeInput Input { get => base.Input; set => base.Input = value; }

	[Output, Title( "" ), Hide]
	public override NodeResult.Func Result => base.Result;

	public Sqrt() : base()
	{
		ExpandSize = new Vector2( -12 * 8, 12 );
	}

	public override void OnPaint( Rect rect )
	{
		rect = rect.Shrink( 2, 20, 0, 0 );
		Paint.SetPen( Theme.TextControl );
		Paint.SetFont( "Poppins Bold", 12 );
		Paint.DrawText( rect, " x" );
		List<Vector2> points = new()
		{
			rect.TopLeft + new Vector2(10, 20),
			rect.TopLeft + new Vector2(14, 20),
			rect.TopLeft + new Vector2(18, 30),
			rect.TopLeft + new Vector2(22, 12),
			rect.TopLeft + new Vector2(40, 12)
		};
		for ( int i = points.Count - 1; i >= 0; i-- )
		{
			points.Add( points[i] );
		}
		Paint.DrawPolygon( points.ToArray() );
	}

	[Hide]
	protected override string Op => "sqrt";
}

/// <summary>
/// Returns the doc product which is a value equal to the magnitudes of the two input values multiplied together and then multiplied by the cosine of the angle between them.
/// </summary>
[Title( "Dot Product" ), Category( "Math/Unary" )]
public sealed class DotProduct : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Hide, JsonIgnore]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.UnaryNode;

	[Input]
	[Hide]
	public NodeInput InputA { get; set; }

	[Input]
	[Hide]
	public NodeInput InputB { get; set; }

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var resultA = compiler.ResultOrDefault( InputA, 0.0f );
		var resultB = compiler.ResultOrDefault( InputB, 0.0f ).Cast( resultA.Components );

		return new NodeResult( ResultType.Float, $"dot( {resultA}, {resultB} )" );
	};
}

[Title( "DDX" ), Category( "Math/Unary" )]
public sealed class DDX : Unary
{
	public DerivativePrecision Precision { get; set; }

	[Hide]
	protected override string Op
	{
		get
		{
			return Precision switch
			{
				DerivativePrecision.Course => "ddx_course",
				DerivativePrecision.Fine => "ddx_fine",
				_ => "ddx",
			};
		}
	}
}

[Title( "DDY" ), Category( "Math/Unary" )]
public sealed class DDY : Unary
{
	public DerivativePrecision Precision { get; set; }

	[Hide]
	protected override string Op
	{
		get
		{
			return Precision switch
			{
				DerivativePrecision.Course => "ddy_course",
				DerivativePrecision.Fine => "ddy_fine",
				_ => "ddy",
			};
		}
	}
}

[Title( "DDXY" ), Category( "Math/Unary" )]
public sealed class DDXY : Unary
{
	[Hide]
	protected override string Op => "fwidth";
}

[Title( "Exponential" ), Category( "Math/Unary" )]
public sealed class Exponential : Unary
{
	public ExponentBase Base { get; set; }

	[Input, Title( "" ), Hide]
	public override NodeInput Input { get => base.Input; set => base.Input = value; }

	[Output, Title( "" ), Hide]
	public override NodeResult.Func Result => base.Result;

	public Exponential() : base()
	{
		ExpandSize = new Vector2( -12 * 6, 12 );
	}

	public override void OnPaint( Rect rect )
	{
		rect = rect.Shrink( 0, 20, 0, 0 );
		Paint.SetPen( Theme.TextControl );
		Paint.SetFont( "Poppins Bold", 16 );
		Paint.DrawText( rect, (Base == ExponentBase.BaseE) ? "e" : "2" );
		Paint.SetFont( "Poppins Bold", 8 );
		Paint.DrawText( rect.Shrink( 20, 0, 0, 16 ), "x" );
	}

	[Hide]
	protected override string Op => Base == ExponentBase.BaseE ? "exp" : "exp2";
}

/// <summary>
/// Returns the fractional (or decimal) part of the input value.
/// </summary>
[Title( "Frac" ), Category( "Math/Unary" )]
public sealed class Frac : Unary
{
	[Hide]
	protected override string Op => "frac";
}

/// <summary>
/// The largest integer value (or whole number) that is less than or equal to the input value.
/// </summary>
[Title( "Floor" ), Category( "Math/Unary" )]
public sealed class Floor : Unary
{
	[Hide]
	protected override string Op => "floor";
}

/// <summary>
/// Return the length (or magnitude) of the input value.
/// </summary>
[Title( "Length" ), Category( "Math/Unary" )]
public sealed class Length : Unary
{
	[Hide]
	protected override int? Components => 1;

	[Hide]
	protected override string Op => "length";
}


[Title( "Log" ), Category( "Math/Unary" )]
public sealed class BaseLog : Unary
{
	public LogBase Base { get; set; }

	protected override string Op
	{
		get
		{
			return Base switch
			{
				LogBase.Base2 => "log2",
				LogBase.Base10 => "log10",
				_ => "log",
			};
		}
	}
}

[Title( "Min" ), Category( "Math/Unary" )]
public sealed class Min : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Hide, JsonIgnore]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.UnaryNode;

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput InputA { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput InputB { get; set; }

	public float DefaultA { get; set; } = 0.0f;
	public float DefaultB { get; set; } = 0.0f;

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var a = compiler.ResultOrDefault( InputA, DefaultA );
		var b = compiler.ResultOrDefault( InputB, DefaultB );

		int maxComponents = Math.Max( a.IsValid ? a.Components : 1, b.IsValid ? b.Components : 1 );

		ResultType resultType = maxComponents switch
		{
			1 => ResultType.Float,
			2 => ResultType.Vector2,
			3 => ResultType.Vector3,
			4 => ResultType.Color,
			_ => throw new NotImplementedException(),
		};

		return new NodeResult( resultType, $"min( {(a.IsValid ? a : "0.0f")}, {(b.IsValid ? b : "0.0f")} )" );
	};
}

[Title( "Max" ), Category( "Math/Unary" )]
public sealed class Max : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.UnaryNode;

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput InputA { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput InputB { get; set; }

	public float DefaultA { get; set; } = 0.0f;
	public float DefaultB { get; set; } = 0.0f;

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var a = compiler.ResultOrDefault( InputA, DefaultA );
		var b = compiler.ResultOrDefault( InputB, DefaultB );

		int maxComponents = Math.Max( a.IsValid ? a.Components : 1, b.IsValid ? b.Components : 1 );

		ResultType resultType = maxComponents switch
		{
			1 => ResultType.Float,
			2 => ResultType.Vector2,
			3 => ResultType.Vector3,
			4 => ResultType.Color,
			_ => throw new NotImplementedException(),
		};

		return new NodeResult( resultType, $"max( {(a.IsValid ? a : "0.0f")}, {(b.IsValid ? b : "0.0f")} )" );
	};
}

/// <summary>
/// Clamps the specified value within the range of 0 to 1
/// </summary>
[Title( "Saturate" ), Category( "Transform" )]
public sealed class Saturate : Unary
{
	[Hide]
	protected override string Op => "saturate";
}

/// <summary>
/// Returns the sine of the input value
/// </summary>
[Title( "Sine" ), Category( "Math/Unary" )]
public sealed class Sine : UnaryCurve
{
	protected override float Evaluate( float x )
	{
		return MathF.Sin( x * MathF.PI * 2 ) / 2 + 0.5f;
	}

	[Hide]
	protected override string Op => "sin";
}

/// <summary>
/// Computes a smooth interpolation between 0 and 1. When the the input value of Input is greater than or equal to the input value of Edge.
/// </summary>
[Title( "Step" ), Category( "Math/Unary" )]
public sealed class Step : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Hide, JsonIgnore]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.UnaryNode;

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Input { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Edge { get; set; }

	public float DefaultInput { get; set; } = 0.0f;
	public float DefaultEdge { get; set; } = 0.0f;

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var edge = compiler.ResultOrDefault( Edge, DefaultEdge );
		var input = compiler.ResultOrDefault( Input, DefaultInput );

		int maxComponents = Math.Max( edge.IsValid ? edge.Components : 1, input.IsValid ? input.Components : 1 );

		ResultType resultType = maxComponents switch
		{
			1 => ResultType.Float,
			2 => ResultType.Vector2,
			3 => ResultType.Vector3,
			4 => ResultType.Color,
			_ => throw new NotImplementedException(),
		};

		return new NodeResult( resultType, $"step( {(edge.IsValid ? edge : "0.0f")}, {(input.IsValid ? input : "0.0f")} )" );
	};
}

/// <summary>
/// Used to create a smooth transition between two input values (or edges).
/// </summary>
[Title( "Smooth Step" ), Category( "Math/Unary" )]
public sealed class SmoothStep : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Hide, JsonIgnore]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.UnaryNode;

	[Input]
	[Hide]
	public NodeInput Input { get; set; }

	[Input]
	[Hide]
	public NodeInput Edge1 { get; set; }

	[Input]
	[Hide]
	public NodeInput Edge2 { get; set; }

	[InputDefault( nameof( Input ) )]
	public float DefaultInput { get; set; } = 0.0f;

	[InputDefault( nameof( Edge1 ) )]
	public float DefaultEdge1 { get; set; } = 0.0f;

	[InputDefault( nameof( Edge2 ) )]
	public float DefaultEdge2 { get; set; } = 0.0f;

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var edge1 = compiler.Result( Edge1 );
		var edge2 = compiler.Result( Edge2 );
		var input = compiler.Result( Input );

		int maxComponents = Math.Max( edge1.IsValid ? edge1.Components : 1, input.IsValid ? input.Components : 1 );
		maxComponents = Math.Max( edge2.IsValid ? edge2.Components : 1, maxComponents );

		var edge1String = edge1.IsValid ? edge1.ToString() : compiler.ResultValue( DefaultEdge1 ).ToString();
		var edge2String = edge2.IsValid ? edge2.ToString() : compiler.ResultValue( DefaultEdge2 ).ToString();
		var inputString = input.IsValid ? input.ToString() : compiler.ResultValue( DefaultInput ).ToString();

		ResultType resultType = maxComponents switch
		{
			1 => ResultType.Float,
			2 => ResultType.Vector2,
			3 => ResultType.Vector3,
			4 => ResultType.Color,
			_ => throw new NotImplementedException(),
		};

		return new NodeResult( resultType, $"smoothstep( {edge1String}, {edge2String}, {inputString} )" );
	};
}

/// <summary>
/// Computes the tangent of a specified angle (in radians).
/// </summary>
[Title( "Tangent" ), Category( "Math/Unary" )]
public sealed class Tan : Unary
{
	[Hide]
	protected override string Op => "tan";
}

/// <summary>
/// Computes the angle (in radians) whose sine is the specified number.
/// </summary>
[Title( "Arcsin" ), Category( "Math/Unary" )]
public sealed class Arcsin : Unary
{
	[Hide]
	protected override string Op => "asin";
}

/// <summary>
/// Computes the angle (in radians) whose cosine is the specified number.
/// </summary>
[Title( "Arccos" ), Category( "Math/Unary" )]
public sealed class Arccos : Unary
{
	[Hide]
	protected override string Op => "acos";
}

/// <summary>
/// Round to the nearest integer.
/// </summary>
[Title( "Round" ), Category( "Math/Unary" )]
public sealed class Round : Unary
{
	[Hide]
	protected override string Op => "round";
}

/// <summary>
/// Returns the smallest integer value that is greater than or equal to the specified value.
/// </summary>
[Title( "Ceil" ), Category( "Math/Unary" )]
public sealed class Ceil : Unary
{
	[Hide]
	protected override string Op => "ceil";
}

/// <summary>
/// Returns the reuslt of the input value subtracted from 1.
/// </summary>
[Title( "One Minus" ), Category( "Math/Unary" )]
public sealed class OneMinus : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Hide, JsonIgnore]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.UnaryNode;

	[Input( typeof( float ) ), Hide, Title( "" )]
	public NodeInput In { get; set; }

	[Output, Hide, Title( "" )]
	public NodeResult.Func Out => ( GraphCompiler compiler ) =>
	{
		var result = compiler.ResultOrDefault( In, 0.0f );
		return new NodeResult( result.ResultType, $"1 - {result}" );
	};

	public OneMinus() : base()
	{
		ExpandSize = new Vector3( -85, 0 );
	}
}

/// <summary>
/// Positive values passed in become negative and negative values passed in become positive.
/// </summary>
[Title( "Negate" ), Category( "Math/Unary" )]
public sealed class Negate : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;


	[Input( typeof( float ) ), Hide, Title( "" )]
	public NodeInput In { get; set; }

	public Negate() : base()
	{
		ExpandSize = new Vector3( -85, 0 );
	}

	[Output, Hide, Title( "" )]
	public NodeResult.Func Out => ( GraphCompiler compiler ) =>
	{
		var result = compiler.ResultOrDefault( In, 0.0f );
		return new NodeResult( result.ResultType, $"-1 * {result}" );
	};
}

/// <summary>
/// Returns a distance scalar between two vectors.
/// </summary>
[Title( "Distance" ), Category( "Math/Unary" )]
public sealed class Distance : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Hide, JsonIgnore]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.UnaryNode;

	[Input]
	[Hide]
	public NodeInput A { get; set; }

	[Input]
	[Hide]
	public NodeInput B { get; set; }

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var resultA = compiler.ResultOrDefault( A, 0.0f );
		var resultB = compiler.ResultOrDefault( B, 0.0f ).Cast( resultA.Components );

		return new NodeResult( ResultType.Float, $"distance( {resultA}, {resultB} )" );
	};
}
