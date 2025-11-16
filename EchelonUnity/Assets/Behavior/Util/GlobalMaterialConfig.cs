using UnityEngine;

public class GlobalMaterialConfig : MonoBehaviour, IColorListener
{
    public Color mainColor = new Color(0xDE, 0xDE, 0xDE) / 255f;
    public Color stripeColor = new Color(0x3F, 0x4C, 0x7A) / 255f;
    public Shader bakeShader;
    public float stripeSmoothness = 0.5372549f;
    public float mainSmoothness = 0.8f;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetColors(Color mainColor, Color stripeColor, bool forceReapply)
    {
        this.mainColor = mainColor;
        this.stripeColor = stripeColor;
    }

}
