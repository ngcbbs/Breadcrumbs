float2 random2(float2 p) {
    p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
    return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
}

float perlin(float2 p, float time, float scale, float2 offset) {
    p = (p + offset) * scale + time; // 스케일, 오프셋, 시간 적용
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);

    return lerp(lerp(dot(random2(i + float2(0.0, 0.0)), f - float2(0.0, 0.0)),
                        dot(random2(i + float2(1.0, 0.0)), f - float2(1.0, 0.0)), u.x),
                lerp(dot(random2(i + float2(0.0, 1.0)), f - float2(0.0, 1.0)),
                        dot(random2(i + float2(1.0, 1.0)), f - float2(1.0, 1.0)), u.x), u.y);
}