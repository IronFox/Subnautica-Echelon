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

    void Start()
    {
        material = GetComponent<MeshRenderer>().materials[0];
        meshPro = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
            material.SetVector("_Health", M.V3(target.CurrentHealth, target.MaxHealth,1));
        else
            material.SetVector("_Health", M.V3(0));

    }
}
