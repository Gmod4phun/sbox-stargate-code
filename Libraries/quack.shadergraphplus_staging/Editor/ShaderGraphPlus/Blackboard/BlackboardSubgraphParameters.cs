namespace ShaderGraphPlus;

/// <summary>
/// Bool value subgraph input
/// </summary>
[Title( "Bool" ), Icon( "check_box" ), Order( 0 )]
[SubgraphOnly]
public sealed class BoolSubgraphInputParameter : BlackboardSubgraphInputParameter<bool>
{
	public BoolSubgraphInputParameter() : base()
	{
		Value = false;
	}

	public BoolSubgraphInputParameter( string name, bool value ) : base( name, value )
	{

	}

	public override BaseNodePlus InitializeNode()
	{
		return new SubgraphInput()
		{
			BlackboardParameterIdentifier = Identifier,
			InputName = Name,
			InputDescription = Description,
			InputType = SubgraphPortType.Bool,
			DefaultData = Value,
			IsRequired = IsRequired,
			PortOrder = PortOrder,
		};
	}
}

/// <summary>
/// Int value subgraph input
/// </summary>
[Title( "Int" ), Icon( "looks_one" ), Order( 1 )]
[SubgraphOnly]
public sealed class IntSubgraphInputParameter : BlackboardSubgraphInputParameter<int>
{
	[Group( "Range" )] public int Min { get; set; }
	[Group( "Range" )] public int Max { get; set; }

	public IntSubgraphInputParameter()
	{
		Value = 1;
		Min = 0;
		Max = 1;
	}

	public IntSubgraphInputParameter( string name, int value ) : base( name, value )
	{
		Min = 0;
		Max = 1;
	}

	public override BaseNodePlus InitializeNode()
	{
		return new SubgraphInput()
		{
			BlackboardParameterIdentifier = Identifier,
			InputName = Name,
			InputDescription = Description,
			InputType = SubgraphPortType.Int,
			DefaultData = Value,
			IsRequired = IsRequired,
			PortOrder = PortOrder,
		};
	}
}

/// <summary>
/// Float value subgraph input
/// </summary>
[Title( "Float" ), Icon( "looks_one" ), Order( 2 )]
[SubgraphOnly]
public sealed class FloatSubgraphInputParameter : BlackboardSubgraphInputParameter<float>
{
	[Group( "Range" )] public float Min { get; set; }
	[Group( "Range" )] public float Max { get; set; }

	public FloatSubgraphInputParameter()
	{
		Value = 1.0f;
		Min = 0.0f;
		Max = 1.0f;
	}

	public FloatSubgraphInputParameter( string name, float value ) : base( name, value )
	{
		Min = 0.0f;
		Max = 1.0f;
	}

	public override BaseNodePlus InitializeNode()
	{
		return new SubgraphInput()
		{
			BlackboardParameterIdentifier = Identifier,
			InputName = Name,
			InputDescription = Description,
			InputType = SubgraphPortType.Float,
			DefaultData = Value,
			IsRequired = IsRequired,
			PortOrder = PortOrder,
		};
	}
}

/// <summary>
/// Float2 value subgraph input
/// </summary>
[Title( "Float2" ), Icon( "looks_two" ), Order( 3 )]
[SubgraphOnly]
public sealed class Float2SubgraphInputParameter : BlackboardSubgraphInputParameter<Vector2>
{
	[Group( "Range" )] public Vector2 Min { get; set; }
	[Group( "Range" )] public Vector2 Max { get; set; }

	public Float2SubgraphInputParameter()
	{
		Value = Vector2.One;
		Min = Vector2.Zero;
		Max = Vector2.One;
	}

	public Float2SubgraphInputParameter( string name, Vector2 value ) : base( name, value )
	{
		Min = Vector2.Zero;
		Max = Vector2.One;
	}

	public override BaseNodePlus InitializeNode()
	{
		return new SubgraphInput()
		{
			BlackboardParameterIdentifier = Identifier,
			InputName = Name,
			InputDescription = Description,
			InputType = SubgraphPortType.Vector2,
			DefaultData = Value,
			IsRequired = IsRequired,
			PortOrder = PortOrder,
		};
	}
}

/// <summary>
/// Float3 value subgraph input
/// </summary>
[Title( "Float3" ), Icon( "looks_3" ), Order( 4 )]
[SubgraphOnly]
public sealed class Float3SubgraphInputParameter : BlackboardSubgraphInputParameter<Vector3>
{
	[Group( "Range" )] public Vector3 Min { get; set; }
	[Group( "Range" )] public Vector3 Max { get; set; }

