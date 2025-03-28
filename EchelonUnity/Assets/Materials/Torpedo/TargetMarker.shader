Shader "Unlit/Target"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Health ("Health", Vector) = (0.5,1,1,0)
        _CameraX("CameraX", Vector) = (1,0,0)
        _CameraY("CameraY",Vector) = (0,1,0)
        _Scale("Scale", Range(0.1,10)) = 1
        _FadeIn("FadeIn", Range(0,1)) = 1
        [MaterialToggle] _IsPrimary ("Is Primary", Float) = 1
        [MaterialToggle] _IsLocked ("Is Locked", Float) = 1
        
    }
    SubShader
    {
        Tags { "Queue"="Transparent+1000" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off 
        Cull Off
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
                float2 uv: TEXCOORD0;

            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float3 _CameraX;
            float3 _CameraY;
            float4 _Health;
            float3 _ObjCenter;
            float _Scale;
            float _IsPrimary;
            float _IsLocked;
            float _FadeIn;

            v2f vert (appdata v)
            {
                v2f o;
                //float3 objCenter = mul(UNITY_MATRIX_M,float4(0,0,0,1)).xyz;
                float3 dir = (_ObjCenter - _WorldSpaceCameraPos);
                float3 y = normalize(cross(_CameraX,dir));
                float3 x = normalize(cross(dir,_CameraY));

                float3 world = v.vertex.x * _CameraX*_Scale + v.vertex.y * _CameraY*_Scale /* * y */ + _ObjCenter;

                o.vertex = mul(UNITY_MATRIX_VP,float4(world,1));
                o.uv = v.uv;


                return o;
            }

            float dd(float v)
            {
                return max(abs(ddx(v)),abs(ddy(v)));
            }

            float hardRange(float begin, float end, float value, float valueDD)
            {
                return (1.0 - smoothstep(end-valueDD*2, end,value)) * smoothstep(begin, begin+valueDD*2, value);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 c = (float4)1;
                c.rg = i.uv;

                float alpha = 1;
                
                float2 xy = i.uv * 2 - 1;
                float r = length(xy);
                float rd = dd(r);
                
                alpha = hardRange(0.8,0.99,r, rd);


                float circularAngle = atan2(xy.y,xy.x);

                float radial = 0.5 + 0.5 * sin(circularAngle * 3 + _Time.w);
                float radialDD = dd(radial);

                if (_Health.z < 0.5)
                {
                    if (_IsLocked > 0.5)
                    {
                        //alpha *= 0.5 * (1.0 - smoothstep(0.6, 0.6+d*4, cs)) * smoothstep(0.2, 0.2+d2*2,radial );
                        c.rgb = float3(1,0.5,0.4)*1.5 //* (1.0 - smoothstep(0.5, 0.5+d*4, cs))
                                //* smoothstep(0.4, 0.4+d*2, cs)
                                // * smoothstep(0.3, 0.3+radialDD*2,radial )
                                * hardRange(0.85,0.94, r, rd)
                                * smoothstep(0.38, 0.38+radialDD*2,radial );
                                ;

                        alpha *= smoothstep(0.3, 0.3+radialDD*2,radial );
                    }
                }
                else
                {
                    float circularAngleOne = (circularAngle + M_PI) / (2* M_PI);
                    float circular2Fmod = fmod(circularAngleOne*2,1);
                    float circular2FmodDD = dd(circular2Fmod);
                    float flash = _Health.w * 0.9;
                    float relHealth = _Health.x / _Health.y;
                    float radialH = hardRange(0, 0.1 + 0.9 * relHealth, circular2Fmod, circular2FmodDD);
                    //float d2 = max(abs(ddx(radial)),abs(ddy(radial)));
                    alpha *= 0.5 * radialH;
                    c.rgb = float3(
                        smoothstep(0.2, 0.4, 1-relHealth),
                        smoothstep(0.2, 0.4, relHealth),
                        0
                        ) 
                            * hardRange(0.85,0.94, r, rd)
                            * hardRange(0.02, 0.1 + 0.9 * relHealth-0.02, circular2Fmod, circular2FmodDD)
                            * ((1 - flash) + (cos(_Time.z* flash*10)*0.5 + 0.5)*flash)
                             ;

                    
                    alpha *= 0.1 + 0.9 * _IsPrimary;
                    if (_IsLocked > 0.5)
                    {
                        alpha = max(alpha, 
                            hardRange(0.6,0.8,r, rd)
                            * smoothstep(0.3, 0.3+radialDD*2,radial )
                            );
                        //c.rgb = (float3)radial;

                        c.rgb += float3(1,0.5,0.4)*1.5 //* (1.0 - smoothstep(0.5, 0.5+d*4, cs))
                        //         //* smoothstep(0.4, 0.4+d*2, cs)
                                  * smoothstep(0.38, 0.38+radialDD*2,radial )
                                  * hardRange(0.65,0.75, r, rd)
                        //         * 
                                 ;
                    }
                    // alpha *= smoothstep(0.3, 0.3+radialDD*2,radial );
                }

                c.a = alpha * _FadeIn;
                return c;
               
            }
            ENDCG
        }
    }
}
