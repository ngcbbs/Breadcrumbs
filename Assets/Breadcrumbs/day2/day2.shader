Shader "Unlit/day2-sdfScreen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // data
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
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            // sdf: https://iquilezles.org/articles/distfunctions/
            float dot2( in float2 v ) { return dot(v,v); }
            float dot2( in float3 v ) { return dot(v,v); }
            float ndot( in float2 a, in float2 b ) { return a.x*b.x - a.y*b.y; }
            
            float sdBox( float3 p, float3 b )
            {
                float3 d = abs(p) - b;
                return min(max(d.x,max(d.y,d.z)),0.0) + length(max(d,0.0));
            }
            
            float sdSphere( float3 p, float s ) {
                return length(p)-s;
            }
            
            float2 iBox( in float3 ro, in float3 rd, in float3 rad ) 
            {
                float3 m = 1.0/rd;
                float3 n = m*ro;
                float3 k = abs(m)*rad;
                float3 t1 = -n - k;
                float3 t2 = -n + k;
	            return float2( max( max( t1.x, t1.y ), t1.z ),
	                         min( min( t2.x, t2.y ), t2.z ) );
            }
            
            float3x3 setCamera( in float3 ro, in float3 ta, float cr )
            {
	            float3 cw = normalize(ta-ro);
	            float3 cp = float3(sin(cr), cos(cr),0.0);
	            float3 cu = normalize( cross(cw,cp) );
	            float3 cv =          ( cross(cu,cw) );
                return float3x3(
                    cu.x, cv.x, cw.x,
                    cu.y, cv.y, cw.y,
                    cu.z, cv.z, cw.z
                    );
            }
            
            float2 opU( float2 d1, float2 d2 )
            {
	            return (d1.x<d2.x) ? d1 : d2;
            }
            
            
            float2 map(in float3 pos, float4 data[8]) {
                float2 res = float2( pos.y, 0.0 );
                if (sdBox(pos- float3(0, 0, 0), float3(16,16,16)) < res.x)
                {
                    res = opU(res, float2(sdSphere(pos - data[4].xyz, data[5].x), 5.0));
                    res = opU(res, float2(sdBox(pos - data[2].xyz, data[3].xyz), 12.0));
                }
                return res;
            }
            
            float2 raycast( in float3 ro, in float3 rd, float4 data[8] )
            {
                float2 res = float2(-1.0,-1.0);
            
                float tmin = 1.0;
                float tmax = 20.0;
            
                // hum how to plane calc?
            
                // raytrace floor plane
                float tp1 = (0.0-ro.y)/rd.y;
                if( tp1>0.0 )
                {
                    tmax = min( tmax, tp1 );
                    res = float2( tp1, 1.0 );
                }
                else return res;
                
                // raymarch primitives   
                float2 tb = iBox( ro, rd, float3(32.0,32.0, 32.0) );
                if( tb.x<tb.y && tb.y>0.0 && tb.x<tmax)
                {
                    //return float2(tb.x,2.0);
                    tmin = max(tb.x,tmin);
                    tmax = min(tb.y,tmax);
            
                    float t = tmin;
                    for( int i=0; i<64 && t<tmax; i++ )
                    {
                        float2 h = map( ro+rd*t, data );
                        if( abs(h.x)<(0.01*t) )
                        { 
                            res = float2(t,h.y); 
                            break;
                        }
                        t += h.x;
                    }
                }
                
                return res;
            }

            float4 frag (v2f i) : SV_Target
            {
                // read data - plane, cube, sphere, cylinder
                float4 data[8];

                for (uint d = 0; d < 8; d++)
                    data[d] = tex2D(_MainTex, float2(d%8,d/8) * _MainTex_TexelSize);
                
                // texture resolution
                float2 resolution = float2(1, 1);
                
                // camera	
                float3 look_target = data[0];
                
                //float rotate_speed = _Time.x * 20.;
                //float camera_distance = 5 + data1.x;
                
                float4 camera_position = data[6];
                //float3 ro = look_target + float3( sin(rotate_speed) * camera_distance, 4, cos(rotate_speed) * camera_distance);
                
                // camera-to-world transformation
                //float3x3 ca = setCamera( ro, look_target, 0.0 );
                float3x3 ca = setCamera( camera_position, look_target, 0.0 );
                
                // focal length
                const float fl = 2.5;
                
                float2 p = (2.0 * i.uv - resolution.xy) / resolution.y;
                
                // ray direction
                float3 rd = mul(ca, normalize(float3(p, fl)));
                
                float2 res = raycast(camera_position, rd, data);
                float m = res.y;
                float3 col = 0.2 + 0.2 * sin( m*2.0 + float3(0.0,1.0,2.0) );
                col = pow( col, float3(0.4545, 0.4545,0.4545) );
                float3 color = float3(
                    clamp(col.x,0.0,1.0),
                    clamp(col.y,0.0,1.0),
                    clamp(col.z,0.0,1.0));
                return float4(color, 1);
            }
            ENDCG
        }
    }
}
