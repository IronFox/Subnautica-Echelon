using UnityEngine;
using UnityEngine.Rendering;

public class TextureBakery : MonoBehaviour, IColorListener
{
    public Texture colorOverlay;
    public Color colorOverlayConstant = Color.white;
    public Texture bodyOpacityTexture;
    [Range(0f, 1f)]
    public float bodyOpacityConstant = 0f;
    public GlobalMaterialConfig globalMaterialConfig;
    public EchelonControl echelon;

    private bool forceReapply;
    private Color lastMainColor;
    private Color lastStripeColor;
    private RenderTexture texture;
    private Mesh screenQuad;
    private float lastStripeSmoothness = -1;
    private float lastMainSmoothness = -1;

    public int versionCounter = 0;

    private int width;
    private int height;

    public Texture GetBakedTexture()
    {
        return texture;
    }

    // Start is called before the first frame update
    void Awake()
    {
        width = colorOverlay ? colorOverlay.width : 1;
        height = colorOverlay ? colorOverlay.height : 1;
        texture = new RenderTexture(new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32));
        texture.useMipMap = true;
        texture.autoGenerateMips = true;
        screenQuad = new Mesh();
        screenQuad.vertices = new Vector3[4] {
            new Vector3(-1, -1, 0),
            new Vector3(1, -1, 0),
            new Vector3(1, 1, 0),
            new Vector3(-1, 1, 0),
        };

        screenQuad.triangles = new int[6] {
            0, 1, 2,
            0, 2, 3
        };

        screenQuad.uv = new Vector2[4] {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };
        screenQuad.RecalculateBounds();
    }

    void OnDestroy()
    {
        Destroy(texture);
    }


    // Update is called once per frame
    void Update()
    {
        if (!globalMaterialConfig)
        {
            ULog.Fail($"No global material config assigned to {this.name}");
            return;
        }
        if (!echelon)
        {
            ULog.Fail($"No echelon assigned to {this.name}");
            return;
        }
        if (
            globalMaterialConfig.mainColor != lastMainColor
            || globalMaterialConfig.stripeColor != lastStripeColor
            || globalMaterialConfig.mainSmoothness != lastMainSmoothness
            || globalMaterialConfig.stripeSmoothness != lastStripeSmoothness
            || forceReapply)
        {
            versionCounter++;
            lastMainColor = globalMaterialConfig.mainColor;
            lastStripeColor = globalMaterialConfig.stripeColor;
            lastMainSmoothness = globalMaterialConfig.mainSmoothness;
            lastStripeSmoothness = globalMaterialConfig.stripeSmoothness;
            forceReapply = false;
            //ULog.Write($"(Re)Baking texture using shader {globalMaterialConfig.bakeShader} for colors {globalMaterialConfig.mainColor}/{globalMaterialConfig.stripeColor} and smoothness levels {lastMainSmoothness}/{lastStripeSmoothness}");
            var bakeMaterial = new Material(globalMaterialConfig.bakeShader);

            using (var command = new CommandBuffer())
            {
                command.name = "Texture Bakery";

                bakeMaterial.SetTexture($"_Source", colorOverlay.Or(OnePixelTexture.Get(colorOverlayConstant)));
                bakeMaterial.SetTexture($"_StripeMask", bodyOpacityTexture.Or(OnePixelTexture.GetGray(bodyOpacityConstant)));
                bakeMaterial.SetColor($"_MainColor", lastMainColor);
                bakeMaterial.SetFloat($"_MainSmoothness", lastMainSmoothness);
                bakeMaterial.SetColor($"_StripeColor", lastStripeColor);
                bakeMaterial.SetFloat($"_StripeSmoothness", lastStripeSmoothness);

                command.SetRenderTarget(texture);
                //command.ClearRenderTarget(true, true, Color.black);
                command.DrawMesh(screenQuad, Matrix4x4.identity, bakeMaterial);

                Graphics.ExecuteCommandBuffer(command);
            }
            Destroy(bakeMaterial);
            ULog.Write($"Re-Baked " + name);
        }
    }

    public void SetColors(
        Color mainColor,
        float mainSmoothness,
        Color stripeColor,
        float stripeSmoothness,
        bool forceReapply)
    {
        if (forceReapply)
            this.forceReapply = true;
    }
}
