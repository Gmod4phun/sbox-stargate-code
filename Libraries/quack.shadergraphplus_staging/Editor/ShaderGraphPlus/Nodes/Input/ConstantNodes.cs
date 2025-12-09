
namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Bool value.
/// </summary>
[Title( "Bool Constant" ), Category( "Constants" ), Icon( "check_box" ), Order( 0 )]
public sealed class BoolConstantNode : ConstantNode<bool>
{
	[Hide]
	public override int Version => 1;

	public BoolConstantNode() : base()
	{
		Value = false;
	}

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Output( typeof( bool ) ), Title( "Value" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( "", Value, default, default, false, false, default );
	};

	public override BaseBlackboardParameter InitializeMaterialParameter( string name )
	{
		return new BoolParameter( name, Value, false )
		{
		};
	}

	public override BaseBlackboardParameter InitializeSubgraphInputParameter( string name )
	{
		return new BoolSubgraphInputParameter( name, Value )
		{
		};
	}
}

///<summary>
/// Single int value.
///</summary>
[Title( "Int Constant" ), Category( "Constants" ), Icon( "looks_one" ), Order( 1 )]
public sealed class IntConstantNode : ConstantNode<int>, IRangedConstantNode
{
	[Hide]
	public override int Version => 1;

	[Group( "Range" )] public int Min { get; set; }
	[Group( "Range" )] public int Max { get; set; }
	[Hide, JsonIgnore] public float Step => 1;

	public IntConstantNode() : base()
	{
		Value = 1;
		Min = 0;
		Max = 1;
	}