	public Float3SubgraphInputParameter()
	{
		Value = Vector3.One;
		Min = Vector3.Zero;
		Max = Vector3.One;
	}

	public Float3SubgraphInputParameter( string name, Vector3 value ) : base( name, value )
	{
		Min = Vector3.Zero;
		Max = Vector3.One;
	}

	public override BaseNodePlus InitializeNode()
	{
		return new SubgraphInput()
		{
			BlackboardParameterIdentifier = Identifier,
			InputName = Name,
			InputDescription = Description,
			InputType = SubgraphPortType.Vector3,
			DefaultData = Value,
			IsRequired = IsRequired,
			PortOrder = PortOrder,
		};
	}
}

/// <summary>
/// Float4 value subgraph input
/// </summary>
[Title( "Float4" ), Icon( "looks_4" ), Order( 5 )]
[SubgraphOnly]
public sealed class Float4SubgraphInputParameter : BlackboardSubgraphInputParameter<Vector4>
{
	[Group( "Range" )] public Vector4 Min { get; set; }
	[Group( "Range" )] public Vector4 Max { get; set; }

	public Float4SubgraphInputParameter()
	{
		Value = Vector4.One;
		Min = Vector4.Zero;
		Max = Vector4.One;
	}

	public Float4SubgraphInputParameter( string name, Vector4 value ) : base( name, value )
	{
		Min = Vector4.Zero;
		Max = Vector4.One;
	}

	public override BaseNodePlus InitializeNode()
	{
		return new SubgraphInput()
		{
			BlackboardParameterIdentifier = Identifier,
			InputName = Name,
			InputDescription = Description,
			InputType = SubgraphPortType.Vector4,
			DefaultData = Value,
			IsRequired = IsRequired,
			PortOrder = PortOrder,
		};
	}
}

/// <summary>
/// Color value subgraph input
/// </summary>
[Title( "Color" ), Icon( "palette" ), Order( 6 )]
[SubgraphOnly]
public sealed class ColorSubgraphInputParameter : BlackboardSubgraphInputParameter<Color>
{
	public ColorSubgraphInputParameter()
	{
		Value = Color.White;
	}

	public ColorSubgraphInputParameter( string name, Color value ) : base( name, value )
	{
	}

	public override BaseNodePlus InitializeNode()
	{
		return new SubgraphInput()
		{
			BlackboardParameterIdentifier = Identifier,
			InputName = Name,
			InputDescription = Description,
			InputType = SubgraphPortType.Color,
			DefaultData = Value,
			IsRequired = IsRequired,
			PortOrder = PortOrder,
		};
	}
}

/// <summary>
/// Float2x2 value subgraph input
/// </summary>
[Title( "Float2x2" ), Icon( "apps" ), Order( 7 )]
[SubgraphOnly]
public sealed class Float2x2SubgraphInputParameter : BlackboardSubgraphInputParameter<Float2x2>
{
	public Float2x2SubgraphInputParameter() : base()
	{
		Value = new Float2x2();
	}

	public Float2x2SubgraphInputParameter( string name, Float2x2 value ) : base( name, value )
	{
	}

	public override BaseNodePlus InitializeNode()
	{
		return new SubgraphInput()
		{
			BlackboardParameterIdentifier = Identifier,
			InputName = Name,
			InputDescription = Description,
			InputType = SubgraphPortType.Float2x2,
			DefaultData = Value,
		};
	}
}

/// <summary>
/// Float3x3 value subgraph input
/// </summary>
[Title( "Float3x3" ), Icon( "apps" ), Order( 8 )]
[SubgraphOnly]
public sealed class Float3x3SubgraphInputParameter : BlackboardSubgraphInputParameter<Float3x3>
{
	public Float3x3SubgraphInputParameter() : base()
	{
		Value = new Float3x3();
	}

	public Float3x3SubgraphInputParameter( string name, Float3x3 value ) : base( name, value )
	{
	}

	public override BaseNodePlus InitializeNode()
	{
		return new SubgraphInput()
		{
			BlackboardParameterIdentifier = Identifier,
			InputName = Name,
			InputDescription = Description,
			InputType = SubgraphPortType.Float3x3,
			DefaultData = Value,
		};
	}
}

