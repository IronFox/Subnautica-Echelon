using UnityEngine;

public class DebugColorEmitter : MonoBehaviour
{
    public Color mainColor = Color.white;
    public Color stripeColor = new Color(0x3F / 255f, 0x4C / 255f, 0x7A / 255f);

    private Color lastMainColor = Color.white;
    private Color lastStripeColor = new Color(0x3F / 255f, 0x4C / 255f, 0x7A / 255f);
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (mainColor != lastMainColor || stripeColor != lastStripeColor)
        {
            lastMainColor = mainColor;
            lastStripeColor = stripeColor;
            var listeners = GetComponentsInChildren<IColorListener>();
            foreach (var l in listeners)
                l.SetColors(mainColor, stripeColor, true);
            Debug.Log($"SetColors: {mainColor} {stripeColor}");
        }
    }
}
