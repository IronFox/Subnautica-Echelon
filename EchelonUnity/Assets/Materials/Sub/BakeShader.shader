Shader "Unlit/BakeShader"
{
    Properties
    {
        _Source ("Texture", 2D) = "white" {}
        _StripeMask ("Stripe Mask Texture", 2D) = "white" {}
        _MainColor ("Main Color",Color) = (1,1,1)
        _StripeColor("Stripe Color", Color) = (1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off
        Blend One Zero

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

            sampler2D _Source;
            sampler2D _StripeMask;
            float4 _Source_ST;
            float4 _MainColor;
            float4 _StripeColor;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                #if UNITY_UV_STARTS_AT_TOP == 1
                    o.uv.y = 1 - o.uv.y;
                #endif
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_Source, i.uv);
                float mask = tex2D(_StripeMask,i.uv).r;
                //return col;
                //return 1-float4(mask,mask,mask,0);
                //return _StripeColor;
                //return float4(1,0,0,1);
                return float4(col.rgb * lerp(_StripeColor.rgb, _MainColor.rgb, mask), 1);
                //return float4(1,1,0, 1);
            }
            ENDCG
        }
    }
}