/// <summary>
/// Float4x4 value subgraph input
/// </summary>
[Title( "Float4x4" ), Icon( "apps" ), Order( 9 )]
[SubgraphOnly]
public sealed class Float4x4SubgraphInputParameter : BlackboardSubgraphInputParameter<Float4x4>
{
	public Float4x4SubgraphInputParameter() : base()
	{
		Value = new Float4x4();
	}

	public Float4x4SubgraphInputParameter( string name, Float4x4 value ) : base( name, value )
	{
	}

	public override BaseNodePlus InitializeNode()
	{
		return new SubgraphInput()
		{
			BlackboardParameterIdentifier = Identifier,
			InputName = Name,
			InputDescription = Description,
			InputType = SubgraphPortType.Float4x4,
			DefaultData = Value,
		};
	}
}

/// <summary>
/// Gradient value subgraph input
/// </summary>
[Title( "Gradient" ), Icon( "gradient" ), Order( 10 )]
[SubgraphOnly]
public sealed class GradientSubgraphInputParameter : BlackboardSubgraphInputParameter<Gradient>
{
	public GradientSubgraphInputParameter() : base()
	{
		Value = new Gradient();
	}

	public GradientSubgraphInputParameter( string name, Gradient value ) : base( name, value )
	{
	}

	public override BaseNodePlus InitializeNode()
	{
		return new SubgraphInput()
		{
			BlackboardParameterIdentifier = Identifier,
			InputName = Name,
			InputDescription = Description,
			InputType = SubgraphPortType.Gradient,
			DefaultData = Value,
		};
	}
}

/// <summary>
/// Texture2D subgraph input
/// </summary>
[Title( "Texture2D" ), Icon( "texture" ), Order( 11 )]
[SubgraphOnly]
public sealed class Texture2DSubgraphInputParameter : BlackboardSubgraphInputParameter<TextureInput>
{
	public Texture2DSubgraphInputParameter() : base()
	{
		Value = new TextureInput
		{
			Name = Name,
			ImageFormat = TextureFormat.DXT5,
			SrgbRead = true,
			DefaultColor = Color.White,
			Type = TextureType.Tex2D,
		};
	}

	public Texture2DSubgraphInputParameter( string name, TextureInput value ) : base( name, value )
	{
	}

	public override BaseNodePlus InitializeNode()
	{
		return new SubgraphInput()
		{
			BlackboardParameterIdentifier = Identifier,
			InputName = Name,
			InputDescription = Description,
			InputType = SubgraphPortType.Texture2DObject,
			DefaultData = Value with { Name = Name, Type = TextureType.Tex2D },
		};
	}
}

/// <summary>
/// TextureCube subgraph input
/// </summary>
[Title( "TextureCube" ), Icon( "view_in_ar" ), Order( 12 )]
[SubgraphOnly]
public sealed class TextureCubeSubgraphInputParameter : BlackboardSubgraphInputParameter<TextureInput>
{
	public TextureCubeSubgraphInputParameter() : base()
	{
		Value = new TextureInput
		{
			Name = Name,
			ImageFormat = TextureFormat.DXT5,
			SrgbRead = true,
			DefaultColor = Color.White,
			Type = TextureType.TexCube,
		};
	}

	public TextureCubeSubgraphInputParameter( string name, TextureInput value ) : base( name, value )
	{
	}

	public override BaseNodePlus InitializeNode()
	{
		return new SubgraphInput()
		{
			BlackboardParameterIdentifier = Identifier,
			InputName = Name,
			InputDescription = Description,
			InputType = SubgraphPortType.TextureCubeObject,
			DefaultData = Value with { Name = Name, Type = TextureType.TexCube },
		};
	}
}

/// <summary>
/// SamplerState subgraph input
/// </summary>
[Title( "Sampler State" ), Icon( "colorize" ), Order( 13 )]
[SubgraphOnly]
public sealed class SamplerStateSubgraphInputParameter : BlackboardSubgraphInputParameter<Sampler>
{
	public SamplerStateSubgraphInputParameter() : base()
	{
		Value = new Sampler();
	}

	public SamplerStateSubgraphInputParameter( string name, Sampler value ) : base( name, value )
	{
	}

	public override BaseNodePlus InitializeNode()
	{
		return new SubgraphInput()
		{
			BlackboardParameterIdentifier = Identifier,
			InputName = Name,
			InputDescription = Description,
			InputType = SubgraphPortType.SamplerState,
			DefaultData = Value,
		};
	}
}
