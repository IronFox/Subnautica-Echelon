using UnityEngine;
using UnityEngine.Rendering;

public class TextureBakery : MonoBehaviour, IColorListener
{
    public Texture sourceTexture;
    public Texture mapTexture;
    public GlobalMaterialConfig globalMaterialConfig;
    public EchelonControl echelon;

    public int targetMaterialSlot;
    private bool forceReapply;
    private Color lastMainColor;
    private Color lastStripeColor;
    private RenderTexture texture;
    private Mesh screenQuad;
    private new MeshRenderer renderer;
    private float lastStripeSmoothness = -1;
    private float lastMainSmoothness = -1;

    // Start is called before the first frame update
    void Awake()
    {
        renderer = GetComponent<MeshRenderer>();
        texture = new RenderTexture(new RenderTextureDescriptor(sourceTexture.width, sourceTexture.height, RenderTextureFormat.ARGB32));
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
            lastMainColor = globalMaterialConfig.mainColor;
            lastStripeColor = globalMaterialConfig.stripeColor;
            lastMainSmoothness = globalMaterialConfig.mainSmoothness;
            lastStripeSmoothness = globalMaterialConfig.stripeSmoothness;
            forceReapply = false;
            ULog.Write($"(Re)Baking texture using shader {globalMaterialConfig.bakeShader} for colors {globalMaterialConfig.mainColor}/{globalMaterialConfig.stripeColor} and smoothness levels {lastMainSmoothness}/{lastStripeSmoothness}");
            var bakeMaterial = new Material(globalMaterialConfig.bakeShader);

            using (var command = new CommandBuffer())
            {
                command.name = "Texture Bakery";

                bakeMaterial.SetTexture($"_Source", sourceTexture);
                bakeMaterial.SetTexture($"_StripeMask", mapTexture);
                bakeMaterial.SetColor($"_MainColor", lastMainColor);
                bakeMaterial.SetFloat($"_MainSmoothness", lastMainSmoothness);
                bakeMaterial.SetColor($"_StripeColor", lastStripeColor);
                bakeMaterial.SetFloat($"_StripeSmoothness", lastStripeSmoothness);

                command.SetRenderTarget(texture);
                //command.ClearRenderTarget(true, true, Color.black);
                command.DrawMesh(screenQuad, Matrix4x4.identity, bakeMaterial);

                Graphics.ExecuteCommandBuffer(command);
            }
            //ULog.Write($"Releasing bake material");
            Destroy(bakeMaterial);

            if (renderer != null && targetMaterialSlot < renderer.materials.Length)
            {

                var targetMaterial = renderer.materials[targetMaterialSlot];

                MaterialAdapter.UpdateMainTexture(echelon, renderer, targetMaterialSlot, texture);
                //targetMaterial.SetTexture($"_MainTex", texture);
                ULog.Write($"Assigned {texture.GetInstanceID()} to material {targetMaterial.GetInstanceID()}/{targetMaterial.mainTexture.GetInstanceID()} using shader {targetMaterial.shader}");
            }
            else
                ULog.Fail($"Unable to assign generated texture");
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
