using UnityEngine;
using UnityEngine.Rendering;

public class TextureBakery : MonoBehaviour, IColorListener
{
    public Texture sourceTexture;
    public Texture mapTexture;
    public Color mainColor = new Color(0xDE, 0xDE, 0xDE) / 255f;
    public Color stripeColor = new Color(0x3F, 0x4C, 0x7A) / 255f;
    public Shader bakeShader;
    public int targetMaterialSlot;

    private Color lastMainColor;
    private Color lastStripeColor;
    private RenderTexture texture;
    private Mesh screenQuad;
    private new MeshRenderer renderer;
    // Start is called before the first frame update
    void Start()
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
        if (mainColor != lastMainColor || stripeColor != lastStripeColor)
        {
            lastMainColor = mainColor;
            lastStripeColor = stripeColor;
            ULog.Write($"(Re)Baking texture using shader {bakeShader}");
            var bakeMaterial = new Material(bakeShader);

            using (var command = new CommandBuffer())
            {
                command.name = "Texture Bakery";

                bakeMaterial.SetTexture($"_Source", sourceTexture);
                bakeMaterial.SetTexture($"_StripeMask", mapTexture);
                bakeMaterial.SetColor($"_MainColor", mainColor);
                bakeMaterial.SetColor($"_StripeColor", stripeColor);

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

                targetMaterial.mainTexture = texture;
                //targetMaterial.SetTexture($"_MainTex", texture);
                ULog.Write($"Assigned to material using shader {targetMaterial.shader}");
            }
            else
                ULog.Fail($"Unable to assign generated texture");
        }
    }

    public void SetColors(Color mainColor, Color stripeColor)
    {
        this.mainColor = mainColor;
        this.stripeColor = stripeColor;
    }
}
