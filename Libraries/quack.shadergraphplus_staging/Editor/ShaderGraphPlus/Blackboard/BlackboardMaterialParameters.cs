using Editor.ShaderGraph.Nodes;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

/// <summary>
/// Bool value material parameter
/// </summary>
[Title( "Bool" ), Icon( "check_box" ), Order( 0 )]
public sealed class BoolParameter : BlackboardMaterialParameter<bool>
{
	public BoolParameter() : base()
	{
		UI = new ParameterUI() { ShowStepProperty = false, ShowTypeProperty = false };
		Value = false;
	}

	public BoolParameter( string name, bool value, bool isAttribute ) : base( name, value, isAttribute )
	{
		UI = new ParameterUI() { ShowStepProperty = false, ShowTypeProperty = false };
	}

	public override BaseNodePlus InitializeNode()
	{
		return new BoolParameterNode()
		{
			BlackboardParameterIdentifier = Identifier,
			Name = Name,
			Value = Value,
			IsAttribute = IsAttribute,
			UI = UI with { ShowTypeProperty = false, ShowStepProperty = false },
		};
	}
}

/// <summary>
/// Int value material parameter
/// </summary>
[Title( "Int" ), Icon( "looks_one" ), Order( 1 )]
public sealed class IntParameter : BlackboardMaterialParameter<int>
{
	[Group( "Range" )] public int Min { get; set; }
	[Group( "Range" )] public int Max { get; set; }

	public IntParameter() : base()
	{
		Value = 1;
		Min = 0;
		Max = 1;
		UI = new ParameterUI { Type = UIType.Default, ShowStepProperty = false };
	}

	public IntParameter( string name, int value, bool isAttribute ) : base( name, value, isAttribute )
	{
		Min = 0;
		Max = 1;
		UI = new ParameterUI { Type = UIType.Default, ShowStepProperty = false };
	}

	public override BaseNodePlus InitializeNode()
	{
		return new IntParameterNode()
		{
			BlackboardParameterIdentifier = Identifier,
			Name = Name,
			Value = Value,
			IsAttribute = IsAttribute,
			UI = UI with { ShowStepProperty = false }
		};
	}
}

/// <summary>
/// Float value material parameter
/// </summary>
[Title( "Float" ), Icon( "looks_one" ), Order( 2 )]
public sealed class FloatParameter : BlackboardMaterialParameter<float>
{
	[Group( "Range" )] public float Min { get; set; }
	[Group( "Range" )] public float Max { get; set; }

	public FloatParameter() : base()
	{
		Value = 1.0f;
		Min = 0.0f;
		Max = 1.0f;
		UI = new ParameterUI { Type = UIType.Default };
	}

	public FloatParameter( string name, float value, bool isAttribute ) : base( name, value, isAttribute )
	{
		Min = 0.0f;
		Max = 1.0f;
		UI = new ParameterUI { Type = UIType.Default };
	}

	public override BaseNodePlus InitializeNode()
	{
		return new FloatParameterNode()
		{
			BlackboardParameterIdentifier = Identifier,
			Name = Name,
			Value = Value,
			IsAttribute = IsAttribute,
			UI = UI
		};
	}
}

/// <summary>
/// Float2 value material parameter
/// </summary>
[Title( "Float2" ), Icon( "looks_two" ), Order( 3 )]
public sealed class Float2Parameter : BlackboardMaterialParameter<Vector2>
{
	[Group( "Range" )] public Vector2 Min { get; set; }
	[Group( "Range" )] public Vector2 Max { get; set; }

	public Float2Parameter() : base()
	{
		Value = Vector2.One;
		Min = Vector2.Zero;
		Max = Vector2.One;
		UI = new ParameterUI { Type = UIType.Default };
	}

	public Float2Parameter( string name, Vector2 value, bool isAttribute ) : base( name, value, isAttribute )
	{
		Min = Vector2.Zero;
		Max = Vector2.One;
		UI = new ParameterUI { Type = UIType.Default };
	}

