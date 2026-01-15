
namespace ShaderGraphPlus;


public enum FillMode
{
	Wireframe,
	Solid,
}

public enum CullMode
{
	None,
	Back,
	Front,
}

public enum DepthFunc
{
	Never,
	Less,
	Equal,
	LessEqual,
	Greater,
	NotEqual,
	GreaterEqual,
	Always,
}

public enum StencilFailOp
{
	Keep,
	Zero,
	Replace,
	Incr_sat,
	Decr_sat,
	Invert,
	Incr,
	Decr,
}

public enum StencilDepthFailOp
{
	Keep,
	Zero,
	Replace,
	Incr_sat,
	Decr_sat,
	Invert,
	Incr,
	Decr,
}

public enum StencilPassOp
{
	Keep,
	Zero,
	Replace,
	Incr_sat,
	Decr_sat,
	Invert,
	Incr,
	Decr,
}

public enum StencilFunc
{
	Never,
	Less,
	Equal,
	LessEqual,
	Greater,
	NotEqual,
	GreaterEqual,
	Always,
}

public enum BackStencilFailOp
{
	Keep,
	Zero,
	Replace,
	Incr_sat,
	Decr_sat,
	Invert,
	Incr,
	Decr,
}

public enum BackStencilDepthFailOp
{
	Keep,
	Zero,
	Replace,
	Incr_sat,
	Decr_sat,
	Invert,
	Incr,
	Decr,
}

public enum BackStencilPassOp
{
	Keep,
	Zero,
	Replace,
	Incr_sat,
	Decr_sat,
	Invert,
	Incr,
	Decr,
}

public enum BackStencilFunc
{
	Keep,
	Zero,
	Replace,
	Incr_sat,
	Decr_sat,
	Invert,
	Incr,
	Decr,
}

public enum SrcBlend
{
	Zero,
	One,
	Src_Color,
	Inv_Src_Color,
	Src_Alpha,
	Inv_Src_Alpha,
	Dest_Alpha,
	Inv_Dest_Alpha,
	Dest_Color,
	Inv_Dest_Color,
	SRC_Alpha_SAT,
	Blend_Factor,
	Src1_Color,
	Inv_SRC1_Color,
	Src1_Alpha,
	Inv_Src1_Alpha
}

public enum DstBlend
{
	Zero,
	One,
	Src_Color,
	Inv_Src_Color,
	Src_Alpha,
	Inv_Src_Alpha,
	Dest_Alpha,
	Inv_Dest_Alpha,
	Dest_Color,
	Inv_Dest_Color,
	SRC_Alpha_SAT,
	Blend_Factor,
	Src1_Color,
	Inv_SRC1_Color,
	Src1_Alpha,
	Inv_Src1_Alpha
}

public enum BlendOp
{
	Add,
	Subtract,
	RevSubtract,
	Min,
	Max
}

public enum BlendOpAlpha
{
	Add,
	Subtract,
	RevSubtract,
	Min,
	Max
}

public enum SrcBlendAlpha
{
	Zero,
	One,
	Src_Color,
	Inv_Src_Color,
	Src_Alpha,
	Inv_Src_Alpha,
	Dest_Alpha,
	Inv_Dest_Alpha,
	Dest_Color,
	Inv_Dest_Color,
	SRC_Alpha_SAT,
	Blend_Factor,
	Src1_Color,
	Inv_SRC1_Color,
	Src1_Alpha,
	Inv_Src1_Alpha
}

public enum DstBlendAlpha
{
	Zero,
	One,
	Src_Color,
	Inv_Src_Color,
	Src_Alpha,
	Inv_Src_Alpha,
	Dest_Alpha,
	Inv_Dest_Alpha,
	Dest_Color,
	Inv_Dest_Color,
	SRC_Alpha_SAT,
	Blend_Factor,
	Src1_Color,
	Inv_SRC1_Color,
	Src1_Alpha,
	Inv_Src1_Alpha
}

public enum AlphaTestFunc
{
	Never,
	Less,
	Equal,
	LessEqual,
	Greater,
	NotEqual,
	GreaterEqual,
	Always,
}

[AttributeUsage( AttributeTargets.Property )]
public class RenderStateAttribute : Attribute
{
	public string Name;
	public bool Hidden;

	public RenderStateAttribute( string valueName )
	{
		Name = valueName;
	}
}




//
// TODO : Im really not sure about the RenderState class below. There should probabaly be a better way to only set
// what you want rather than having every thing show up at once.

/// <summary>
/// Most of the avalible shader render states. Initially set to their defaults ( I think ) and if one is set to the default it should be compiled out of the shader.
/// </summary>
public class RenderStates
{
	[Group( "Config" )] public bool SetBlendOp { get; set; } = false;
	[Group( "Config" )] public bool SetBlendOpAlpha { get; set; } = false;
	[Group( "Config" )] public bool SetSrcBlendAlpha { get; set; } = false;
	[Group( "Config" )] public bool SetDstBlendAlpha { get; set; } = false;
	[Group( "Config" )] public bool SetAlphaTestFunc { get; set; } = false;

