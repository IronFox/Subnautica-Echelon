Shader "Unlit/TargetDirectionMarker"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent+1000" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off 
        //Cull Off
        ZWrite Off 
        //ZTest Off
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
            };

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = normalize(mul(UNITY_MATRIX_MV, float4(v.normal,0)).xyz);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float fresnel = smoothstep(0.3, 1, i.normal.z) * 0.5;
                // sample the texture
                return (1-fresnel) * _Color;
            }
            ENDCG
        }
    }
}
