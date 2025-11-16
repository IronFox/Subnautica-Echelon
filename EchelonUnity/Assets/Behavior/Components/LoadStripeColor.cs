using UnityEngine;

public class LoadStripeColor : MonoBehaviour, IColorListener
{
    public int materialIndex;
    private new MeshRenderer renderer;
    public GlobalMaterialConfig globalMaterialConfig;

    public void SetColors(Color mainColor, Color stripeColor, bool forceReapply)
    {
        forceUpdate = true;
    }

    // Start is called before the first frame update
    void Awake()
    {
        renderer = GetComponent<MeshRenderer>();
    }

    private bool forceUpdate = true;

    // Update is called once per frame
    void Update()
    {
        if (!globalMaterialConfig)
        {
            ULog.Fail($"No global material config assigned to {this.name}");
            return;
        }
        if (forceUpdate)
        {
            forceUpdate = false;
            if (materialIndex < renderer.materials.Length)
            {
                renderer.materials[materialIndex].color = globalMaterialConfig.stripeColor;
                renderer.materials[materialIndex].SetFloat("_Glossiness", globalMaterialConfig.stripeSmoothness);
            }
        }

    }
}
