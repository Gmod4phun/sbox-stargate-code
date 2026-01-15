
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
    ToolsShadingComplexity("tools_shading_complexity.shader");
}

COMMON
{

#include "common/shared.hlsl"
#include "procedural.hlsl"

#define S_UV2 1
}

struct VertexInput
{
#include "common/vertexinput.hlsl"
    float4 vColor : COLOR0 < Semantic(Color);
    > ;
};

struct PixelInput
{
#include "common/pixelinput.hlsl"
    float3 vPositionOs : TEXCOORD14;
    float3 vNormalOs : TEXCOORD15;
    float4 vTangentUOs_flTangentVSign : TANGENT < Semantic(TangentU_SignV);
    > ;
    float4 vColor : COLOR0;
    float4 vTintColor : COLOR1;
#if (PROGRAM == VFX_PROGRAM_PS)
    bool vFrontFacing : SV_IsFrontFace;
#endif
};

VS
{
#include "common/vertex.hlsl"

    PixelInput MainVs(VertexInput v)
    {

        PixelInput i = ProcessVertex(v);
        i.vPositionOs = v.vPositionOs.xyz;
        i.vColor = v.vColor;

        ExtraShaderData_t extraShaderData = GetExtraPerInstanceShaderData(v.nInstanceTransformID);
        i.vTintColor = extraShaderData.vTint;

        VS_DecodeObjectSpaceNormalAndTangent(v, i.vNormalOs, i.vTangentUOs_flTangentVSign);
        return FinalizeVertex(i);
    }
}

PS
{

// #define CUSTOM_MATERIAL_INPUTS
#include "common/pixel.hlsl"
    CreateInputTexture2D(TextureColor, Srgb, 8, "None", "_color", ",0/,0/0", DefaultFile("materials/dev/white_color.tga"));

    Texture2D g_tColor < Channel(RGB, Box(TextureColor), Srgb);
    OutputFormat(DXT1);
    SrgbRead(true);
    > ;


    RenderState(BlendEnable, true);

    CreateInputTexture2D(TextureTranslucency, Linear, 8, "None", "_trans", ",0/,0/0", DefaultFile("materials/dev/white_color.tga"));
    Texture2D g_tTranslucency < Channel(R, Box(TextureTranslucency), Linear);
    OutputFormat(DXT1);
    SrgbRead(false);
    > ;

    int g_flFrame < Attribute("Frame");
    Default(0);
    > ;

    void ApplyTextCoordAdjustments(float2 scale, inout float2 offset)
    {
        int frameNumber = g_flFrame;
        frameNumber = frameNumber % 19;

        offset.x = frameNumber % 8 * scale.x;
        offset.y = floor(frameNumber / 8.0) * scale.y;
    }

    float4 MainPs(PixelInput i) : SV_Target0
    {
        Material m = Material::Init(i);

        float2 texCoordScale = float2(1.0/8, 1.0/4);
        float2 texCoordOffset = float2(0, 0);

        ApplyTextCoordAdjustments(texCoordScale, texCoordOffset);

        float2 uvCoords = i.vTextureCoords.xy * texCoordScale + texCoordOffset;

        float4 color = g_tColor.Sample(g_sAniso, uvCoords);

        m.Albedo = color.xyz;
        m.Emission = m.Albedo;
        m.Roughness = 1;
        m.Metalness = 0;
        // m.TintMask = 1;
        m.Transmission = 0;
        m.Opacity = g_tTranslucency.Sample(g_sAniso, uvCoords).r;

        m.AmbientOcclusion = saturate(m.AmbientOcclusion);
        m.Roughness = saturate(m.Roughness);
        m.Metalness = saturate(m.Metalness);
        m.Opacity = saturate(m.Opacity);

        // Result node takes normal as tangent space, convert it to world space now
        m.Normal = TransformNormal(m.Normal, i.vNormalWs, i.vTangentUWs, i.vTangentVWs);

        // for some toolvis shit
        m.WorldTangentU = i.vTangentUWs;
        m.WorldTangentV = i.vTangentVWs;
        m.TextureCoords = uvCoords;

        return ShadingModelStandard::Shade(m);
    }
}
