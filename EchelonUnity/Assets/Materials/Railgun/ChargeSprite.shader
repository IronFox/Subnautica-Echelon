Shader "Unlit/ChargeSprite"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent+1000" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha One
        Cull Off Lighting Off ZWrite Off
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

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float opacity(float r2)
            {
                return 1/(1 + r2*20);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 pos = i.uv * 2 - 1;
                float r2 = dot(pos,pos);
                clip(-(r2 - 1));
                float ar = opacity(r2);
                float a0 = opacity(0);
                float a1 = opacity(1);
                float a = (ar - a1) / (a0 - a1);


                return float4(_Color.rgb, a);
            }
            ENDCG
        }
    }
}
