using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadStripeColor : MonoBehaviour, IColorListener
{
    public int materialIndex;
    private new MeshRenderer renderer;

    public void SetColors(Color mainColor, Color stripeColor)
    {
        if (materialIndex < renderer.materials.Length)
            renderer.materials[materialIndex].color = stripeColor;
    }

    // Start is called before the first frame update
    void Awake()
    {
        renderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