	[RenderState( nameof( FillMode ) )]
	public FillMode FillMode { get; set; } = FillMode.Solid;

	[RenderState( nameof( CullMode ) )]
	public CullMode CullMode { get; set; } = CullMode.Back;

	[Hide]
	public DepthFunc DepthFunc { get; set; } = DepthFunc.Less;

	[RenderState( nameof( StencilEnable ) )]
	public bool StencilEnable { get; set; } = true;
	[Range( 0, 255 )]

	[RenderState( nameof( StencilReadMask ) )]
	public int StencilReadMask { get; set; } = 1;
	[Range( 0, 255 )]

	[RenderState( nameof( StencilWriteMask ) )]
	public int StencilWriteMask { get; set; } = 254;

	[RenderState( nameof( StencilFailOp ) )]
	public StencilFailOp StencilFailOp { get; set; } = StencilFailOp.Keep;

	[RenderState( nameof( StencilDepthFailOp ) )]
	public StencilDepthFailOp StencilDepthFailOp { get; set; } = StencilDepthFailOp.Keep;

	[RenderState( nameof( StencilPassOp ) )]
	public StencilPassOp StencilPassOp { get; set; } = StencilPassOp.Keep;

	[RenderState( nameof( StencilFunc ) )]
	public StencilFunc StencilFunc { get; set; } = StencilFunc.NotEqual;

	[RenderState( nameof( BackStencilFailOp ) )]
	public BackStencilFailOp BackStencilFailOp { get; set; } = BackStencilFailOp.Keep;

	[RenderState( nameof( BackStencilDepthFailOp ) )]
	public BackStencilDepthFailOp BackStencilDepthFailOp { get; set; } = BackStencilDepthFailOp.Keep;

	[RenderState( nameof( BackStencilPassOp ) )]
	public BackStencilPassOp BackStencilPassOp { get; set; } = BackStencilPassOp.Keep;

	[RenderState( nameof( BackStencilFunc ) )]
	public BackStencilFunc BackStencilFunc { get; set; } = BackStencilFunc.Keep;

	[Range( 0, 255 )]
	[RenderState( nameof( StencilRef ) )]
	public int StencilRef { get; set; } = 3;

	[RenderState( nameof( AlphaToCoverageEnable ) )]
	public bool AlphaToCoverageEnable { get; set; } = false;

	[RenderState( nameof( BlendEnable ) )]
	public bool BlendEnable { get; set; } = false;

	[RenderState( nameof( IndependentBlendEnable ) )]
	public bool IndependentBlendEnable { get; set; } = false;

	[RenderState( nameof( SrcBlend ) )]
	public SrcBlend SrcBlend { get; set; } = SrcBlend.Src_Alpha;

	[RenderState( nameof( DstBlend ) )]
	public DstBlend DstBlend { get; set; } = DstBlend.Inv_Src_Alpha;

	[HideIf( nameof( SetBlendOp ), false )]
	[RenderState( nameof( BlendOp ) )]
	public BlendOp BlendOp { get; set; }

	[HideIf( nameof( SetBlendOpAlpha ), false )]
	[RenderState( nameof( BlendOpAlpha ) )]
	public BlendOpAlpha BlendOpAlpha { get; set; }

	[HideIf( nameof( SetSrcBlendAlpha ), false )]
	[RenderState( nameof( SrcBlendAlpha ) )]
	public SrcBlendAlpha SrcBlendAlpha { get; set; }

	[HideIf( nameof( SetDstBlendAlpha ), false )]
	[RenderState( nameof( DstBlendAlpha ) )]
	public DstBlendAlpha DstBlendAlpha { get; set; }

	[HideIf( nameof( SetAlphaTestFunc ), false )]
	[RenderState( nameof( AlphaTestFunc ) )]
	public AlphaTestFunc AlphaTestFunc { get; set; }

	public bool Check<T>( T obj, string PropertyName, string Value )
	{

		var defaultRS = new RenderStates();

		var Properties = defaultRS.GetType().GetProperties()
			   .Where( p => (p.PropertyType.IsEnum || p.PropertyType == typeof( int ) || p.PropertyType == typeof( bool )) );

		foreach ( var Property in Properties )
		{
			var objValue = Property.GetValue( defaultRS )?.ToString();

			if ( PropertyName == Property.Name )
			{
				if ( objValue == Value )
				{
					Log.Info( $" {PropertyName} input value : {Value} == {Property.Name} default value : {objValue}" );
					return true;
				}
			}

		}

		return false;
	}

}
