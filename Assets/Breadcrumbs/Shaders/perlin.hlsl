#include "library.hlsl"
void perlin_half(half2 p, half time, half scale, half2 offset, out half _out) {
    _out = 0;
    _out = perlin(p, time, scale, offset);
}
void perlin_float(float2 p, float time, float scale, float2 offset, out float _out) {
    _out = 0;
    _out = perlin(p, time, scale, offset);
}
void colorBurn_half(half3 baseColor, half3 blendColor, out half3 _out) {
    _out = saturate(1.0 - (1.0 - baseColor) / blendColor);
}
void colorBurn_float(float3 baseColor, float3 blendColor, out float3 _out) {
    _out = saturate(1.0 - (1.0 - baseColor) / blendColor);
}