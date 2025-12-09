namespace ShaderGraphPlus;

// TODO : Probably dont need sperate templates anymore. So remove this file if thats the case.
public static class ShaderTemplateBlending
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
	float4 vColorBlendValues : TEXCOORD4 < Semantic( VertexPaintBlendParams ); >;
	float4 vColorPaintValues : TEXCOORD5 < Semantic( VertexPaintTintColor ); >;
	float4 vColor : COLOR0 < Semantic( Color ); >;
	#include ""common/vertexinput.hlsl""
{3}
}};

struct PixelInput
{{
	float4 vColor : COLOR0;
	float4 vBlendValues		 : TEXCOORD14;
	float4 vPaintValues		 : TEXCOORD15;
	#include ""common/pixelinput.hlsl""
{4}
}};

VS
{{
	StaticCombo( S_MULTIBLEND, F_MULTIBLEND, Sys( PC ) );

	#include ""common/vertex.hlsl""

	BoolAttribute( VertexPaintUI2Layer, F_MULTIBLEND == 1 );
	BoolAttribute( VertexPaintUI3Layer, F_MULTIBLEND == 2 );
	BoolAttribute( VertexPaintUI4Layer, F_MULTIBLEND == 3 );
	BoolAttribute( VertexPaintUI5Layer, F_MULTIBLEND == 4 );
	BoolAttribute( VertexPaintUIPickColor, true );

{9}{10}{13}
	PixelInput MainVs( VertexInput v )
	{{
{8}
	}}
}}

PS
{{
	StaticCombo( S_MULTIBLEND, F_MULTIBLEND, Sys( PC ) );

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

}
