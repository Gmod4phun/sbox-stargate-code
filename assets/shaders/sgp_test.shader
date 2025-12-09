
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
	#define S_TRANSLUCENT 1
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
	
	BoolAttribute( bWantsFBCopyTexture, true );
	Texture2D g_tFrameBufferCopyTexture < Attribute( "FrameBufferCopyTexture"); SrgbRead( false ); >;
	SamplerState g_sSampler0 < Filter( BILINEAR ); AddressU( WRAP ); AddressV( WRAP ); AddressW( WRAP ); MaxAniso( 8 ); >;
	CreateInputTexture2D( Noise, Srgb, 8, "None", "_color", ",0/,0/0", DefaultFile( "textures/noise_01.psd" ) );
	Texture2D g_tNoise < Channel( RGBA, Box( Noise ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	bool g_bBackside < UiGroup( ",0/,0/0" ); Default( 0 ); >;
	float g_flRefractStrength < UiType( Slider ); UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 1 ); >;
	float g_flRefractEdgeMargin < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 0.1 ); >;
	float g_flEstablished < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 1 ); >;
	float g_flMasterOpacity < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 1 ); >;
	float g_flNormalStrength < UiType( Slider ); UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 2 ); >;
		
	#include "procedural/simplex3d.hlsl"
	
	RenderState( CullMode, F_RENDER_BACKFACES ? NONE : DEFAULT );
		
	float RoundGradient( float2 vUV, float2 flCenter, float flRadius, float flDensity, bool bInvert )
	{
		float flDistance = length( vUV - flCenter );
		float flResult = pow( saturate( flDistance / flRadius ), flDensity );
	
		if( bInvert )
		{
			return 1 - flResult;
		}
	
		return flResult;
	}
	
	float3 Height2Normal( float flHeight , float flStrength, float3 vPosition, float3 vNormal )
	{
	    float3 worldDerivativeX = ddx(vPosition);
	    float3 worldDerivativeY = ddy(vPosition);
	
	    float3 crossX = cross(vNormal, worldDerivativeX);
	    float3 crossY = cross(worldDerivativeY, vNormal);
	
	    float d = dot(worldDerivativeX, crossY);
	
	    float sgn = d < 0.0 ? (-1.f) : 1.f;
	    float surface = sgn / max(0.00000000000001192093f, abs(d));
	
	    float dHdx = ddx(flHeight);
	    float dHdy = ddy(flHeight);
	
	    float3 surfGrad = surface * (dHdx*crossY + dHdy*crossX);
	
	    return normalize(vNormal - (flStrength * surfGrad));
	}
	
	float2 MapSceneColorCoords( float2 vInput, float2 modes )
	{
		float2 result;
	
		// X
		if ( modes.x == 1 ) // Mirror
		{
			float xx = abs( vInput.x );
			result.x = (fmod( floor( xx ), 2.0 ) == 0.0) ? frac( xx ) : 1.0 - frac( xx );
		}
		else if ( modes.x == 2 ) // Clamp
		{
			result.x = clamp( vInput.x, 0.0, 1.0 );
		}
		else if ( modes.x == 3 ) // Border
		{
			result.x = (vInput.x < 0.0 || vInput.x > 1.0) ? 0.5 : vInput.x;
		}
		else if ( modes.x == 4 ) // MirrorOnce
		{
			float xx = abs( vInput.x );
			float floorX = floor( xx );
			if ( floorX < 1.0 )
			{
				result.x = frac( xx );
			}
			else if ( floorX < 2.0 )
			{
				result.x = 1.0 - frac( xx );
			}
			else
			{
				result.x = vInput.x;
			}
		}
		else // Wrap by default
		{
			result.x = vInput.x;
		}
	
		// Y
		if ( modes.y == 1 ) // Mirror
		{
			float yy = abs( vInput.y );
			result.y = (fmod( floor( yy ), 2.0 ) == 0.0) ? frac( yy ) : 1.0 - frac( yy );
		}
		else if ( modes.y == 2 ) // Clamp
		{
			result.y = clamp( vInput.y, 0.0, 1.0 );
		}
		else if ( modes.y == 3 ) // Border
		{
			result.y = (vInput.y < 0.0 || vInput.y > 1.0) ? 0.5 : vInput.y;
		}
		else if ( modes.y == 4 ) // MirrorOnce
		{
			float yy = abs( vInput.y );
			float floorY = floor( yy );
			if ( floorY < 1.0 )
			{
				result.y = frac( yy );
			}
			else if ( floorY < 2.0 )
			{
				result.y = 1.0 - frac( yy );
			}
			else
			{
				result.y = vInput.y;
			}
		}
		else // Wrap by default
		{
			result.y = vInput.y;
		}
	
		return result;
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
		
		Gradient Gradient0 = Gradient::Init();
		
		Gradient0.colorsLength = 3;
		Gradient0.alphasLength = 0;
		Gradient0.colors[0] = float4( 0.0114, 0.04902, 0.14419, 0.16666667 );
		Gradient0.colors[1] = float4( 0.01785, 0.04573, 0.11628, 0.89 );
		Gradient0.colors[2] = float4( 0, 0, 0, 1 );
		
		bool l_0 = g_bBackside;
		float l_1 = l_0 ? 0.4 : 0;
		float2 l_2 = CalculateViewportUv( i.vPositionSs.xy );
		float2 l_3 = TileAndOffsetUv( i.vTextureCoords.xy, float2( 8, 8 ), float2( 0, 0 ) );
		float l_4 = g_flTime * 1.2;
		
		float ol_0 = 0.0f;
		Simplex3D( l_3, l_4,ol_0 );
		
		float l_6 = (saturate( ( ol_0 - 0 ) / ( 1 - 0 ) ) * ( 1 - 0 )) + 0;
		float l_7 = RoundGradient( i.vTextureCoords.xy, float2( 0.5, 0.5 ), 0.5, 1, true );
		float l_8 = l_6 * l_7;
		float l_9 = g_flRefractStrength;
		float l_10 = g_flRefractEdgeMargin;
		float l_11 = 0.5 - l_10;
		float l_12 = RoundGradient( i.vTextureCoords.xy, float2( 0.5, 0.5 ), l_11, 8, true );
		float l_13 = l_9 * l_12;
		float3 l_14 = Vec3WsToTs( Height2Normal( l_8, l_13, i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz, i.vNormalWs ), i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
		float3 l_15 = float3( l_2, 0 ) + l_14;
		float3 l_16 = g_tFrameBufferCopyTexture.Sample( g_sAniso, MapSceneColorCoords( l_15.xy, float2(1,1) )).rgb;
		float3 l_17 = float3( l_1, l_1, l_1 ) * l_16;
		float2 l_18 = TileAndOffsetUv( i.vTextureCoords.xy, float2( 1, 1 ), float2( 0, 0 ) );
		float2 l_19 = PolarCoordinates( ( l_18 ) - ( float2( 0.5, 0.5 ) ), 1, 1 );
		float4 l_20 = Gradient::SampleGradient( Gradient0, l_19 );
		float2 l_21 = TileAndOffsetUv( i.vTextureCoords.xy, float2( 12, 20 ), float2( 0, 0 ) );
		float l_22 = g_flTime * 0.5;
		
		float ol_1 = 0.0f;
		Simplex3D( l_21, l_22,ol_1 );
		
		float l_24 = (saturate( ( ol_1 - 0.72 ) / ( 0.79999995 - 0.72 ) ) * ( 0.12000022 - 0 )) + 0;
		float l_25 = RoundGradient( i.vTextureCoords.xy, float2( 0.5, 0.5 ), 0.35, 0.7, true );
		float l_26 = l_24 * l_25;
		float l_27 = l_26 * 4;
		float l_28 = g_flTime * 1.5;
		
		float ol_2 = 0.0f;
		Simplex3D( l_21, l_28,ol_2 );
		
		float l_30 = (saturate( ( ol_2 - 0.030000009 ) / ( 0.6499991 - 0.030000009 ) ) * ( 0.34000003 - 0.08000004 )) + 0.08000004;
		float l_31 = RoundGradient( i.vTextureCoords.xy, float2( 0.5, 0.5 ), 0.40999997, 0.109999985, true );
		float l_32 = l_30 * l_31;
		float l_33 = l_27 + l_32;
		float l_34 = g_flTime * 0.6;
		
		float ol_3 = 0.0f;
		Simplex3D( l_21, l_34,ol_3 );
		
		float l_36 = (saturate( ( ol_3 - 0.55000013 ) / ( 1.9499991 - 0.55000013 ) ) * ( 3.1599991 - 0 )) + 0;
		float l_37 = RoundGradient( i.vTextureCoords.xy, float2( 0.5, 0.5 ), 0.15, 0.750001, true );
		float l_38 = l_36 * l_37;
		float l_39 = l_33 + l_38;
		float4 l_40 = l_20 + float4( l_39, l_39, l_39, l_39 );
		float4 l_41 = float4( l_17, 0 ) + l_40;
		float4 l_42 = l_41 * float4( 2, 2, 2, 2 );
		float2 l_43 = TileAndOffsetUv( i.vTextureCoords.xy, float2( 4, 4 ), float2( 0, 0 ) );
		float4 l_44 = g_tNoise.Sample( g_sSampler0,l_43 );
		float l_45 = (saturate( ( l_44.b - 0 ) / ( 1 - 0 ) ) * ( 8.49 - 0 )) + 0;
		float l_46 = g_flEstablished;
		float l_47 = 1 - l_46;
		float l_48 = RoundGradient( i.vTextureCoords.xy, float2( 0.5, 0.5 ), l_47, 16, false );
		float l_49 = l_45 * l_48;
		float l_50 = RoundGradient( i.vTextureCoords.xy, float2( 0.5, 0.5 ), l_47, 50, false );
		float l_51 = l_49 + l_50;
		float l_52 = clamp( l_51, 0, 1 );
		float l_53 = g_flMasterOpacity;
		float l_54 = l_52 * l_53;
		float l_55 = g_flNormalStrength;
		float3 l_56 = Height2Normal( l_8, l_55, i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz, i.vNormalWs );
		
		m.Albedo = l_41.xyz;
		m.Emission = l_42.xyz;
		m.Opacity = l_54;
		m.Normal = l_56;
		m.Roughness = 0;
		m.Metalness = 0;
		m.AmbientOcclusion = 0;
		
		
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