	public override BaseNodePlus InitializeNode()
	{
		return new Float2ParameterNode()
		{
			BlackboardParameterIdentifier = Identifier,
			Name = Name,
			Value = Value,
			IsAttribute = IsAttribute,
			UI = UI
		};
	}
}

/// <summary>
/// Float3 value material parameter
/// </summary>
[Title( "Float3" ), Icon( "looks_3" ), Order( 4 )]
public sealed class Float3Parameter : BlackboardMaterialParameter<Vector3>
{
	[Group( "Range" )] public Vector3 Min { get; set; }
	[Group( "Range" )] public Vector3 Max { get; set; }

	public Float3Parameter() : base()
	{
		Value = Vector3.One;
		Min = Vector3.Zero;
		Max = Vector3.One;
		UI = new ParameterUI { Type = UIType.Default };
	}

	public Float3Parameter( string name, Vector3 value, bool isAttribute ) : base( name, value, isAttribute )
	{
		Min = Vector3.Zero;
		Max = Vector3.One;
		UI = new ParameterUI { Type = UIType.Default };
	}

	public override BaseNodePlus InitializeNode()
	{
		return new Float3ParameterNode()
		{
			BlackboardParameterIdentifier = Identifier,
			Name = Name,
			Value = Value,
			IsAttribute = IsAttribute,
			UI = UI
		};
	}
}

/// <summary>
/// Float4 value material parameter
/// </summary>
[Title( "Float4" ), Icon( "looks_4" ), Order( 5 )]
public sealed class Float4Parameter : BlackboardMaterialParameter<Vector4>
{
	[Group( "Range" )] public Vector4 Min { get; set; }
	[Group( "Range" )] public Vector4 Max { get; set; }

	public Float4Parameter() : base()
	{
		Value = Vector4.One;
		Min = Vector4.Zero;
		Max = Vector4.One;
		UI = new ParameterUI { Type = UIType.Default };
	}

	public Float4Parameter( string name, Vector4 value, bool isAttribute ) : base( name, value, isAttribute )
	{
		Min = Vector4.Zero;
		Max = Vector4.One;
		UI = new ParameterUI { Type = UIType.Default };
	}

	public override BaseNodePlus InitializeNode()
	{
		return new Float4ParameterNode()
		{
			BlackboardParameterIdentifier = Identifier,
			Name = Name,
			Value = Value,
			IsAttribute = IsAttribute,
			UI = UI
		};
	}
}

/// <summary>
/// Color value material parameter
/// </summary>
[Title( "Color" ), Icon( "palette" ), Order( 6 )]
public sealed class ColorParameter : BlackboardMaterialParameter<Color>
{
	public ColorParameter() : base()
	{
		Value = Color.White;
		UI = new ParameterUI { Type = UIType.Color, ShowStepProperty = false };
	}

	public ColorParameter( string name, Color value, bool isAttribute ) : base( name, value, isAttribute )
	{
		UI = new ParameterUI { Type = UIType.Color, ShowStepProperty = false };
	}

	public override BaseNodePlus InitializeNode()
	{
		return new ColorParameterNode()
		{
			BlackboardParameterIdentifier = Identifier,
			Name = Name,
			Value = Value,
			IsAttribute = IsAttribute,
			UI = UI
		};
	}
}

/// <summary>
/// Texture2D material parameter
/// </summary>
[Title( "Texture2D" ), Icon( "texture" ), Order( 7 )]
public sealed class Texture2DParameter : BlackboardGenericParameter<TextureInput>
{
	public Texture2DParameter() : base()
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

	public Texture2DParameter( string name, TextureInput value ) : base( name, value )
	{
	}

	public override BaseNodePlus InitializeNode()
	{
		return new Texture2DParameterNode()
		{
			BlackboardParameterIdentifier = Identifier,
			UI = Value with { Name = Name, Type = TextureType.Tex2D },
		};
	}
}

