using Sandbox;
using System.Xml.Linq;

namespace ShaderGraphPlus.Nodes;

internal interface ITextureParameterNodeNew
{
	TextureInput UI { get; set; }
}

/// <summary>
/// Node that can return any generic data.
/// </summary>
internal interface IMetaDataNode
{
	NodeResult GetResult( GraphCompiler compiler );
}

/// <summary>
/// Bool value
/// </summary>
[Title( "Bool" ), Category( "Parameters" ), Icon( "check_box" )]
[InternalNode]
public sealed class BoolParameterNode : ParameterNodeBase<bool>
{
	[Hide]
	public override int Version => 2;

	public override void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is BoolParameter boolParam )
		{
			Name = boolParam.Name;
			Value = boolParam.Value;
			IsAttribute = boolParam.IsAttribute;
			UI = boolParam.UI;
		}
	}

	public BoolParameterNode() : base()
	{
		UI = UI with { ShowStepProperty = false, ShowTypeProperty = false };
	}

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Output( typeof( bool ) ), Title( "Value" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		UI = UI with { ShowStepProperty = false, ShowTypeProperty = false };
		return compiler.ResultParameter( Name, Value, default, default, false, IsAttribute, UI );
	};
}

///<summary>
/// Single int value
///</summary>
[Title( "Int" ), Category( "Parameters" ), Icon( "looks_one" )]
[InternalNode]
public sealed class IntParameterNode : ParameterNodeBase<int>
{
	[Hide]
	public override int Version => 2;

	public override void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is IntParameter intParam )
		{
			Name = intParam.Name;
			Value = intParam.Value;
			Min = intParam.Min;
			Max = intParam.Max;
			IsAttribute = intParam.IsAttribute;
			UI = intParam.UI;
		}
	}

	[Group( "Range" )] public int Min { get; set; }
	[Group( "Range" )] public int Max { get; set; }
	[Hide, JsonIgnore] public float Step => 1;

	public IntParameterNode()
	{
		Min = 0;
		Max = 1;
		UI = UI with { ShowStepProperty = false, ShowTypeProperty = true };
	}

	[Output( typeof( int ) ), Title( "Value" )]
	[Hide, NodeValueEditor( nameof( Value ) ), Range( nameof( Min ), nameof( Max ), nameof( Step ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		UI = UI with { ShowStepProperty = false, ShowTypeProperty = true };
		return compiler.ResultParameter( Name, Value, Min, Max, Min != Max, IsAttribute, UI );
	};
}

/// <summary>
/// Single float value
/// </summary>
[Title( "Float" ), Category( "Parameters" ), Icon( "looks_one" )]
[InternalNode]
public sealed class FloatParameterNode : ParameterNodeBase<float>
{
	[Hide]
	public override int Version => 2;

	public override void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is FloatParameter floatParam )
		{
			Name = floatParam.Name;
			Value = floatParam.Value;
			Min = floatParam.Min;
			Max = floatParam.Max;
			IsAttribute = floatParam.IsAttribute;
			UI = floatParam.UI;
		}
	}

	[Group( "Range" )] public float Min { get; set; }
	[Group( "Range" )] public float Max { get; set; }

	[Hide] public float Step => UI.Step;

	public FloatParameterNode()
	{
		Min = 0.0f;
		Max = 1.0f;
		UI = UI with { ShowStepProperty = true, ShowTypeProperty = true };
	}

	[Output( typeof( float ) ), Title( "Value" )]
	[Hide, NodeValueEditor( nameof( Value ) ), Range( nameof( Min ), nameof( Max ), nameof( Step ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		UI = UI with { ShowStepProperty = true, ShowTypeProperty = true };
		return compiler.ResultParameter( Name, Value, Min, Max, Min != Max, IsAttribute, UI );
	};
}

/// <summary>
/// 2 float values
/// </summary>
[Title( "Float2" ), Category( "Parameters" ), Icon( "looks_two" )]
[InternalNode]
public sealed class Float2ParameterNode : ParameterNodeBase<Vector2>
{
	[Hide]
	public override int Version => 2;

	public override void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is Float2Parameter float2Param )
		{
			Name = float2Param.Name;
			Value = float2Param.Value;
			Min = float2Param.Min;
			IsAttribute = float2Param.IsAttribute;
			Max = float2Param.Max;
			UI = float2Param.UI;
		}
	}

	[Group( "Range" )] public Vector2 Min { get; set; }
	[Group( "Range" )] public Vector2 Max { get; set; }

	public Float2ParameterNode()
	{
		Min = Vector2.Zero;
		Max = Vector2.One;
		UI = UI with { ShowStepProperty = true, ShowTypeProperty = true };
	}

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

	[Hide] public float MinX => Min.x;
	[Hide] public float MinY => Min.y;
	[Hide] public float MaxX => Max.x;
	[Hide] public float MaxY => Max.y;
	[Hide] public float Step => UI.Step;

	[Output( typeof( Vector2 ) ), Title( "XY" ), Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		UI = UI with { ShowStepProperty = true, ShowTypeProperty = true };
		return compiler.ResultParameter( Name, Value, Min, Max, Min != Max, IsAttribute, UI );
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
}

