
#ifndef SIMPLEX3D_H
#define SIMPLEX3D_H

#define SIMPLEX_MOD_3D(x, m) ((x) - floor((x) * (1.0f / (m))) * (m))
#define SIMPLEX_PERMUTE_3D(x) SIMPLEX_MOD_3D(((x) * 34.0f + 1.0f) * (x), 289.0f)

float Simplex3DInternal(float3 v)
{
    const float2 C = float2(1.0f / 6.0f, 1.0f / 3.0f);
    const float4 D = float4(0.0f, 0.5f, 1.0f, 2.0f);

    // First corner
    float3 i = floor(v + dot(v, C.yyy));
    float3 x0 = v - i + dot(i, C.xxx);

    // Other corners
    float3 g = step(x0.yzx, x0.xyz);
    float3 l = 1.0f - g;
    float3 i1 = min(g.xyz, l.zxy);
    float3 i2 = max(g.xyz, l.zxy);

    float3 x1 = x0 - i1 + C.xxx;
    float3 x2 = x0 - i2 + C.yyy;
    float3 x3 = x0 - D.yyy;

    // Permutations
    i = SIMPLEX_MOD_3D(i, 289.0f);
    float4 p = SIMPLEX_PERMUTE_3D(
        SIMPLEX_PERMUTE_3D(
            SIMPLEX_PERMUTE_3D(
                i.z + float4(0.0f, i1.z, i2.z, 1.0f)) +
            i.y + float4(0.0f, i1.y, i2.y, 1.0f)) +
        i.x + float4(0.0f, i1.x, i2.x, 1.0f));

    // Gradients: 7x7 points over a square, mapped onto an octahedron
    float n_ = 0.142857142857f; // 1.0/7.0
    float3 ns = n_ * D.wyz - D.xzx;

    float4 j = p - 49.0f * floor(p * ns.z * ns.z);

    float4 x_ = floor(j * ns.z);
    float4 y_ = floor(j - 7.0f * x_);

    float4 x = x_ * ns.x + ns.yyyy;
    float4 y = y_ * ns.x + ns.yyyy;
    float4 h = 1.0f - abs(x) - abs(y);

    float4 b0 = float4(x.xy, y.xy);
    float4 b1 = float4(x.zw, y.zw);

    float4 s0 = floor(b0) * 2.0f + 1.0f;
    float4 s1 = floor(b1) * 2.0f + 1.0f;
    float4 sh = -step(h, float4(0, 0, 0, 0));

    float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
    float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;

    float3 p0 = float3(a0.xy, h.x);
    float3 p1 = float3(a0.zw, h.y);
    float3 p2 = float3(a1.xy, h.z);
    float3 p3 = float3(a1.zw, h.w);

    // Normalise gradients
    float4 norm = rsqrt(float4(
        dot(p0, p0),
        dot(p1, p1),
        dot(p2, p2),
        dot(p3, p3)));
    p0 *= norm.x;
    p1 *= norm.y;
    p2 *= norm.z;
    p3 *= norm.w;

    // Mix final noise value
    float4 m = max(0.6f - float4(
                              dot(x0, x0),
                              dot(x1, x1),
                              dot(x2, x2),
                              dot(x3, x3)),
                   0.0f);
    m = m * m;

    float result = 42.0f * dot(m * m, float4(
                                          dot(p0, x0),
                                          dot(p1, x1),
                                          dot(p2, x2),
                                          dot(p3, x3)));

    // remap result from -1..1 to 0..1
    return result * 0.5f + 0.5f;
}

// Time-evolved 2D simplex using 3D simplex
void Simplex3D(float2 input, float time, out float output)
{
    output = Simplex3DInternal(float3(input, time));
}

#undef SIMPLEX_MOD_3D
#undef SIMPLEX_PERMUTE_3D

#endif // SIMPLEX3D_H
