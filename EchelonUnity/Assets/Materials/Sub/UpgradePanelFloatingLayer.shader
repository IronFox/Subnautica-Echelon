Shader "Unlit/UpgradePanelFloatingLayer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Phase ("Phase", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            #define M_PI 3.14159265359


            sampler2D _MainTex;
            sampler2D _Phase;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, 1-i.uv);
                col.rgb *= col.rgb;
                col.rgb *= col.rgb;
                col.rgb *= col.rgb;
                col.rgb *= 2;
                float opacity = cos(tex2D(_Phase, i.uv).x*2*M_PI + _Time.z);
                col.a = saturate(opacity);
                return col;
            }
            ENDCG
        }
    }
}