	[Output( typeof( int ) ), Title( "Value" )]
	[Hide, NodeValueEditor( nameof( Value ) ), Range( nameof( Min ), nameof( Max ), nameof( Step ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( "", Value, default, default, false, false, default );
	};

	public object GetMinValue()
	{
		return Min;
	}

	public object GetMaxValue()
	{
		return Max;
	}

	public object GetStepValue()
	{
		return 0.0f;
	}

	public override BaseBlackboardParameter InitializeMaterialParameter( string name )
	{
		return new IntParameter( name, Value, false )
		{
			Min = Min,
			Max = Max
		};
	}

	public override BaseBlackboardParameter InitializeSubgraphInputParameter( string name )
	{
		return new IntSubgraphInputParameter( name, Value )
		{
			Min = Min,
			Max = Max,
			IsRequired = false,
		};
	}
}

/// <summary>
/// Single float value.
/// </summary>
[Title( "Float Constant" ), Category( "Constants" ), Icon( "looks_one" ), Order( 2 )]
public sealed class FloatConstantNode : ConstantNode<float>, IRangedConstantNode
{
	[Hide]
	public override int Version => 1;

	[Group( "Range" )] public float Min { get; set; }
	[Group( "Range" )] public float Max { get; set; }
	public float Step { get; set; } = 0.0f;

	public FloatConstantNode() : base()
	{
		Value = 1.0f;
		Min = 0.0f;
		Max = 1.0f;
	}

	[Output( typeof( float ) ), Title( "Value" )]
	[Hide, NodeValueEditor( nameof( Value ) ), Range( nameof( Min ), nameof( Max ), nameof( Step ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( "", Value, default, default, false, false, default );
	};

	public object GetMinValue()
	{
		return Min;
	}

	public object GetMaxValue()
	{
		return Max;
	}

	public object GetStepValue()
	{
		return Step;
	}

	public override BaseBlackboardParameter InitializeMaterialParameter( string name )
	{
		return new FloatParameter( name, Value, false )
		{
			Min = Min,
			Max = Max,
			UI = new() { Step = Step, ShowStepProperty = true }
		};
	}

	public override BaseBlackboardParameter InitializeSubgraphInputParameter( string name )
	{
		return new FloatSubgraphInputParameter( name, Value )
		{
			Min = Min,
			Max = Max,
			IsRequired = false,
		};
	}
}

/// <summary>
/// 2 float values.
/// </summary>
[Title( "Float2 Constant" ), Category( "Constants" ), Icon( "looks_two" ), Order( 3 )]
public sealed class Float2ConstantNode : ConstantNode<Vector2>, IRangedConstantNode
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide]
	public float ValueX
	{
		get => Value.x;
		set => Value = Value.WithX( value );
	}

	[JsonIgnore, Hide]
	public float ValueY
	{
		get => Value.y;
		set => Value = Value.WithY( value );
	}

	[Group( "Range" )] public Vector2 Min { get; set; }
	[Group( "Range" )] public Vector2 Max { get; set; }
	public float Step { get; set; } = 0.0f;

	[Hide] public float MinX => Min.x;
	[Hide] public float MinY => Min.y;
	[Hide] public float MaxX => Max.x;
	[Hide] public float MaxY => Max.y;

	public Float2ConstantNode() : base()
	{
		Value = Vector2.One;
		Min = Vector2.Zero;
		Max = Vector2.One;
	}

	[Output( typeof( Vector2 ) ), Title( "XY" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( "", Value, default, default, false, false, default );
	};

	/// <summary>
	/// X component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeValueEditor( nameof( ValueX ) ), Title( "X" )]
	[Range( nameof( MinX ), nameof( MaxX ), nameof( Step ) )]
	public NodeResult.Func X => ( GraphCompiler compiler ) => Component( "x", ValueX, compiler );

	/// <summary>
	/// Y component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeValueEditor( nameof( ValueY ) ), Title( "Y" )]
	[Range( nameof( MinY ), nameof( MaxY ), nameof( Step ) )]
	public NodeResult.Func Y => ( GraphCompiler compiler ) => Component( "y", ValueY, compiler );

	public object GetMinValue()
	{
		return Min;
	}

	public object GetMaxValue()
	{
		return Max;
	}

	public object GetStepValue()
	{
		return Step;
	}

	public override BaseBlackboardParameter InitializeMaterialParameter( string name )
	{
		return new Float2Parameter( name, Value, false )
		{
			Min = Min,
			Max = Max,
			UI = new() { Step = Step, ShowStepProperty = true }
		};
	}

	public override BaseBlackboardParameter InitializeSubgraphInputParameter( string name )
	{
		return new Float2SubgraphInputParameter( name, Value )
		{
			Min = Min,
			Max = Max,
			IsRequired = false,
		};
	}
}

/// <summary>
/// 3 float values.
/// </summary>
[Title( "Float3 Constant" ), Category( "Constants" ), Icon( "looks_3" ), Order( 4 )]
public sealed class Float3ConstantNode : ConstantNode<Vector3>, IRangedConstantNode
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide]
	public float ValueX
	{
		get => Value.x;
		set => Value = Value.WithX( value );
	}

	[JsonIgnore, Hide]
	public float ValueY
	{
		get => Value.y;
		set => Value = Value.WithY( value );
	}

	[JsonIgnore, Hide]
	public float ValueZ
	{
		get => Value.z;
		set => Value = Value.WithZ( value );
	}

	[Group( "Range" )] public Vector3 Min { get; set; }
	[Group( "Range" )] public Vector3 Max { get; set; }
	public float Step { get; set; } = 0.0f;

	[Hide] public float MinX => Min.x;
	[Hide] public float MinY => Min.y;
	[Hide] public float MinZ => Min.z;
	[Hide] public float MaxX => Max.x;
	[Hide] public float MaxY => Max.y;
	[Hide] public float MaxZ => Max.z;

	public Float3ConstantNode() : base()
	{
		Value = Vector3.One;
		Min = Vector3.Zero;
		Max = Vector3.One;
	}

	[Output( typeof( Vector3 ) ), Title( "XYZ" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( "", Value, default, default, false, false, default );
	};

	/// <summary>
	/// X component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeValueEditor( nameof( ValueX ) ), Title( "X" )]
	[Range( nameof( MinX ), nameof( MaxX ), nameof( Step ) )]
	public NodeResult.Func X => ( GraphCompiler compiler ) => Component( "x", ValueX, compiler );

	/// <summary>
	/// Y component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeValueEditor( nameof( ValueY ) ), Title( "Y" )]
	[Range( nameof( MinY ), nameof( MaxY ), nameof( Step ) )]
	public NodeResult.Func Y => ( GraphCompiler compiler ) => Component( "y", ValueY, compiler );

	/// <summary>
	/// Z component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeValueEditor( nameof( ValueZ ) ), Title( "Z" )]
	[Range( nameof( MinZ ), nameof( MaxZ ), nameof( Step ) )]
	public NodeResult.Func Z => ( GraphCompiler compiler ) => Component( "z", ValueZ, compiler );

	public object GetMinValue()
	{
		return Min;
	}

	public object GetMaxValue()
	{
		return Max;
	}

	public object GetStepValue()
	{
		return Step;
	}

	public override BaseBlackboardParameter InitializeMaterialParameter( string name )
	{
		return new Float3Parameter( name, Value, false )
		{
			Min = Min,
			Max = Max,
			UI = new() { Step = Step, ShowStepProperty = true }
		};
	}

	public override BaseBlackboardParameter InitializeSubgraphInputParameter( string name )
	{
		return new Float3SubgraphInputParameter( name, Value )
		{
			Min = Min,
			Max = Max,
			IsRequired = false,
		};
	}
}

/// <summary>
/// 4 float values.
/// </summary>
[Title( "Float4 Constant" ), Category( "Constants" ), Icon( "looks_4" ), Order( 5 )]
public sealed class Float4ConstantNode : ConstantNode<Vector4>, IRangedConstantNode
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide]
	public float ValueX
	{
		get => Value.x;
		set => Value = Value.WithX( value );
	}

	[JsonIgnore, Hide]
	public float ValueY
	{
		get => Value.y;
		set => Value = Value.WithY( value );
	}

	[JsonIgnore, Hide]
	public float ValueZ
	{
		get => Value.z;
		set => Value = Value.WithZ( value );
	}

	[JsonIgnore, Hide]
	public float ValueW
	{
		get => Value.w;
		set => Value = Value.WithW( value );
	}

