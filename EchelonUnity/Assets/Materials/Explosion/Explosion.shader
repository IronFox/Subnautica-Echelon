Shader "Unlit/Explosion"
{
    Properties
    {
        _Noise1 ("Noise1", 2D) = "white" {}
        _Noise2 ("Noise2", 2D) = "white" {}
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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _Noise1;
            sampler2D _Noise2;
            float _Seconds;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            float sqr(float f)
            {
                return f * f;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 color = float4(2,1.2,0.5,1);
                //color.rg = i.uv;
                // // sample the texture
                // fixed4 col = tex2D(_MainTex, i.uv);
                // // apply fog
                // UNITY_APPLY_FOG(i.fogCoord, col);
                float noise = saturate((tex2D(_Noise2, i.uv + float2(2,0.02)*_Seconds*0.1).r 
                            + tex2D(_Noise1, i.uv + float2(-1,0.01)*_Seconds*0.1).r)/2);
                float intensity = //(float3)(tex2D(_Noise1, i.uv).r * 
                    sqr(saturate(((1-abs(i.uv.y-0.5)*2)+noise * 0.5)/ (1.0 + _Seconds)));
                color.a = intensity * saturate(_Seconds);
                
                clip(intensity-0.1);


                return color;
            }
            ENDCG
        }
    }
}
