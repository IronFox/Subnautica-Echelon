using UnityEngine;


public class DebugColorEmitter : MonoBehaviour
{
    public Color mainColor = Color.white;
    public Color stripeColor = new Color(0x3F / 255f, 0x4C / 255f, 0x7A / 255f);
    public float mainSmoothness = GlobalMaterialConfig.DefaultMainSmoothness;
    public float stripeSmoothness = GlobalMaterialConfig.DefaultStripeSmoothness;

    private Color lastMainColor = Color.white;
    private Color lastStripeColor = new Color(0x3F / 255f, 0x4C / 255f, 0x7A / 255f);
    private float lastMainSmoothness = -1;
    private float lastStripeSmoothness = -1;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (mainColor != lastMainColor
            || stripeColor != lastStripeColor
            || stripeSmoothness != lastStripeSmoothness
            || mainSmoothness != lastMainSmoothness)
        {
            lastMainColor = mainColor;
            lastStripeColor = stripeColor;
            lastMainSmoothness = mainSmoothness;
            lastStripeSmoothness = stripeSmoothness;
            var listeners = GetComponentsInChildren<IColorListener>();
            foreach (var l in listeners)
                l.SetColors(mainColor, mainSmoothness, stripeColor, stripeSmoothness, true);
            Debug.Log($"SetColors: {mainColor} {stripeColor}");
        }
    }
}