/// <summary>
/// TextureCube material parameter
/// </summary>
[Title( "TextureCube" ), Icon( "view_in_ar" ), Order( 8 )]
public sealed class TextureCubeParameter : BlackboardGenericParameter<TextureInput>
{
	public TextureCubeParameter() : base()
	{
		Value = new TextureInput
		{
			Name = Name,
			IsAttribute = false,
			ImageFormat = TextureFormat.DXT5,
			SrgbRead = true,
			DefaultColor = Color.White,
			Type = TextureType.TexCube,
		};
	}

	public TextureCubeParameter( string name, TextureInput value ) : base( name, value )
	{
	}

	public override BaseNodePlus InitializeNode()
	{
		return new TextureCubeParameterNode()
		{
			BlackboardParameterIdentifier = Identifier,
			UI = Value with { Name = Name, Type = TextureType.TexCube },
		};
	}
}

// TODO : Implament the rest of SamplerStateParameter once SamplerState
// is exposed to the MaterialEditor.
/*
/// <summary>
/// SamplerState material parameter
/// </summary>
[Title( "Sampler State" ), Icon( "colorize" ), Order( 8 )]
public sealed class SamplerStateParameter : BlackboardGenericParameter<Sampler>
{
	public SamplerStateParameter() : base()
	{
		Value = new Sampler();
	}

	public SamplerStateParameter( Sampler value ) : base( value )
	{
	}

	public override BaseNodePlus InitializeNode()
	{
		throw new NotImplementedException();
	}
}
*/

/// <summary>
/// Bool material feature
/// </summary>
[Title( "Shader Feature Boolean" ), Order( 9 )]
public sealed class ShaderFeatureBooleanParameter : BaseBlackboardParameter, IShaderFeatureParameter
{
	[Hide, JsonIgnore, Browsable( false )]
	public override bool IsValid => !string.IsNullOrWhiteSpace( Name );

	/// <summary>
	/// Name of this feature.
	/// </summary>
	[Title( "Feature Name" )]
	public override string Name { get; set; }

	/// <summary>
	/// What this feature does.
	/// </summary>
	[Hide]
	public string Description { get; set; }

	/// <summary>
	/// Header Name of this Feature that shows up in the Material Editor.
	/// </summary>
	public string HeaderName { get; set; }

	public ShaderFeatureBooleanParameter() : base()
	{
	}

	public override BaseNodePlus InitializeNode()
	{
		return new BooleanFeatureSwitchNode()
		{
			BlackboardParameterIdentifier = Identifier,
			Feature = new ShaderFeatureBoolean()
			{
				Name = Name,
				Description = Description,
				HeaderName = HeaderName,
			}
		};
	}
}

/// <summary>
/// TODO : 
/// </summary>
[Title( "Shader Feature Enum" ), Order( 10 )]
public sealed class ShaderFeatureEnumParameter : BaseBlackboardParameter, IShaderFeatureParameter
{
	[Hide, JsonIgnore, Browsable( false )]
	public override bool IsValid => !string.IsNullOrWhiteSpace( Name ) && Options.All( x => !string.IsNullOrWhiteSpace( x ) );

	/// <summary>
	/// Name of this feature.
	/// </summary>
	[Title( "Feature Name" )]
	public override string Name { get; set; }

	/// <summary>
	/// What this feature does.
	/// </summary>
	[Hide]
	public string Description { get; set; }

	/// <summary>
	/// Header Name of this Feature that shows up in the Material Editor.
	/// </summary>
	public string HeaderName { get; set; }

	/// <summary>
	/// Options of your feature. Must have no special characters. Note : all lowercase letters will be converted to uppercase.
	/// </summary>
	public List<string> Options { get; set; }

	public ShaderFeatureEnumParameter() : base()
	{
		Options = new List<string>();
	}

	public override BaseNodePlus InitializeNode()
	{
		return new EnumFeatureSwitchNode()
		{
			BlackboardParameterIdentifier = Identifier,
			Feature = new ShaderFeatureEnum()
			{
				Name = Name,
				Description = Description,
				HeaderName = HeaderName,
				Options = Options
			}
		};
	}
}
