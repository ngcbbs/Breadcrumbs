Shader "Unlit/test"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

            	float2 resolution = float2(1,1);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);float2 p2 = (2.0*i.uv.xy-resolution.xy)/resolution.y;
                float tau = 3.1415926535*2.0;
                float a = atan2(p2.y, p2.x);
                float r = length(p2)*0.75;
                float2 uv = float2(a/tau,r);
	            
	            //get the color
	            float xCol = (uv.x - (_Time.z / 3.0)) * 3.0;
	            xCol = xCol / 3.0;
	            float3 horColour = float3(0.25, 0.25, 0.25);
	            
	            if (xCol < 1.0) {
		            
		            horColour.r += 1.0 - xCol;
		            horColour.g += xCol;
	            }
	            else if (xCol < 2.0) {
		            
		            xCol -= 1.0;
		            horColour.g += 1.0 - xCol;
		            horColour.b += xCol;
	            }
	            else {
		            
		            xCol -= 2.0;
		            horColour.b += 1.0 - xCol;
		            horColour.r += xCol;
	            }

	            // draw color beam
	            uv = (2.0 * uv) - 1.0;
	            float beamWidth = (0.7+0.5*cos(uv.x*10.0*tau*0.15*clamp(floor(5.0 + 10.0*cos(_Time.z)), 0.0, 10.0))) * abs(1.0 / (30.0 * uv.y));
            	beamWidth *= 0.5;
	            float3 horBeam = float3(beamWidth,beamWidth,beamWidth);
                return float4(horBeam * horColour, 1.0);
            }
            ENDCG
        }
    }
}
