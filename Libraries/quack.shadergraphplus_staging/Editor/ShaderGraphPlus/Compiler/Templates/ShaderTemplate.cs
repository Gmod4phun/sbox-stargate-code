namespace ShaderGraphPlus;

public static class ShaderTemplate
{
	public static string Code => @"
HEADER
{{
    Description = ""{0}"";
}}

FEATURES
{{
	#include ""common/features.hlsl""
{1}
}}

MODES
{{
	Forward();
	Depth();
	ToolsShadingComplexity( ""tools_shading_complexity.shader"" );
}}

COMMON
{{
{2}
	#include ""common/shared.hlsl""
	#include ""common/gradient.hlsl""
	#include ""procedural.hlsl""

	#define S_UV2 1
}}

struct VertexInput
{{
	#include ""common/vertexinput.hlsl""
{3}
}};

struct PixelInput
{{
	#include ""common/pixelinput.hlsl""
{4}
}};

VS
{{
	#include ""common/vertex.hlsl""
{9}{10}{13}
	PixelInput MainVs( VertexInput v )
	{{
{8}
	}}
}}

PS
{{
	#include ""common/pixel.hlsl""
{5}{11}{12}
	float4 MainPs( PixelInput i ) : SV_Target0
	{{
{14}
{6}
{7}
{15}
	}}
}}
";



	// Included by common/vertexinput.hlsl
	internal static Dictionary<string, string> InternalVertexInputs => new()
	{
		//{ "vColor", "float4 vColor : COLOR0 < Semantic( Color ); >;" },
		{ "vTexCoord", "float2 vTexCoord : TEXCOORD0 < Semantic( LowPrecisionUv ); >;" },
		{ "vTexCoord2", "float2 vTexCoord2 : TEXCOORD1 < Semantic( LowPrecisionUv1 ); >;" },
		{ "vNormalOs", "float4 vNormalOs : NORMAL < Semantic( OptionallyCompressedTangentFrame ); >;" },
		{ "vTangentUOs_flTangentVSign", "float4 vTangentUOs_flTangentVSign : TANGENT < Semantic( TangentU_SignV ); >;" },
		{ "vBlendIndices", "uint4 vBlendIndices : BLENDINDICES < Semantic( BlendIndices ); >;" },
		{ "vBlendWeight", "float4 vBlendWeight : BLENDWEIGHT < Semantic( BlendWeight ); >;" },
		{ "flSSSCurvature", "float flSSSCurvature : TEXCOORD2 < Semantic( Curvature ); >;" },
		{ "nVertexIndex", "float nVertexIndex : TEXCOORD14 < Semantic( MorphIndex ); >;" },
		{ "nVertexCacheIndex", "float nVertexCacheIndex : TEXCOORD15 < Semantic( MorphIndex ); >;" },
		{ "nInstanceTransformID", "uint nInstanceTransformID : TEXCOORD13 < Semantic( InstanceTransformUv ); >;" },
		{ "vLightmapUV", "float2 vLightmapUV : TEXCOORD3 < Semantic( LightmapUV ); > ;" },
	};

	// Included by common/pixelinput.hlsl
	internal static Dictionary<string, string> InternalPixelInputs => new()
	{
		{ "vPositionWithOffsetWs", "float3 vPositionWithOffsetWs : TEXCOORD0;" },
		{ "vPositionWs", "float3 vPositionWs : TEXCOORD0;" },
		{ "vNormalWs", "float3 vNormalWs : TEXCOORD1;" },
		{ "vTextureCoords", "float4 vTextureCoords : TEXCOORD2;" },
		{ "vVertexColor", "float4 vVertexColor : TEXCOORD4;" },
		{ "vCentroidNormalWs", "centroid float3 vCentroidNormalWs : TEXCOORD5;" },
		{ "vTangentUWs", "float3 vTangentUWs : TEXCOORD6;" },
		{ "vTangentVWs", "float3 vTangentVWs : TEXCOORD7;" },
		{ "flSSSCurvature", "float flSSSCurvature : TEXCOORD11;" },
		{ "vLightmapUV", "centroid float2 vLightmapUV : TEXCOORD3;" },
		{ "vPositionPs", "float4 vPositionPs : SV_Position;" },
		{ "vPositionSs", "float4 vPositionSs : SV_Position;" },
		{ "face", "bool face : SV_IsFrontFace;" },
	};

	internal static Dictionary<string, string> VertexInputs => new()
	{
		{ "vColor", "float4 vColor : COLOR0 < Semantic( Color ); >;" },
	};

	internal static Dictionary<string, string> PixelInputs => new()
	{
		{ "vPositionOs", "float3 vPositionOs : TEXCOORD14;" },
		{ "vNormalOs", "float3 vNormalOs : TEXCOORD15;" },
		{ "vTangentUOs_flTangentVSign", "float4 vTangentUOs_flTangentVSign : TANGENT\t< Semantic( TangentU_SignV ); >;" },
		{ "vColor", "float4 vColor : COLOR0;" },
		{ "vTintColor", "float4 vTintColor : COLOR1;" },
		{ "vFrontFacing", "#if ( PROGRAM == VFX_PROGRAM_PS )\n\tbool vFrontFacing : SV_IsFrontFace;\n#endif" },
	};

	public static string Material_init => @"
Material m = Material::Init( i );
m.Albedo = float3( 1, 1, 1 );
m.Normal = float3( 0, 0, 1 );
m.Roughness = 1;
m.Metalness = 0;
m.AmbientOcclusion = 1;
m.TintMask = 1;
m.Opacity = 1;
m.Emission = float3( 0, 0, 0 );
m.Transmission = 0;";

	public static string Material_output => @"
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
		
return ShadingModelStandard::Shade( m );";


	public static string TextureDefinition => @"<!-- dmx encoding keyvalues2_noids 1 format vtex 1 -->
""CDmeVtex""
{{
    ""m_inputTextureArray"" ""element_array"" 
    [
        ""CDmeInputTexture""
        {{
            ""m_name"" ""string"" ""0""
            ""m_fileName"" ""string"" ""{0}""
            ""m_colorSpace"" ""string"" ""{1}""
            ""m_typeString"" ""string"" ""2D""
            ""m_imageProcessorArray"" ""element_array"" 
            [
                ""CDmeImageProcessor""
                {{
                    ""m_algorithm"" ""string"" ""{3}""
                    ""m_stringArg"" ""string"" """"
                    ""m_vFloat4Arg"" ""vector4"" ""0 0 0 0""
                }}
            ]
        }}
    ]
    ""m_outputTypeString"" ""string"" ""2D""
    ""m_outputFormat"" ""string"" ""{2}""
    ""m_textureOutputChannelArray"" ""element_array""
    [
        ""CDmeTextureOutputChannel""
        {{
            ""m_inputTextureArray"" ""string_array""
            [
                ""0""
            ]
            ""m_srcChannels"" ""string"" ""rgba""
            ""m_dstChannels"" ""string"" ""rgba""
            ""m_mipAlgorithm"" ""CDmeImageProcessor""
            {{
                ""m_algorithm"" ""string"" ""Box""
                ""m_stringArg"" ""string"" """"
                ""m_vFloat4Arg"" ""vector4"" ""0 0 0 0""
            }}
            ""m_outputColorSpace"" ""string"" ""{1}""
        }}
    ]
}}";
}
