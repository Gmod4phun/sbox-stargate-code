HEADER
{
	DevShader = true;
	Description = "";
}

MODES
{
	Default();
}

COMMON
{
	#include "system.fxc" // This should always be the first include in COMMON
	#include "common/shared.hlsl"
	#include "procedural.hlsl"
}

CS
{
	#include "common.fxc"

	RWTexture2D<float4> g_tOutputTexture < Attribute( "RWOutputTexture" ); >;

	float4 g_vGridColor < Attribute( "TestColor" ); Default4( 1, 1, 1, 1 );>;
	float2 g_vTextureSize < Attribute( "TextureSize" ); Default2( 1024, 1024 ); >;

	float2 GetTextureCoords( uint2 vThreadID )
	{
		// Texel centers are at a half pixel offset. Correct that by a lovely 0.5f offset in the x & y of the ThreadID.
		return ( ( (float2)vThreadID + 0.5f ) / g_vTextureSize );
	}

	[numthreads(8, 8, 1)]
	void MainCs( uint3 vThreadId : SV_DispatchThreadID )
	{
		float2 vTextureCoords = GetTextureCoords( vThreadId.xy );

		g_tOutputTexture[vThreadId.xy] = float4(vTextureCoords.x, vTextureCoords.y, 0.0f, 1.0f );//lerp( dotColor, BLACK, 0.1f * distance( vTextureCoords.xy, float2( 0.5f, 0.5f) ) );
	}
}