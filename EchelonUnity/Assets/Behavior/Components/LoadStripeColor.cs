using UnityEngine;

public class LoadStripeColor : MonoBehaviour, IColorListener
{
    public int materialIndex;
    private new MeshRenderer renderer;
    public GlobalMaterialConfig globalMaterialConfig;
    public EchelonControl echelon;

    public void SetColors(
        Color mainColor,
        float mainSmoothness,
        Color stripeColor,
        float stripeSmoothness,
        bool forceReapply)
    {
        forceUpdate = true; //always set here because we don't otherwise compare
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
        if (!echelon)
        {
            ULog.Fail($"No echelon assigned to {this.name}");
            return;
        }
        if (forceUpdate)
        {
            forceUpdate = false;
            if (materialIndex < renderer.materials.Length)
            {
                MaterialAdapter.UpdateColorSmoothness(
                    echelon,
                    renderer,
                    materialIndex,
                    globalMaterialConfig.stripeColor,
                    globalMaterialConfig.stripeSmoothness);
            }
        }

    }
}
