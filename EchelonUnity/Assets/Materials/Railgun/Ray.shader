Shader "Unlit/Ray"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (1,1,1,1)
        _Progress ("Progress", Range(0,100)) = 0
        _FadeIn ("FadeIn", Range(0,100)) = 1
        _FadeOut ("FadeOut", Range(0,100)) = 10
        _Origin ("Origin", Vector) = (0,0,0)
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
            };

            struct v2f
            {
                float4 position : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            float _Progress;
            float _FadeIn;
            float _FadeOut;
            float3 _Origin;

     
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.position = mul(unity_ObjectToWorld, float4( v.vertex.xyz + 1,0));
                //o.position.xyz /= o.position.w;
                o.position.w = length(unity_ObjectToWorld[2].xyz) * (v.vertex.y + 1)/2;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float d = i.position.w;
                float t = _Progress;

                float a = (1-smoothstep(t, t+_FadeIn, d)) * smoothstep(t - _FadeOut, t, d);

                return float4(_Color.xyz,a);
            }
            ENDCG
        }
    }
}
