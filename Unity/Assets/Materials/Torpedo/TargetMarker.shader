Shader "Unlit/Target"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Health ("Health", Vector) = (0.5,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off 
        /* Cull Off */ 
        Lighting Off 
        ZWrite Off 
        ZTest Off
        Fog { Color (0,0,0,0) }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            #define M_PI 3.14159265359

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal: NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal: NORMAL;
                float3 pointWorld: TEXCOORD0;
                float3 pointCamera: TEXCOORD1;
                float4 eyeObjectCenter: TEXCOORD2;
                float4 originalVertex: TEXCOORD3;
                float4 eyeVertex: TEXCOORD4;

            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float3 _CameraCenter;
            float3 _Health;

            v2f vert (appdata v)
            {
                v2f o;
                o.originalVertex = o.vertex = UnityObjectToClipPos(v.vertex);
                o.eyeVertex = mul(UNITY_MATRIX_MV, v.vertex);
                o.normal = normalize(mul(unity_ObjectToWorld,float4(v.normal.xyz,0)).xyz);
                o.pointWorld = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.eyeObjectCenter =mul(UNITY_MATRIX_MV,float4(0,0,0,1));
                //float3 cs = mul(transpose(UNITY_MATRIX_V), float4(0,0,0,1)).xyz;
                o.pointCamera = o.pointWorld - _CameraCenter;

                return o;
            }

            float dd(float v)
            {
                return max(abs(ddx(v)),abs(ddy(v)));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 c3 = i.eyeObjectCenter.xyz / i.eyeObjectCenter.w;
                float3 p3 = i.eyeVertex.xyz / i.eyeVertex.w;// / i.vertex.w;
                float2 p2 = p3.xy - c3.xy;
                float circularAngle = atan2(p2.y,p2.x) ;

                // sample the texture
                fixed4 col = (float4)1;
                float cs = abs(dot(i.pointCamera, i.normal)) / (length(i.pointCamera) * length(i.normal));
                float d = dd(cs);
                if (_Health.z < 0.5)
                {
                    float radial = 0.5 + 0.5 * sin(circularAngle * 3 + _Time.w);
                    float d2 = max(abs(ddx(radial)),abs(ddy(radial)));
                    col.a = 0.5 * (1.0 - smoothstep(0.6, 0.6+d*4, cs)) * smoothstep(0.2, 0.2+d2*2,radial );
                    col.rgb = float3(1,0.5,0.4)*1.5 * (1.0 - smoothstep(0.5, 0.5+d*4, cs))
                            * smoothstep(0.4, 0.4+d*2, cs)
                             * smoothstep(0.3, 0.3+d2*2,radial );
                }
                else
                {
                    float circularAngleOne = (circularAngle + M_PI) / (2* M_PI);
                    float circular2Fmod = fmod(circularAngleOne*2,1);
                    float dc = dd(circular2Fmod);
                    float flash = smoothstep(sqrt(750),sqrt(3000),  sqrt(_Health.y)) * 0.9;
                    float relHealth = _Health.x / _Health.y;
                    float radial = smoothstep(circular2Fmod-dc, circular2Fmod+dc,0.1 + 0.9 * relHealth);
                    //float d2 = max(abs(ddx(radial)),abs(ddy(radial)));
                    col.a = 0.5 * (1.0 - smoothstep(0.6, 0.6+d*4, cs)) * radial;
                    col.rgb = float3(
                        smoothstep(0.2, 0.4, 1-relHealth),
                        smoothstep(0.2, 0.4, relHealth),
                        0
                        ) * (1.0 - smoothstep(0.5, 0.5+d*4, cs))
                            * smoothstep(0.4, 0.4+d*2, cs)
                            * ((1 - flash) + (cos(_Time.z* flash*10)*0.5 + 0.5)*flash)
                             ;


                }
                clip(col.a - 0.1);
                //circularAngle;
                //tex2D(_MainTex, i.uv);
                // apply fog
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
