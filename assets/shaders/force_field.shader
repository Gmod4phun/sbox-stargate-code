HEADER
{
	Description = "Simple force field shader with object intersection";
}

MODES
{
	Forward();
	Depth( S_MODE_DEPTH );
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

FEATURES
{
    #include "common/features.hlsl"
}

COMMON
{
    #ifndef S_ALPHA_TEST
	#define S_ALPHA_TEST 0
	#endif
	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT 1
	#endif
    #define S_RENDER_BACKFACES 1
	#include "common/shared.hlsl"
    #define CUSTOM_MATERIAL_INPUTS
}

struct VertexInput
{
    float4 vColorBlendValues : Color0 < Semantic( Color ); >;
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
    float4 vBlendValues		 : TEXCOORD14;
	float3 v3                : TEXCOORD15; //terrain specific
	float3 v4                : TEXCOORD16; //terrain specific
    float3 v5                : TEXCOORD17; //terrain specific

    float3 vPositionOs : TEXCOORD18;
	float3 vNormalOs : TEXCOORD19;
	float4 vTangentUOs_flTangentVSign : TANGENT	< Semantic( TangentU_SignV ); >;

	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"
    #define CUSTOM_TEXTURE_FILTERING
    #define cmp -

//vs_samplers
//vs_CBuffers
//vs_Inputs

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );
        // float4 r0,r1,r2;
        o.vBlendValues = i.vColorBlendValues;
		o.vBlendValues.a = i.vColorBlendValues.a;
        o.vPositionOs = i.vPositionOs.xyz;
        VS_DecodeObjectSpaceNormalAndTangent( i, o.vNormalOs, o.vTangentUOs_flTangentVSign );

//vs_Function

		return FinalizeVertex( o );
	}
}

PS
{
    #include "common/pixel.hlsl"
    #define CUSTOM_TEXTURE_FILTERING
    #define cmp -

	RenderState(AlphaToCoverageEnable, false)
	RenderState(IndependentBlendEnable, true)
	RenderState(BlendEnable, true)
	RenderState(SrcBlend, ONE)
	RenderState(DstBlend, INV_SRC_ALPHA)
	RenderState(BlendOp, ADD)
	RenderState(SrcBlendAlpha, ONE)
	RenderState(DstBlendAlpha, INV_SRC_ALPHA)
	RenderState(BlendOpAlpha, ADD)

	// SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D(TextureTintMask, 				Linear, 	8, "", "_mask",	"Basic,10/11",	Default3(1.0, 1.0, 1.0));
	CreateTexture2DWithoutSampler(g_tTintMask) 			< Channel(R,	Box(TextureTintMask),	Linear); OutputFormat(ATI1N); SrgbRead(false); > ;
	// Texture2D g_tTintMask < Channel( RGBA, Box( TextureTintMask ), Srgb ); OutputFormat( ATI1N ); SrgbRead( false ); >;

	float3 g_vShieldColor < Attribute( "shieldColor" ); Default3( 1.00, 1.00, 1.00 ); >;
	float g_flIntersectionSharpness < UiGroup( ",0/,0/0" ); Default1( 0.2 ); Range1( 0.01, 1 ); >;
	float g_flBorderDistanceFromSphereCenter < UiGroup( ",0/,0/0" ); Default1( 3 ); Range1( 0.01, 5 ); >;
	float g_flBubbleAlphaMul < UiGroup( ",0/,0/0" ); Default1( 0.1 ); Range1( 0, 10 ); >;
	float g_flMasterAlphaMul < Attribute( "masterAlphaMul" ); Default1( 1 ); >;

    float4 MainPs( PixelInput i ) : SV_Target0
    {
		float2 f2FinalTexCoord = i.vTextureCoords.xy;

		float3 pos = i.vPositionWithOffsetWs.xyz;
		float depth = Depth::GetNormalized(i.vPositionSs);
		float notSure = -abs(dot(pos, -g_vCameraDirWs)) + (1 / depth);

		float clampedDepth = saturate(notSure * g_flIntersectionSharpness * 0.1);
		
		float something = pow(dot(pos * rsqrt(dot(pos, pos)), i.vNormalWs), 2);
		float clampedSomething = saturate(something);

		float varDiff = something - clampedSomething;
		float sampleArg = abs((varDiff * clampedDepth + clampedSomething) * clampedDepth);
		float sample2 = abs((varDiff * clampedDepth + clampedSomething) * clampedDepth - 0.02);

		float alphaMul = 0.05;
		float alpha = alphaMul / sampleArg - alphaMul;
		alpha = alpha + (alphaMul / sample2 - alphaMul);

		alpha = abs(alpha);

		float time = g_flTime;;

		float2 offset1 = float2(time * 0.0725, time * 0.04);
		float f1TintMask = g_tTintMask.Sample( g_sAniso, f2FinalTexCoord + offset1 ).x;

		float2 offset2 = float2(time * -0.0725, time * -0.04);
		float f2TintMask = g_tTintMask.Sample( g_sAniso, f2FinalTexCoord + offset2 ).x;

		alpha = alpha * (f1TintMask + f2TintMask);

		float3 outVar = g_vShieldColor * alpha;

		outVar = outVar * g_flMasterAlphaMul;

		outVar = saturate(outVar);

		return float4(outVar, 0);
    }
}