	[Group( "Range" )] public Vector4 Min { get; set; }
	[Group( "Range" )] public Vector4 Max { get; set; }
	public float Step { get; set; } = 0.0f;

	[Hide] public float MinX => Min.x;
	[Hide] public float MinY => Min.y;
	[Hide] public float MinZ => Min.z;
	[Hide] public float MinW => Min.z;
	[Hide] public float MaxX => Max.x;
	[Hide] public float MaxY => Max.y;
	[Hide] public float MaxZ => Max.z;
	[Hide] public float MaxW => Max.z;

	public Float4ConstantNode() : base()
	{
		Value = Vector4.One;
		Min = Vector4.Zero;
		Max = Vector4.One;
	}

	[Output( typeof( Vector4 ) ), Title( "XYZW" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( "", Value, default, default, false, false, default );
	};

	/// <summary>
	/// X component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeValueEditor( nameof( ValueX ) ), Title( "X" )]
	[Range( nameof( MinX ), nameof( MaxX ), nameof( Step ) )]
	public NodeResult.Func X => ( GraphCompiler compiler ) => Component( "x", ValueX, compiler );

	/// <summary>
	/// Green component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeValueEditor( nameof( ValueY ) ), Title( "Y" )]
	[Range( nameof( MinY ), nameof( MaxY ), nameof( Step ) )]
	public NodeResult.Func Y => ( GraphCompiler compiler ) => Component( "y", ValueY, compiler );

	/// <summary>
	/// Y component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeValueEditor( nameof( ValueZ ) ), Title( "Z" )]
	[Range( nameof( MinZ ), nameof( MaxZ ), nameof( Step ) )]
	public NodeResult.Func Z => ( GraphCompiler compiler ) => Component( "z", ValueZ, compiler );

	/// <summary>
	/// W component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeValueEditor( nameof( ValueW ) ), Title( "W" )]
	[Range( nameof( MinW ), nameof( MaxW ), nameof( Step ) )]
	public NodeResult.Func W => ( GraphCompiler compiler ) => Component( "w", ValueW, compiler );

	public object GetMinValue()
	{
		return Min;
	}

	public object GetMaxValue()
	{
		return Max;
	}

	public object GetStepValue()
	{
		return Step;
	}

	public override BaseBlackboardParameter InitializeMaterialParameter( string name )
	{
		return new Float4Parameter( name, Value, false )
		{
			Min = Min,
			Max = Max,
			UI = new() { Step = Step, ShowStepProperty = true }
		};
	}

	public override BaseBlackboardParameter InitializeSubgraphInputParameter( string name )
	{
		return new Float4SubgraphInputParameter( name, Value )
		{
			Min = Min,
			Max = Max,
			IsRequired = false,
		};
	}
}

/// <summary>
/// 4 float values, Just like <see cref="Float4ConstantNode"/> but with color control ui.
/// </summary>
[Title( "Color Constant" ), Category( "Constants" ), Icon( "palette" ), Order( 6 )]
public sealed class ColorConstantNode : ConstantNode<Color>
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide]
	public float ValueR
	{
		get => Value.r;
		set => Value = Value.WithRed( value );
	}

	[JsonIgnore, Hide]
	public float ValueG
	{
		get => Value.g;
		set => Value = Value.WithGreen( value );
	}

	[JsonIgnore, Hide]
	public float ValueB
	{
		get => Value.b;
		set => Value = Value.WithBlue( value );
	}

	[JsonIgnore, Hide]
	public float ValueA
	{
		get => Value.a;
		set => Value = Value.WithAlpha( value );
	}

	public ColorConstantNode() : base()
	{
		Value = Color.White;
	}

	[Output( typeof( Color ) ), Title( "RGBA" )]
	[Hide, NodeValueEditor( nameof( Value ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( "", Value, default, default, false, false, default );
	};

	/// <summary>
	/// Red component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeValueEditor( nameof( ValueR ) ), Title( "Red" )]
	public NodeResult.Func R => ( GraphCompiler compiler ) => Component( "r", ValueR, compiler );

	/// <summary>
	/// Green component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeValueEditor( nameof( ValueG ) ), Title( "Green" )]
	public NodeResult.Func G => ( GraphCompiler compiler ) => Component( "g", ValueG, compiler );

	/// <summary>
	/// Green component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeValueEditor( nameof( ValueB ) ), Title( "Blue" )]
	public NodeResult.Func B => ( GraphCompiler compiler ) => Component( "b", ValueB, compiler );

	/// <summary>
	/// Alpha component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeValueEditor( nameof( ValueA ) ), Title( "Alpha" )]
	public NodeResult.Func A => ( GraphCompiler compiler ) => Component( "a", ValueA, compiler );

	public override BaseBlackboardParameter InitializeMaterialParameter( string name )
	{
		return new ColorParameter( name, Value, false )
		{
		};
	}

	public override BaseBlackboardParameter InitializeSubgraphInputParameter( string name )
	{
		return new ColorSubgraphInputParameter( name, Value )
		{
			IsRequired = false,
		};
	}
}
