Shader "Hidden/RetroCRT"
{
    HLSLINCLUDE
    
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        float4 _TintColor;
        float _Intensity;

        float random(float2 uv)
        {
            return frac(sin(dot(uv.xy, float2(12.9898, 78.233))) * 43758.5453);
        }
        
        float4 RetroCRT(Varyings input) : SV_Target
        {
            float2 uv = input.texcoord;
            float _ScanlineIntensity = 0.5f;
            float _Aberration = 2.5f;
            float _NoiseIntensity = 0.05f;
            
            // 원본 텍스처에서 색상 샘플링
            float3 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord).rgb;
            
            // 색수차 효과
            float2 aberrationOffset = _Aberration * float2(0.001, 0.0);
            float r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + aberrationOffset).r;
            float g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).g;
            float b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv - aberrationOffset).b;
            col = float3(r, g, b);

            // 스캔 라인 효과
            float scanline = sin(uv.y * _BlitTexture_TexelSize.y * 2000 * _ScreenParams.y) * 0.5 + 0.5;
            col *= lerp(1.0, scanline, _ScanlineIntensity);

            // 노이즈 효과
            float noise = (random(uv + _Time.y) - 0.5) * _NoiseIntensity;
            col += noise;

            col = lerp(col, col * _TintColor, _Intensity);
            return float4(col, 1);
        }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "RetroCRT - Pass"
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment RetroCRT
            ENDHLSL
        }
    }
}