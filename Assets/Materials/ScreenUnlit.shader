Shader "Unlit/ScreenUnlit"
{
    Properties
    {
        _Color ("Color (RGB)", 2D) = "white" {}
        _Depth ("Depth (R)", 2D) = "white" {}
        _PixelSizeX("PixelSizeX", Range(0,1)) = 1
        _PixelSizeX("PixelSizeY", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
                float2 vCoords: TEXCOORD1;
            };


            sampler2D _Color;
            sampler2D _Depth;
            float4x4 _Unproject;
            float _PixelSizeX;
            float _PixelSizeY;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.vCoords = v.vertex*2.0;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 color = tex2D(_Color, i.uv);
                float2 duv = i.uv;
                duv.y = 1.0 - duv.y;
                float4 d0 = tex2D(_Depth, duv);
                float depth = (d0.r);
                float actualDepth = 1.0/depth;
                float4 c = float4(0,0,0,1);
                

                float4 projected = float4(i.vCoords, depth,1);
                #ifdef SHADER_API_GLCORE    //https://stackoverflow.com/a/58600831
                    projected.z = depth * 2 - 1;
                #endif

                float4 unprojected = mul(_Unproject, projected);
                float3 world  = unprojected.xyz / unprojected.w;

                float3 cell = fmod(abs(world), 2.0) / 2.0;
                //c.rgb = cell;
                //c.rgb = d0.bga * 0.5 + 0.5;
                cell = (cell - 0.5) * 2.0;
                cell*= cell;
                float grid = max(max(cell.x,cell.y),cell.z);
                grid *= (1.0 - smoothstep(100,1000,actualDepth)) * smoothstep(20,40,actualDepth);
                float dx = ddx(depth);
                float dy = ddy(depth);
                float fresnel = max(abs(dx),abs(dy));
                float fActual = min(fresnel*1000,1);
                //fActual *= fActual;
                float brightness = color.r * 0.3 + color.g * 0.6 + color.b * 0.1;
                c.rgb = color.rgb;
                float mod = fActual*0.1 + smoothstep(0.8,1.0,grid)*0.05;
                float upMod = mod * (1.0 - smoothstep(0.4,0.5,brightness));
                float downMod = mod * (smoothstep(0.5,0.6,brightness));
                c.rgb += upMod - downMod;
                // if (brightness > 0.5)
                //     c.rgb -= mod;
                // else
                //     c.rgb += mod;






                //c.rgb  = unprojected.xyz * 0.01;


                //c.rgb = color.rgb;
                return c;
            }
            ENDCG
        }
    }
}
