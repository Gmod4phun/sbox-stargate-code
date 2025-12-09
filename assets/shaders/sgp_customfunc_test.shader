
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
	Depth();
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
	#include "common/gradient.hlsl"
	#include "procedural.hlsl"

	#define S_UV2 1
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
		
		ExtraShaderData_t extraShaderData = GetExtraPerInstanceShaderData( v.nInstanceTransformID );
		i.vTintColor = extraShaderData.vTint;
		
		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );
				
		return FinalizeVertex( i );
		
	}
}

PS
{
	#include "common/pixel.hlsl"
	
	SamplerState g_sSampler0 < Filter( BILINEAR ); AddressU( WRAP ); AddressV( WRAP ); AddressW( WRAP ); MaxAniso( 8 ); >;
	SamplerState g_sSampler1 < Filter( BILINEAR ); AddressU( WRAP ); AddressV( WRAP ); AddressW( WRAP ); MaxAniso( 8 ); >;
	CreateInputTexture2D( WaterNormal, Linear, 8, "None", "_color", ",0/,0/0", DefaultFile( "materials/sbox_stargate/kawoosh/waternormal0001.tga" ) );
	Texture2D g_tWaterNormal < Channel( RGBA, Box( WaterNormal ), Linear ); OutputFormat( BC7 ); SrgbRead( False ); >;
		
	
	RenderState( CullMode, F_RENDER_BACKFACES ? NONE : DEFAULT );
		
	float ElipseShape( float2 UV, float Width, float Height )
	{
		float d = length( ( UV * 2 - 1) / float2( Width, Height ) );
		return saturate( ( 1 - d ) / fwidth( d ) );
	}
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		
		Material m = Material::Init( i );
		m.Albedo = float3( 1, 1, 1 );
		m.Normal = float3( 0, 0, 1 );
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 0;
		
		float l_0 = ElipseShape( i.vTextureCoords.xy, 2, 2 );
		float4 l_1 = float4( 0.00175, 0.01713, 0.04186, 1 );
		float4 l_2 = float4( l_0, l_0, l_0, l_0 ) * l_1;
		float3 l_3 = float3( 1, 1, 1 );
		float l_4 = g_flTime * 0.113;
		float2 l_5 = float2( 0.1423712, 0.2291892 );
		float2 l_6 = float2( l_4, l_4 ) * l_5;
		float2 l_7 = TileAndOffsetUv( i.vTextureCoords.xy, float2( 1, 1 ), l_6 );
		float4 l_8 = g_tWaterNormal.Sample( g_sSampler0,l_7 );
		float l_9 = g_flTime * 0.12;
		float2 l_10 = float2( -0.1604054, -0.07358748 );
		float2 l_11 = float2( l_9, l_9 ) * l_10;
		float2 l_12 = TileAndOffsetUv( i.vTextureCoords.xy, float2( 1.19, 1.13 ), l_11 );
		float4 l_13 = g_tWaterNormal.Sample( g_sSampler1,l_12 );
		float l_14 = l_8.r + l_13.r;
		float l_15 = l_8.g + l_13.g;
		float l_16 = l_8.b * l_13.b;
		float3 l_17 = float3( l_14, l_15, l_16 );
		float3 l_18 = lerp( l_3, l_17, 1 );
		float3 l_19 = DecodeNormal( l_18 );
		float3 l_20 = normalize( l_19 );
		
		m.Albedo = l_2.xyz;
		m.Opacity = 1;
		m.Normal = l_20;
		m.Roughness = 0;
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
				
		return ShadingModelStandard::Shade( m );
	}
}
