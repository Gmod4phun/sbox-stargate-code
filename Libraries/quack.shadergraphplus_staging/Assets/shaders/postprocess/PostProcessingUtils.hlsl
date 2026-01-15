#ifndef COMMON_POSTPROCESS_UTILS_H
#define COMMON_POSTPROCESS_UTILS_H


#include "postprocess/UVUtils.hlsl"

// Used by the noise functin to generate a pseudo random value between 0.0 and 1.0
float2 random(float2 uv){
    uv = float2( dot(uv, float2(127.1,311.7) ),
               dot(uv, float2(269.5,183.3) ) );
    return -1.0 + 2.0 * frac(sin(uv) * 43758.5453123);
}

// Generate a Perlin noise used by the distortion effects
float TestNoise(float2 uv) {
    float2 uv_index = floor(uv);
    float2 uv_fract = frac(uv);

    float2 blur = smoothstep(0.0, 1.0, uv_fract);

    return lerp( lerp( dot( random(uv_index + float2(0.0,0.0) ), uv_fract - float2(0.0,0.0) ),
                     dot( random(uv_index + float2(1.0,0.0) ), uv_fract - float2(1.0,0.0) ), blur.x),
                lerp( dot( random(uv_index + float2(0.0,1.0) ), uv_fract - float2(0.0,1.0) ),
                     dot( random(uv_index + float2(1.0,1.0) ), uv_fract - float2(1.0,1.0) ), blur.x), blur.y) * 0.5 + 0.5;
}

// Takes in the UV and warps the edges, creating the spherized effect
float2 warp(float2 uv , float warp_amount){
	float2 delta = uv - 0.5;
	float delta2 = dot(delta.xy, delta.xy);
	float delta4 = delta2 * delta2;
	float delta_offset = delta4 * warp_amount;
	
	return uv + delta * delta_offset;
}

// Adds a black border to hide stretched pixel created by the warp effect
float border (float2 uv, float warp_amount){
	float radius = min(warp_amount, 0.08);
	radius = max(min(min(abs(radius * 2.0), abs(1.0)), abs(1.0)), 1e-5);
	float2 abs_uv = abs(uv * 2.0 - 1.0) - float2(1.0, 1.0) + radius;
	float dist = length(max(float2(0.0,0.0), abs_uv)) / radius;
	float square = smoothstep(0.96, 1.0, dist);
	return clamp(1.0 - square, 0.0, 1.0);
}

// Adds a vignette shadow to the edges of the image
float vignette(float2 uv, float vignette_intensity, float vignette_opacity){
	uv *= 1.0 - uv.xy;
	float vignette = uv.x * uv.y * 15.0;
	return pow(vignette, vignette_intensity * vignette_opacity);
}

#endif // COMMON_POSTPROCESS_UTILS_H