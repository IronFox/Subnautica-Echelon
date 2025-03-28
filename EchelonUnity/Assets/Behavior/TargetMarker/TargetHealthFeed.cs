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
    public bool isLocked;
    public float fadeIn = 0;

    private static float lowerCriticalHealthSqrt = Mathf.Sqrt(750);
    private static float upperCriticalHealthSqrt = Mathf.Sqrt(3000);


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
            material.SetVector("_Health", M.V4(
                target.CurrentHealth,
                target.MaxHealth,
                1,
                target.IsCriticalTarget
                    ? 1
                    : M.Smoothstep(lowerCriticalHealthSqrt,
                        upperCriticalHealthSqrt,
                        Mathf.Sqrt(target.MaxHealth)
                        )
                )
            );
        else
            material.SetVector("_Health", M.V4(0));
        material.SetFloat("_IsPrimary", isPrimary ? 1 : 0);
        material.SetFloat("_IsLocked", isLocked ? 1 : 0);
        material.SetFloat("_FadeIn", fadeIn);

    }
}
