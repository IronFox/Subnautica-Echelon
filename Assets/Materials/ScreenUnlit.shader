Shader "Unlit/ScreenUnlit"
{
    Properties
    {
        _Noise("Noise (Gray)", 2D) = "white" {}
        _Color ("Color (RGB)", 2D) = "white" {}
        _Depth ("Depth (R)", 2D) = "white" {}
        _PixelSizeX("PixelSizeX", Range(0,1)) = 1
        _PixelSizeX("PixelSizeY", Range(0,1)) = 1
        _NoiseImpact("Noise impact",Range(0,0.2)) = 0.0649
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
            sampler2D _Noise;
            sampler2D _Canvas;
            float4x4 _Unproject;
            float4x4 _UnprojectToView;
            float3 _CameraPosition;
            float _PixelSizeX;
            float _PixelSizeY;
            float _EnabledProgress;
            float _NoiseImpact;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.vCoords = v.vertex*2.0;
                return o;
            }


            float3 Unproject(float4x4 m, float2 obj1, float depth01)
            {
                float4 p = float4(obj1, depth01,1);
                #ifdef SHADER_API_GLCORE    //https://stackoverflow.com/a/58600831
                    p.z = depth01 * 2 - 1;
                #endif
                

                float4 unprojected = mul(m, p);
                return unprojected.xyz / unprojected.w;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 color = tex2D(_Color, i.uv);
                float2 duv = i.uv;
                duv.y = 1.0 - duv.y;
                float4 d0 = tex2D(_Depth, duv);
                float depth = (d0.r);
                float4 c = float4(0,0,0,1);
                float3 world = Unproject(_Unproject, i.vCoords, depth);
                float3 view = Unproject(_UnprojectToView, i.vCoords, depth);


                float actualDepth = distance(world,_CameraPosition);


                float3 cell = fmod(abs(world), 10.0) / 10.0;
                //c.rgb = cell;
                //c.rgb = d0.bga * 0.5 + 0.5;
                float3 cell012 = (cell - 0.5) * 2.0;
                cell012*= cell012;
                float grid = max(max(cell012.x,cell012.y),cell012.z);
                //grid -= any(cell < 0.6);
                //grid *= (1.0 - smoothstep(100,1000,actualDepth)) * smoothstep(20,40,actualDepth);
                //grid = saturate(grid);
                float3 dx3 = ddx(view);
                float3 dy3 = ddy(view);
                float3 normal = normalize(cross(dx3,dy3));


                float fresnel = 1.0+dot(normal,view)/ length(view);
                //saturate( 1.0 - normal.z);
                fresnel *= fresnel;
                 fresnel *= fresnel;
                // float dx = ddx(actualDepth);
                // float dy = ddy(actualDepth);
                //float fresnel = max(abs(dx),abs(dy));
                //fActual *= fActual;
                float brightness = color.r * 0.3 + color.g * 0.6 + color.b * 0.1;
                c.rgb = color.rgb;
                //grid *= smoothstep(10,20,actualDepth) * (1.0 - smoothstep(100,200,actualDepth));
                float gridFade = smoothstep(10,20,actualDepth) * (1.0 - smoothstep(100,200,actualDepth));

                float gridStep = max(abs(ddx(grid)), abs(ddy(grid)));

                float gridInterpolate = gridStep * 2;


                float localProgress = _EnabledProgress - (i.vCoords.x * 0.5 + 0.5) +  (tex2D(_Noise, i.uv).x - 0.5) *2*_NoiseImpact - _NoiseImpact ;
                //localProgress >0 = show

                float strongEffects = 1.0 - smoothstep(0,0.5,localProgress);

                float mod = fresnel*(0.15);
                float upMod = mod * (1.0 - smoothstep(0.4,0.5,brightness));
                float downMod = 1.0 - mod * (smoothstep(0.5,0.6,brightness));

                c.rgb += upMod;
                c.rgb = lerp(c.rgb, normal * 0.5 + 0.5,strongEffects*0.5);


                c.rg *= 1.0 - strongEffects*0.25;



                float upGrid = smoothstep(1.0-1.5*gridInterpolate,1.0 - 0.5*gridInterpolate,grid) * gridFade;
                float downGrid = smoothstep(1.0-4*gridInterpolate,1.0 - 3 * gridInterpolate,grid) * (1.0 - upGrid) * gridFade;
                c.rgb *= 1.0 + upGrid*(0.05 + strongEffects);
                c.rgb *= 1.0 - downGrid *(0.05 + strongEffects);
                c.rgb *= downMod;


                c.rgb *= smoothstep(0,0.01,localProgress);
                c.rgb += smoothstep(0,0.01, localProgress) * (1.0 - smoothstep(0.01, 0.02, localProgress));

                //c.rgb = (float3)fresnel;

                //c.r += pow(fresnel,10);

                // if (brightness > 0.5)
                //     c.rgb -= mod;
                // else
                //     c.rgb += mod;


                float2 canvasUV = i.uv;
                float bendX = i.vCoords.x;
                float bendY = sqrt(1.5-bendX * bendX)-(sqrt(1.5-1));
                canvasUV.y = (canvasUV.y-0.5)*(1 + bendY * 0.2)+0.5;
                float4 canvas = tex2D(_Canvas, canvasUV);
                c.rgb = lerp(c.rgb, canvas.rgb, canvas.a);


                //c.rgb  = unprojected.xyz * 0.01;


                //c.rgb = color.rgb;
                return c;
            }
            ENDCG
        }
    }
}
