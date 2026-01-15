
HEADER
{
    Description = "";
}

FEATURES
{

    Feature(F_ALPHA_TEST, 0..1, "Translucent");
    Feature(F_TRANSLUCENT, 0..1, "Translucent");

    FeatureRule(Allow1(F_ALPHA_TEST, F_TRANSLUCENT), "Alpha test and Translucent are not compatible!");

    Feature(F_TEX_COORD_MANIPULATION, 0..1, "Texture Coordinates");

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
    // #ifndef S_ALPHA_TEST
    // #define S_ALPHA_TEST 0
    // #endif
    // #ifndef S_TRANSLUCENT
    // #define S_TRANSLUCENT 0
    // #endif

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

    StaticCombo(S_TRANSLUCENT, F_TRANSLUCENT, Sys(ALL));
    StaticCombo(S_ALPHA_TEST, F_ALPHA_TEST, Sys(ALL));
    StaticCombo(S_TEX_COORD_MANIPULATION, F_TEX_COORD_MANIPULATION, Sys(ALL));

    CreateInputTexture2D(TextureColor, Srgb, 8, "None", "_color", ",0/,0/0", DefaultFile("materials/dev/white_color.tga"));
    CreateInputTexture2D(TextureSelfIllumMask, Linear, 8, "None", "_selfillum", ",0/,0/0", DefaultFile("materials/dev/white_color.tga"));
    CreateInputTexture2D(TextureNormal, Linear, 8, "NormalizeNormals", "_normal", ",0/,0/0", DefaultFile("materials/dev/white_color.tga"));
    CreateInputTexture2D(TextureRoughness, Linear, 8, "None", "_rough", ",0/,0/0", DefaultFile("materials/dev/white_color.tga"));
    CreateInputTexture2D(TextureMetalness, Linear, 8, "None", "_metal", ",0/,0/0", DefaultFile("materials/dev/white_color.tga"));
    CreateInputTexture2D(TextureAmbientOcclusion, Linear, 8, "None", "_ao", ",0/,0/0", DefaultFile("materials/dev/white_color.tga"));

    Texture2D g_tColor < Channel(RGB, Box(TextureColor), Srgb);
    OutputFormat(BC7);
    SrgbRead(true);
    > ;
    Texture2D g_tSelfIllum < Channel(R, Box(TextureSelfIllumMask), Linear);
    OutputFormat(DXT5);
    SrgbRead(false);
    > ;
    Texture2D g_tNormal < Channel(RGB, Box(TextureNormal), Linear);
    OutputFormat(BC7);
    SrgbRead(false);
    > ;
    Texture2D g_tRoughness < Channel(R, Box(TextureRoughness), Linear);
    OutputFormat(DXT5);
    SrgbRead(false);
    > ;
    Texture2D g_tMetalness < Channel(R, Box(TextureMetalness), Linear);
    OutputFormat(DXT5);
    SrgbRead(false);
    > ;
    Texture2D g_tAmbientOcclusion < Channel(R, Box(TextureAmbientOcclusion), Linear);
    OutputFormat(DXT5);
    SrgbRead(false);
    > ;

#if (S_TRANSLUCENT || S_ALPHA_TEST)
    RenderState(BlendEnable, true);
    RenderState(AlphaToCoverageEnable, F_ALPHA_TEST);

    CreateInputTexture2D(TextureTranslucency, Linear, 8, "None", "_trans", ",0/,0/0", DefaultFile("materials/dev/white_color.tga"));
    Texture2D g_tTranslucency < Channel(R, Box(TextureTranslucency), Linear);
    OutputFormat(DXT1);
    SrgbRead(false);
    > ;
#endif

    // BoolAttribute(translucent, F_TRANSLUCENT);

    TextureAttribute(LightSim_DiffuseAlbedoTexture, g_tColor);
    TextureAttribute(RepresentativeTexture, g_tColor);

    float g_flSelfIllumBrightness < UiType(Slider);
    Default1(0);
    > ;

    float4 g_vColorTint < UiType(Color);
    Default4(1, 1, 1, 1);
    > ;

    float g_flRoughnessScaleFactor < UiType(Slider);
    Default1(1);
    > ;

    float g_flSelfIllumAlbedoFactor < UiType(Slider);
    Default1(0);
    > ;

    float g_flSelfIllumScale < Attribute("SelfIllumScale");
    Default1(0);
    > ;

    float4 g_vSelfIllumTint < UiType(Color);
    Default4(1, 1, 1, 1);
    > ;

    float2 g_vTexCoordOffset < Attribute("TexCoordOffset");
    Default2(0, 0);
    > ;

    float2 g_vTexCoordScale < Attribute("TexCoordScale");
    Default2(1, 1);
    > ;

    float ScaleRoughness(float roughness)
    {
        float scale = g_flRoughnessScaleFactor;

        if (scale == 0)
        {
            return 1;
        }

        roughness = 1 - roughness;

        float r2 = roughness * roughness;
        return sqrt(1 - saturate(r2 * scale));
    }

    float3 GetSelfIllumination(float selfIllumMask, float3 albedo)
    {
        float3 emission = selfIllumMask.xxx;

        emission *= SrgbGammaToLinear(g_vSelfIllumTint.xyz);
        emission *= g_flSelfIllumScale;
        emission *= pow(albedo, g_flSelfIllumAlbedoFactor);
        emission *= pow(2, g_flSelfIllumBrightness);

        return emission;
    }

    float4 MainPs(PixelInput i) : SV_Target0
    {
        Material m = Material::Init(i);

#if (S_TEX_COORD_MANIPULATION)
        float2 uvCoords = i.vTextureCoords.xy * g_vTexCoordScale + g_vTexCoordOffset;
#else
        float2 uvCoords = i.vTextureCoords.xy;
#endif

        float4 color = g_tColor.Sample(g_sAniso, uvCoords);
        float4 selfillum = g_tSelfIllum.Sample(g_sAniso, uvCoords);
        // float3 normal = DecodeNormal(NormalMap.Sample(g_sAniso, uv).rgb); // Decoding is necessary!
        float3 normalmap = DecodeNormal(g_tNormal.Sample(g_sAniso, uvCoords).xyz);
        float4 roughness = g_tRoughness.Sample(g_sAniso, uvCoords);
        float4 metalness = g_tMetalness.Sample(g_sAniso, uvCoords);
        float4 ao = g_tAmbientOcclusion.Sample(g_sAniso, uvCoords);
        float3 colorTint = SrgbGammaToLinear(g_vColorTint.xyz);

        float3 NormalTransformed = TransformNormal(normalmap, i.vNormalWs, i.vTangentUWs, i.vTangentVWs);

        m.Albedo = color.xyz * colorTint;
        m.Emission = GetSelfIllumination(selfillum.x, m.Albedo);
        m.Normal = NormalTransformed.xyz;
        // m.Normal.y = -m.Normal.y; // flip green channel for opengl style normal maps
        m.Roughness = ScaleRoughness(roughness.x);
        m.Metalness = metalness.x;
        m.AmbientOcclusion = ao.x;

        m.TintMask = 1;
        m.Transmission = 0;

#if (S_TRANSLUCENT || S_ALPHA_TEST)
        m.Opacity = g_tTranslucency.Sample(g_sAniso, uvCoords).r;
#endif

#if (S_ALPHA_TEST)

        float eps = 1.0f / 255.0f;
        // Clip first to try to kill the wave if we're in an area of all zero
        clip(m.Opacity - eps);

        m.Opacity = AdjustOpacityForAlphaToCoverage(m.Opacity, g_flAlphaTestReference, g_flAntiAliasedEdgeStrength, uvCoords);

        // m.Opacity = AlphaTest(m.Opacity, );
        // if (m.Opacity - 0.001 < g_flAlphaTestReference)
        // {
        //     discard;
        // }

        if (g_nMSAASampleCount == 1)
            OpaqueFadeDepth((m.Opacity + 0.5f + eps) * 0.5f, m.ScreenPosition.xy);
        else
            clip(m.Opacity - 0.000001); // Second clipping pass after alpha to coverage adjustment
#endif

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
