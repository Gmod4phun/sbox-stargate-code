HEADER
{
    Description = "Lens Flare Sun Occluder Shader";
}

FEATURES
{
    #include "common/features.hlsl"
}

MODES
{
    Forward();
    Depth();
}

COMMON
{
    #include "common/shared.hlsl"
}

struct VertexInput
{
    #include "common/vertexinput.hlsl"
};

struct PixelInput
{
    #include "common/pixelinput.hlsl"
};

VS
{
    #include "common/vertex.hlsl"

    PixelInput MainVs( VertexInput i )
    {
        PixelInput o = ProcessVertex( i );
        return FinalizeVertex( o );
    }
}

PS
{
    RenderState( CullMode, NONE );
    RenderState( FillMode, SOLID );
    RenderState( DepthWriteEnable, false );
    RenderState( DepthEnable, true );
    RenderState( DepthFunc, LESS_EQUAL );
    
    float4 MainPs( PixelInput i ) : SV_Target0
    {
        return float4( float3(1,0,0), 1 );
    }
}
