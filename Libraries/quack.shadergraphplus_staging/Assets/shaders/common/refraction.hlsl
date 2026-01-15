#ifndef COMMON_REFRACTION_H
#define COMMON_REFRACTION_H

BoolAttribute( bWantsFBCopyTexture, true );
BoolAttribute( translucent, true );

CreateTexture2D( g_tFrameBufferCopyTexture ) < Attribute( "FrameBufferCopyTexture" ); SrgbRead( true ); Filter( MIN_MAG_MIP_LINEAR ); AddressU( CLAMP ); AddressV( CLAMP ); >;

CreateInputTexture2D( Normal, Linear, 8, "NormalizeNormals", "_normal", "Refraction,1/0",  Default3( 1.00, 1.00, 1.00) );
CreateTexture2DWithoutSampler( g_tNormal ) < Channel( RGB, Box( Normal ), Linear ); OutputFormat( DXT5 ); SrgbRead( false ); >;


struct RefractionInput
{
	float2 vPositionUv;
	float3 vPositionWs;
	float3 vNormal;
	float3 vViewRayWs;

	float3 vRefractionRayWs;
	float3 vRefractionPosWs;
	float2 vRefractionUv;

	float fDepthSample;
	float fDepthSampleRefraction;

	float fRayDistance;
	float fVerticalDistance;
};


//
// Public Refraction API
//
class Refraction
{

    static float3 GetRefraction( RefractionInput i )
	{

		//
		// Disable refraction if material wants so
		//
		if( !g_bRefraction )
			return float3(0, 0, 0);

		//
		// Lerp between normal ray and refraction ray in UV space based on seamless entry
		// so that view looks seamless when transitioning to water
		//
		const float2 vPositionUv = i.vRefractionUv;//lerp( i.vPositionUv, i.vRefractionUv, i.vSeamlessEntry.x );

		return Tex2D( g_tFrameBufferCopyTexture, vPositionUv * g_vFrameBufferCopyInvSizeAndUvScale.zw ).rgb;
	}

    static float FetchDepth( PixelInput i, float2 vPositionUv )
	{
		// Return dummy depth if we don't have refraction enabled
		if( !g_bRefraction )
			return distance( g_vCameraPositionWs, i.vPositionWithOffsetWs.xyz );
		else
			return Depth::Get( vPositionUv * g_vViewportSize ).r;
	}

    // -------------------------------------------------------------------------------------------------------------------------------------------------------------

	//
	// Calculates the refracted angle in world space using the water index of refraction
	//
	static float3 GetRefractionRay( RefractionInput input )
	{
		const float fIndexOfRefraction = 1.05f;
		return normalize( refract( input.vViewRayWs, input.vNormal, 1.0f/fIndexOfRefraction ) ) * float3( 1, 1, -1 );
	}

	//
	// Transforms the refracted ray from world space to UV space
	//
	static float2 GetRefractionUVFromRay( RefractionInput input )
	{
		float flRefractScale = sqrt( input.fRayDistance * 2.0f );
		flRefractScale = min( flRefractScale, 512.0f );

		float3 vRefractionPos = input.vPositionWs + input.vRefractionRayWs * flRefractScale;

		// Convert our world space refracted position back into screen space
		float4 vPositionRefractPs = Position3WsToPs( vRefractionPos );
		vPositionRefractPs.xyz /= vPositionRefractPs.w;
		float2 vPositionRefractSs = PsToSs( vPositionRefractPs );

		vPositionRefractSs.x = 1.0 - vPositionRefractSs.x;
		return vPositionRefractSs;
	}

	// -------------------------------------------------------------------------------------------------------------------------------------------------------------

    static float3 CalculateWorldSpacePosition( float3 vViewRayWs, float fDepth )
	{
		return g_vCameraPositionWs + vViewRayWs * fDepth;
	}

	static RefractionInput SetupRefractionInput( PixelInput i )
	{
		RefractionInput input;

		input.vPositionUv = CalculateViewportUvFromInvSize( i.vPositionSs.xy - g_vViewportOffset.xy, g_vInvViewportSize.xy );
		input.vPositionWs = i.vPositionWithOffsetWs.xyz;
		input.vViewRayWs = CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs );

		const float fSurfaceNdotV = dot( float3(0,0,1), input.vViewRayWs );

		input.vNormal = Tex2DS(g_tNormal, TextureFiltering, i.vTextureCoords.xy).xyz;

		input.fDepthSample = FetchDepth( i, input.vPositionUv );

		input.vRefractionRayWs = GetRefractionRay( input );
		input.vRefractionPosWs = CalculateWorldSpacePosition( input.vViewRayWs, input.fDepthSample );

		input.fRayDistance = length( input.vPositionWs - input.vRefractionPosWs ) ;

		input.vRefractionUv = GetRefractionUVFromRay( input );

		input.fDepthSampleRefraction = FetchDepth( i, input.vRefractionUv );

		// Realign refraction ray
		float fdepth2 = FetchDepth( i, input.vRefractionUv );
		input.vRefractionPosWs = CalculateWorldSpacePosition( input.vRefractionRayWs, fdepth2 );
		//input.fRayDistance = length( input.vPositionWs - input.vRefractionPosWs ) ;

		input.fVerticalDistance =  input.vRefractionPosWs.z;

		return input;
	}

};


#endif // COMMON_REFRACTION_H