
HEADER
{
	Description = "";
}

FEATURES
{
	#include "common/features.hlsl"
}

MODES
{
	Forward();
	Depth( S_MODE_DEPTH );
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

COMMON
{
	#ifndef S_ALPHA_TEST
	#define S_ALPHA_TEST 0
	#endif
	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT 0
	#endif
	
	#include "common/shared.hlsl"
	#include "procedural.hlsl"

	#define S_UV2 1
	#define CUSTOM_MATERIAL_INPUTS
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
	float4 vColor : COLOR0 < Semantic( Color ); >;
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
	float3 vPositionOs : TEXCOORD14;
	float3 vNormalOs : TEXCOORD15;
	float4 vTangentUOs_flTangentVSign : TANGENT	< Semantic( TangentU_SignV ); >;
	float4 vColor : COLOR0;
	float4 vTintColor : COLOR1;
	#if ( PROGRAM == VFX_PROGRAM_PS )
		bool vFrontFacing : SV_IsFrontFace;
	#endif
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;
		i.vColor = v.vColor;
		
		ExtraShaderData_t extraShaderData = GetExtraPerInstanceShaderData( v );
		i.vTintColor = extraShaderData.vTint;
		
		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );
		return FinalizeVertex( i );
		
	}
}

PS
{
	#include "common/pixel.hlsl"
	
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( Color, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tColor < Channel( RGBA, Box( Color ), Srgb ); OutputFormat( BC7 ); SrgbRead( True ); >;
	TextureAttribute( LightSim_DiffuseAlbedoTexture, g_tColor )
	TextureAttribute( RepresentativeTexture, g_tColor )
	bool g_bGrayscale < UiGroup( ",0/,0/0" ); Default( 0 ); >;
	float g_flInMin < UiGroup( ",0/,0/0" ); Default1( 0 ); Range1( 0, 1 ); >;
	float g_flInMax < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 1 ); >;
	float g_flOutMin < UiGroup( ",0/,0/0" ); Default1( 0 ); Range1( 0, 1 ); >;
	float g_flOutMax < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 1 ); >;
	float4 g_vSaturation < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 1.00, 1.00, 1.00, 1.00 ); >;
	float g_flIllumbrightness < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 1, 10 ); >;
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		
		Material m = Material::Init();
		m.Albedo = float3( 1, 1, 1 );
		m.Normal = float3( 0, 0, 1 );
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 0;
		
		float2 l_0 = i.vTextureCoords.xy * float2( 1, 1 );
		float l_1 = g_flTime * 1;
		float l_2 = VoronoiNoise( i.vTextureCoords.xy, l_1, 15 );
		float l_3 = saturate( ( l_2 - 0 ) / ( 1 - 0 ) ) * ( 0.525 - 0.5 ) + 0.5;
		float2 l_4 = l_0 + float2( l_3, l_3 );
		float2 l_5 = l_4 + float2( 0.49, 0.49 );
		float4 l_6 = Tex2DS( g_tColor, g_sSampler0, l_5 );
		float l_7 = l_6.x;
		float l_8 = l_7 * 0.299;
		float l_9 = l_6.y;
		float l_10 = l_9 * 0.587;
		float l_11 = l_8 + l_10;
		float l_12 = l_6.z;
		float l_13 = l_12 * 0.114;
		float l_14 = l_11 + l_13;
		float4 l_15 = float4( l_14, l_14, l_14, 0 );
		float4 l_16 = g_bGrayscale ? l_15 : l_6;
		float l_17 = l_16.x;
		float l_18 = l_17 * 0.299;
		float l_19 = l_16.y;
		float l_20 = l_19 * 0.587;
		float l_21 = l_18 + l_20;
		float l_22 = l_16.z;
		float l_23 = l_22 * 0.114;
		float l_24 = l_21 + l_23;
		float4 l_25 = float4( l_24, l_24, l_24, 0 );
		float l_26 = g_flInMin;
		float l_27 = g_flInMax;
		float l_28 = g_flOutMin;
		float l_29 = g_flOutMax;
		float4 l_30 = saturate( ( l_25 - float4( l_26, l_26, l_26, l_26 ) ) / ( float4( l_27, l_27, l_27, l_27 ) - float4( l_26, l_26, l_26, l_26 ) ) ) * ( float4( l_29, l_29, l_29, l_29 ) - float4( l_28, l_28, l_28, l_28 ) ) + float4( l_28, l_28, l_28, l_28 );
		float l_31 = l_30.x;
		float l_32 = l_31 * 1;
		float l_33 = l_30.y;
		float l_34 = l_33 * 1;
		float l_35 = l_32 + l_34;
		float l_36 = l_30.z;
		float l_37 = l_36 * 4.6;
		float l_38 = l_35 + l_37;
		float4 l_39 = float4( l_38, l_38, l_38, 0 );
		float4 l_40 = l_16 * l_39;
		float l_41 = l_40.x;
		float4 l_42 = g_vSaturation;
		float l_43 = l_41 * l_42.r;
		float l_44 = l_40.y;
		float l_45 = l_44 * l_42.g;
		float l_46 = l_40.z;
		float l_47 = l_46 * l_42.b;
		float4 l_48 = float4( l_43, l_45, l_47, 0 );
		float4 l_49 = float4( 0.5241, 0.8493, 1, 1 );
		float l_50 = g_flIllumbrightness;
		float l_51 = saturate( ( l_50 - 1 ) / ( 10 - 1 ) ) * ( 2 - 0 ) + 0;
		float4 l_52 = l_49 * float4( l_51, l_51, l_51, l_51 );
		float4 l_53 = l_48 + l_52;
		float4 l_54 = saturate( l_53 );
		float4 l_55 = l_54 / float4( 2, 2, 2, 2 );
		
		m.Albedo = l_54.xyz;
		m.Emission = l_55.xyz;
		m.Opacity = 0.5034327;
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		
		
		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );
		
		// Result node takes normal as tangent space, convert it to world space now
		m.Normal = TransformNormal( m.Normal, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
		
		// for some toolvis shit
		m.WorldTangentU = i.vTangentUWs;
		m.WorldTangentV = i.vTangentVWs;
		m.TextureCoords = i.vTextureCoords.xy;
				
		return ShadingModelStandard::Shade( i, m );
	}
}