/// <summary>
/// 3 float values
/// </summary>
[Title( "Float3" ), Category( "Parameters" ), Icon( "looks_3" )]
[InternalNode]
public sealed class Float3ParameterNode : ParameterNodeBase<Vector3>
{
	[Hide]
	public override int Version => 2;

	public override void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is Float3Parameter float3Param )
		{
			Name = float3Param.Name;
			Value = float3Param.Value;
			Min = float3Param.Min;
			Max = float3Param.Max;
			IsAttribute = float3Param.IsAttribute;
			UI = float3Param.UI;
		}
	}

	[Group( "Range" )] public Vector3 Min { get; set; }
	[Group( "Range" )] public Vector3 Max { get; set; }

	public Float3ParameterNode()
	{
		Min = Vector3.Zero;
		Max = Vector3.One;
		UI = UI with { ShowStepProperty = true, ShowTypeProperty = true };
	}

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

	[Hide] public float MinX => Min.x;
	[Hide] public float MinY => Min.y;
	[Hide] public float MinZ => Min.z;
	[Hide] public float MaxX => Max.x;
	[Hide] public float MaxY => Max.y;
	[Hide] public float MaxZ => Max.z;
	[Hide] public float Step => UI.Step;

	[Output( typeof( Vector3 ) ), Title( "XYZ" ), Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		UI = UI with { ShowStepProperty = true, ShowTypeProperty = true };
		return compiler.ResultParameter( Name, Value, Min, Max, Min != Max, IsAttribute, UI );
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
}

/// <summary>
/// 4 float values
/// </summary>
[Title( "Float4" ), Category( "Parameters" ), Icon( "looks_4" )]
[InternalNode]
public sealed class Float4ParameterNode : ParameterNodeBase<Vector4>
{
	[Hide]
	public override int Version => 2;

	public override void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is Float4Parameter float4Param )
		{
			Name = float4Param.Name;
			Value = float4Param.Value;
			Min = float4Param.Min;
			Max = float4Param.Max;
			IsAttribute = float4Param.IsAttribute;
			UI = float4Param.UI;
		}
	}

	[Output( typeof( Vector4 ) ), Title( "XYZW" ), Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		UI = UI with { ShowStepProperty = true, ShowTypeProperty = true };
		return compiler.ResultParameter( Name, Value, Min, Max, Min != Max, IsAttribute, UI );
	};

	[Group( "Range" )] public Vector4 Min { get; set; }
	[Group( "Range" )] public Vector4 Max { get; set; }

	public Float4ParameterNode()
	{
		Min = Vector4.Zero;
		Max = Vector4.One;
		UI = UI with { ShowStepProperty = true, ShowTypeProperty = true };
	}

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

	[Hide] public float MinX => Min.x;
	[Hide] public float MinY => Min.y;
	[Hide] public float MinZ => Min.z;
	[Hide] public float MinW => Min.w;
	[Hide] public float MaxX => Max.x;
	[Hide] public float MaxY => Max.y;
	[Hide] public float MaxZ => Max.z;
	[Hide] public float MaxW => Max.w;
	[Hide] public float Step => UI.Step;

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

	/// <summary>
	/// W component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeValueEditor( nameof( ValueW ) ), Title( "W" )]
	[Range( nameof( MinW ), nameof( MaxW ), nameof( Step ) )]
	public NodeResult.Func W => ( GraphCompiler compiler ) => Component( "w", ValueW, compiler );
}

/// <summary>
/// 4 float values, Just like <see cref="Float4ParameterNode"/> but with a color control ui
/// </summary>
[Title( "Color" ), Category( "Parameters" ), Icon( "palette" )]
[InternalNode]
public sealed class ColorParameterNode : ParameterNodeBase<Color>
{
	[Hide]
	public override int Version => 2;

