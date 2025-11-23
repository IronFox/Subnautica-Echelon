using UnityEngine;

public class BakeryApply : MonoBehaviour
{
    public TextureBakery bakery;
    public int targetMaterialSlot;
    // Start is called before the first frame update
    private int versionCounter = -1;
    private new MeshRenderer renderer;

    void Awake()
    {
        renderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!bakery || !renderer)
            return;
        if (versionCounter != bakery.versionCounter)
        {
            versionCounter = bakery.versionCounter;
            if (targetMaterialSlot < renderer.materials.Length)
            {
                var targetMaterial = renderer.materials[targetMaterialSlot];
                //MaterialAdapter.UpdateColorSmoothness(bakery.echelon, renderer, targetMaterialSlot, Color.white, bakery.GetSmoothness());
                MaterialAdapter.UpdateMainTexture(bakery.echelon, renderer, targetMaterialSlot, bakery.GetBakedTexture());

                //ULog.Write($"Assigned {texture.GetInstanceID()} to material {targetMaterial.GetInstanceID()}/{targetMaterial.mainTexture.GetInstanceID()} using shader {targetMaterial.shader}");
            }
            else
                ULog.Fail($"Unable to assign generated texture");
        }
    }
}
