float2 curveRemapUV(float2 vScreenUV, float2 vCurvature)
{
    // as we near the edge of our screen apply greater distortion using a cubic function
    vScreenUV = vScreenUV * 2.0 - 1.0;
    float2 offset = abs(vScreenUV.yx) / float2(vCurvature.x, vCurvature.y);
    vScreenUV = vScreenUV + vScreenUV * offset * offset;
    vScreenUV = vScreenUV * 0.5 + 0.5;
    return vScreenUV;
}