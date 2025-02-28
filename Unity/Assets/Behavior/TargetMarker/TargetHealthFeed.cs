using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class TargetHealthFeed : MonoBehaviour
{
    // Start is called before the first frame update
    public TargetAdapter target;
    public EchelonControl owner;
    private Material material;
    private TextMeshProUGUI meshPro;
    public bool isPrimary;
    public float fadeIn = 0;

    void Start()
    {
        material = GetComponent<MeshRenderer>().materials[0];
        meshPro = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        fadeIn = M.Saturate(fadeIn + Time.deltaTime*0.5f);
        if (target != null)
            material.SetVector("_Health", M.V3(target.CurrentHealth, target.MaxHealth,1));
        else
            material.SetVector("_Health", M.V3(0));
        material.SetFloat("_IsPrimary", isPrimary ? 1 : 0);
        material.SetFloat("_FadeIn", fadeIn);

    }
}
