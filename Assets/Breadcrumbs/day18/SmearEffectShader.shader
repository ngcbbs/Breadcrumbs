Shader "Custom/SmearEffectShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {} // 기본 텍스처
        _SmearAmount ("Smear Amount", Range(0, 5)) = 0.02 // 잔상 강도
        _SmearDirection ("Smear Direction", Vector) = (0,0,0,0) // Smear 방향 (월드 기준)
        _Rotation ("Rotation", Float) = 0.0
        _PreviousRotation ("Previous Rotation", Float) = 0.0
        _BlurStrength ("Blur Strength", Float) = 0.5
        _StretchAmount ("Stretch Amount", Float) = 0.5
        _SmearAlpha ("Smear Alpha", Range(0,1)) = 0.5 // 잔상의 투명도
        _BlurSamples ("Blur Samples", Range(1, 10)) = 5 // 블러 샘플 개수
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha // 알파 블렌딩
        Cull Off
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 smearOffset : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _SmearAmount;
            float4 _SmearDirection;
            float _Rotation;
            float _SmearAlpha;
            int _BlurSamples;

            float _BlurStrength;
            float _StretchAmount;

            float _PreviousRotation;

            v2f vert (appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);

                // Smear 방향으로 UV 변형
                float2 smearOffset = _SmearDirection.xy * _SmearAmount;
                OUT.smearOffset = smearOffset;
                
                OUT.uv = IN.uv;
                return OUT;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                float2 uv = IN.uv;
                float2 centeredUV = uv - float2(0.5f, 0.5f);

                // 현재 프레임의 회전 각도와 이전 프레임의 회전 각도 차이 계산
                float rotationDifference = _Rotation - _PreviousRotation;

                // 회전 방향 결정
                float rotationDirection = sign(rotationDifference);

                // 잔상 효과 계산
                float2 rotatedUV = centeredUV;
                float4 blurredColor = tex2D(_MainTex, uv);
                float distance = length(uv - centeredUV);
                for (int j = 1; j <= 5; j++) {
                    float rotationAmount = rotationDifference * (j / 5.0) * _BlurStrength;
                    float cosTheta = cos(radians(rotationAmount));
                    float sinTheta = sin(radians(rotationAmount));
                    float2x2 rotationMatrix = float2x2(cosTheta, -sinTheta, sinTheta, cosTheta);
                    rotatedUV = mul(rotationMatrix, centeredUV);

                    // 늘어나는 효과 계산
                    float stretchFactor = 1.0;
                    if (rotationDirection > 0) { // 반대 방향으로 회전하는 경우에만 늘어남
                        float distance = length(centeredUV);
                        stretchFactor = 1.0 + _StretchAmount * (1.0 - distance); // 중심으로 갈수록 효과 감소
                    }
                    float2 stretchedUV = rotatedUV * stretchFactor * distance;

                    blurredColor += tex2D(_MainTex, stretchedUV + float2(0.5f, 0.5f));
                }
                blurredColor /= 6.0;

                // 이전 프레임의 회전 각도 업데이트
                _PreviousRotation = _Rotation;

                return blurredColor;
            }
            ENDCG
        }
    }
}
