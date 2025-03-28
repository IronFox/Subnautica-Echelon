Shader "Unlit/EnergyLevel"
{
    Properties
    {
        _EnergyLevel ("Energy Level", Vector) = (1,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha One
        Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
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
                float4 vertex : SV_POSITION;
            };

            float3 _EnergyLevel;      //x=max, y=current, z=change
            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float dd(float v)
            {
                return max(abs(ddx(v)), abs(ddy(v)));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = (fixed4)1;
                
                float at = i.uv.x * 2;
                if (at < 1)
                    at += (i.uv.y-0.5)*-0.1;
                else
                    at -= (i.uv.y-0.5)*-0.1;
                if (at > 1)
                    at = 2 - at;

                float atY = i.uv.y * 2;
                if (atY > 1)
                    atY = 2 - atY;
                
                    

                at = 1.0 - at;

                float d = dd(at);

                float fill = _EnergyLevel.y / _EnergyLevel.x * 1.03;
                float travel = _EnergyLevel.z / _EnergyLevel.x;
                
                float lit = smoothstep(at-d,at+d,fill);
                float inTravelA = smoothstep(at-d, at+d, fill+abs(travel)+0.02);
                float inTravelB = smoothstep(at-d, at+d, fill+0.02);
                float innerLit = smoothstep(at-0.2,at+0.2,fill - 0.1);
                float inTravel;
                float animation;
                float vAnimation;
                lit *= (1 - innerLit * smoothstep(0.25,0.35,atY)) * 0.75 + 0.25;
                inTravel = inTravelA * (1 - inTravelB);

                if (travel > 0)
                {
                    //inTravel *= atY > (1.0 - fmod(_Time.y,1));
                    animation = -at*40;
                    vAnimation = atY;
                    
                }
                else
                {
                    vAnimation = -atY;
                    animation = at*40;
                    //inTravel *= atY > (fmod(_Time.y,1));

//                    lit = smoothstep(at-d, at+d, fill+travel);
                    //fill = inTravelC;
                }

                float3 litColor = float3(
                    saturate(1.0 - fill*2),
                    saturate(fill * 2),
                    0

                    );

                float pulse = lerp(
                    (sin(_Time.z*4 + animation)+0.5)*0.5,
                    (sin(_Time.z*4)+0.5)*0.5,
                    1- smoothstep(0,0.1,fill));
                float3 backColor = float3(
                    1 - smoothstep(0.1,0.2,fill),
                    travel <= 0 ? 0 : 1 - smoothstep(0.1,0.2,fill),
                    0 )
                    * pulse;
                float3 color = lerp(backColor, litColor, lit);
                float3 travelColor = 
                    float3(
                        max((travel < 0), (1-smoothstep(0.1,0.2, fill))),
                        travel >= 0,
                        0
                    );
                
                travel > 0 ?  float3(0,1,0) : float3(1,0,0);
                //travelColor *= smoothstep(0.25,0.251,((sin(_Time.z*4 + animation)+0.5)*0.5));
                travelColor *= fmod(vAnimation+_Time.y,1);
                col.rgb = lerp(color, travelColor, inTravel );
                col.a = 0.8;

                return col;
            }
            ENDCG
        }
    }
}
