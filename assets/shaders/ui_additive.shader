HEADER
{
	DevShader = true;
	Version = 1;
}

MODES
{
	Default();
	VrForward();
}

// FEATURES
// {
	
// }

COMMON
{
	#define BLEND_MODE_ALREADY_SET

	#include "ui/common.hlsl"
}
  
VS
{
	#include "ui/vertex.hlsl"
}

PS
{
	#include "common.fxc"
	#include "math_general.fxc"
	#include "ui/scissor.hlsl"

	// Defines ------------------------------------------------------------------------------------------------------------------------------------------------

	#define SUBPIXEL_AA_MAGIC 0.5

	// Attributes ---------------------------------------------------------------------------------------------------------------------------------------------
	float2 BoxSize < Attribute( "BoxSize" ); >;
	float2 BoxPosition < Attribute( "BoxPosition" ); >;

	// Render State -------------------------------------------------------------------------------------------------------------------------------------------

	// Already set by shared code
	// RenderState( DepthEnable, false );

	// Main ---------------------------------------------------------------------------------------------------------------------------------------------------
	struct PS_OUTPUT
	{
		float4 vColor : SV_Target0;
	};

	void UI_CommonProcessing_Pre( PS_INPUT i )
	{
		if ( HasScissoring )
		{
			SoftwareScissoring( i );
		}
	}

	PS_OUTPUT UI_CommonProcessing_Post( PS_INPUT i, PS_OUTPUT o )
	{
		return o;
	}

	float4 g_vViewport < Source( Viewport ); >;
	float4 g_vInvTextureDim < Source( InvTextureDim ); SourceArg( g_tColor ); >;
	CreateTexture2D( g_tColor ) < Attribute( "Texture" ); SrgbRead( true ); Filter( ANISOTROPIC ); >;

	RenderState( SrgbWriteEnable0, true );
	RenderState( ColorWriteEnable0, RGBA );
	RenderState( FillMode, SOLID );
	RenderState( CullMode, NONE );
	RenderState( DepthWriteEnable, false );

	// Additive
	RenderState( BlendEnable, true );
	RenderState( SrcBlend, SRC_ALPHA);
	RenderState( DstBlend, ONE);
	// RenderState( BlendOp, ADD);

	// RenderState( SrcBlendAlpha, SRC_ALPHA);
	// RenderState( DstBlendAlpha, ONE);
	// RenderState( BlendOpAlpha, ADD);

	// RenderState( BlendFactor, true);

	PS_OUTPUT MainPs( PS_INPUT i )
	{
		PS_OUTPUT o;
		UI_CommonProcessing_Pre( i );

		float4 vImage = Tex2D( g_tColor, i.vTexCoord.xy );
		o.vColor = vImage * i.vColor.rgba;
		return UI_CommonProcessing_Post( i, o );
	}
}
