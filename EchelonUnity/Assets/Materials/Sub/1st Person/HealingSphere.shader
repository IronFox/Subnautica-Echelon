Shader "Unlit/HealingSphere"
{
    Properties
    {
        _HealingVisibility ("Healing Visibility", Range(0,1)) = 0
		_NoiseTexture0("Noise Texture 0", 2D) = "white" {}
		_NoiseTexture1("Noise Texture 1", 2D) = "white" {}

    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha One
        Cull Front 
        Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
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
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float2 projected : TEXCOORD4;
                float3 v: TEXCOORD1;
                float3 n: TEXCOORD2;
                float3 world: TEXCOORD3;
            };

            float _HealingVisibility;
            
    		sampler2D _NoiseTexture0;
    		sampler2D _NoiseTexture1;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.projected = o.vertex.xy / o.vertex.w;
                o.world = mul(unity_ObjectToWorld,v.vertex).xyz;
                o.v = v.vertex.xyz*0.1;
                o.uv =v.uv;
                o.n = v.normal;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = (fixed4)1;

                float noise0 = tex2D(_NoiseTexture0, i.v.xz * 50 + float2(0,_Time.x*2)).r;
                float noise1 = tex2D(_NoiseTexture0, i.v.xy * 50 + float2(_Time.x*1.5,0)).r;
                float noise2 = tex2D(_NoiseTexture1, i.v.xz * 50 - float2(0,_Time.x*2)).r;
                float noise3 = tex2D(_NoiseTexture1, i.v.xy * 50 - float2(_Time.x*1.5,0)).r;
                float intensity = sin(-_Time.z*2 + i.v.y*1000 + noise0 + noise1) * 0.5 + 0.5;

                col.rgb = intensity * _HealingVisibility * float3(0,1,0.3);

                float fresnel = pow(dot(normalize(_WorldSpaceCameraPos - i.world), i.n),4);
                col.rgb *= 1 - fresnel;
                col.rgb *= noise3 * noise2;
                col.rgb *= smoothstep(0.8,1.0,length(i.projected));
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
