#ifndef COMMON_UNLIT_PIXEL_SHADING_H
#define COMMON_UNLIT_PIXEL_SHADING_H

#include "common/material.hlsl"
//#include "common/GBuffer.hlsl"
//#include "common/ToolVis.hlsl"

class ShadingModelUnlit
{
    //
    // Converts our Material struct to a FinalCombinerInput_t
    // PS_InitFinalCombiner assumes that you want to control the "optional" parameters yourself.
    // These *need* to be set up by you if you want correct lighting.
    //
    // PS_FinalCombiner should be called at the end and works on the FinalCombinerInput_t data only. 
    // This does lighting, tonemapping, etc. in a standardized way.
    //
    static CombinerInput MaterialToCombinerInput( Material m )
    {
        CombinerInput o = PS_InitFinalCombiner();
        o.vEmissive = m.Emission;
        return o;    
    }

    static float4 Shade( Material m )
    {
        CombinerInput combinerInput = MaterialToCombinerInput( m );

        float3 vDiffuse = combinerInput.vDiffuseColor.rgb + combinerInput.vEmissive.rgb;
        float4 color = float4( vDiffuse  , combinerInput.flOpacity );
        
        return color;
    }

#ifdef COMMON_PS_INPUT_DEFINED
    static CombinerInput MaterialToCombinerInput( PixelInput i, Material m )
    {
        CombinerInput o = PS_InitFinalCombiner();

        o = PS_CommonProcessing( i );
        
        // this should not be here
        #if ( S_ALPHA_TEST )
        {
            // Clip first to try to kill the wave if we're in an area of all zero
            o.flOpacity = m.Opacity * o.flOpacity;
            clip( o.flOpacity - .001 );

            o.flOpacity = AdjustOpacityForAlphaToCoverage( o.flOpacity, g_flAlphaTestReference, g_flAntiAliasedEdgeStrength, i.vTextureCoords.xy );
            clip( o.flOpacity - 0.001 );
        }
        #elif ( S_TRANSLUCENT )
        {
            o.flOpacity *= m.Opacity * g_flOpacityScale;
        }
        #else
            o.flOpacity *= m.Opacity;
        #endif

        o = CalculateDiffuseAndSpecularFromAlbedoAndMetalness( o, m.Albedo.rgb, m.Metalness );

        o.vEmissive = m.Emission;

        return o;
    }

    static float4 Shade( PixelInput i, Material m )
    {
        CombinerInput combinerInput = MaterialToCombinerInput( i, m );

        float3 diffuse =  combinerInput.vDiffuseColor.rgb + combinerInput.vEmissive.rgb;
        float4 color = float4( diffuse , combinerInput.flOpacity );

        return color;
    }
#endif
};

DynamicCombo( D_BAKED_LIGHTING_FROM_PROBE, 0..1, Sys( ALL ) );
DynamicCombo( D_BAKED_LIGHTING_FROM_LIGHTMAP, 0..1, Sys( ALL ) );
DynamicComboRule( Allow1( D_BAKED_LIGHTING_FROM_PROBE, D_BAKED_LIGHTING_FROM_LIGHTMAP ) );


#endif // COMMON_UNLIT_PIXEL_SHADING_H