	public override void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is ColorParameter colorParam )
		{
			Name = colorParam.Name;
			Value = colorParam.Value;
			IsAttribute = colorParam.IsAttribute;
			UI = colorParam.UI;
		}
	}

	[Output( typeof( Color ) ), Title( "RGBA" )]
	[Hide, NodeValueEditor( nameof( Value ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		UI = UI with { ShowStepProperty = true, ShowTypeProperty = false };
		return compiler.ResultParameter( Name, Value, default, default, false, IsAttribute, UI );
	};

	public ColorParameterNode()
	{
		Value = Color.White;
		UI = new ParameterUI { Type = UIType.Color, ShowTypeProperty = false };
	}

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
}

/// <summary>
/// Texture 2D Input parameter.
/// </summary>
[Title( "Texture 2D" ), Category( "Parameters" ), Icon( "texture" )]
[InternalNode]
public sealed class Texture2DParameterNode : ShaderNodePlus, IBlackboardSyncableNode, ITextureParameterNodeNew, IMetaDataNode, IErroringNode
{
	[Hide]
	public override int Version => 2;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.ParameterNode;

	[Hide]
	public override bool CanPreview => false;

	[JsonIgnore, Hide, Browsable( false )]
	public override string Title => string.IsNullOrWhiteSpace( UI.Name ) ?
	$"{DisplayInfo.For( this ).Name}" :
	$"{UI.Name}";

	//[JsonIgnore, Hide, Browsable( false )]
	//public override string Subtitle => !string.IsNullOrWhiteSpace( UI.Name ) ? UI.Name : "";

	[Hide, Browsable( false )]
	public Guid BlackboardParameterIdentifier { get; set; }

	[Hide, Browsable( false )]
	public TextureInput UI { get; set; } = new TextureInput();

	[Output( typeof( Texture2DObject ) ), Title( "Texture" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		UI = UI with { Type = TextureType.Tex2D };
		var textureGlobal = compiler.ResultTexture( UI );
		var result = new NodeResult( ResultType.Texture2DObject, "TextureInput", UI );

		result.AddMetadataEntry( "TextureGlobal", textureGlobal );

		return result;
	};

	public void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is Texture2DParameter texture2dParam )
		{
			UI = texture2dParam.Value with { Name = texture2dParam.Name };
		}
	}

	public NodeResult GetResult( GraphCompiler compiler )
	{
		return Result.Invoke( compiler );
	}

	public List<string> GetErrors()
	{
		var errors = new List<string>();

		return errors;
	}
}

/// <summary>
/// Texture Cube Input parameter.
/// </summary>
[Title( "Texture Cube" ), Category( "Parameters" ), Icon( "view_in_ar" )]
[InternalNode]
public sealed class TextureCubeParameterNode : ShaderNodePlus, IBlackboardSyncableNode, ITextureParameterNodeNew, IMetaDataNode, IErroringNode
{
	[Hide]
	public override int Version => 2;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.ParameterNode;

	[Hide]
	public override bool CanPreview => false;

	[JsonIgnore, Hide, Browsable( false )]
	public override string Title => string.IsNullOrWhiteSpace( UI.Name ) ?
	$"{DisplayInfo.For( this ).Name}" :
	$"{UI.Name}";

	//[JsonIgnore, Hide, Browsable( false )]
	//public override string Subtitle => !string.IsNullOrWhiteSpace( UI.Name ) ? UI.Name : "";

	[Hide, Browsable( false )]
	public Guid BlackboardParameterIdentifier { get; set; }

	[Hide, Browsable( false )]
	public TextureInput UI { get; set; } = new TextureInput();

	[Output( typeof( Texture2DObject ) ), Title( "Texture" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		UI = UI with { Type = TextureType.TexCube };
		compiler.ResultTexture( UI );
		return new NodeResult( ResultType.TextureCubeObject, "TextureInput", UI );
	};

	public void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is TextureCubeParameter textureCubeParam )
		{
			UI = textureCubeParam.Value with { Name = textureCubeParam.Name };
		}
	}

	public NodeResult GetResult( GraphCompiler compiler )
	{
		return Result.Invoke( compiler );
	}

	public List<string> GetErrors()
	{
		var errors = new List<string>();

		return errors;
	}
}
