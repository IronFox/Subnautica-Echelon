using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectMarkerAtTarget : PerformanceCaptured_U
{
    public ITargetable target;
    public AdapterTargetable adapterTarget;
    public EchelonControl echelon;
    private float fadeIn;
    public float radius = 3;
    public float scale = 1;
    private MeshRenderer r;
    private Material m;
    private float distanceFade = 1;
    private float flash = 0;
    private Vector3 color = Vector3.one;
    private Vector3 lastDirection;

    private float age;

    // Start is called before the first frame update
    void Start()
    {
        r = GetComponentInChildren<MeshRenderer>();
        m = r.materials[0];
    }


    // Update is called once per frame
    protected override void P_Update()
    {
        age += Time.deltaTime;
        var ep = (echelon != null ? echelon.transform.position : M.V3(-10, 0, 0));
        if (target != null && target.Exists)
        {
            fadeIn = M.Saturate(fadeIn + Time.deltaTime * 0.5f);

            var d = target.Position - ep;
            var distance = d.magnitude;
            d /= distance;
            lastDirection = d;
            transform.rotation = Quaternion.LookRotation(d);
            transform.position = ep + d * radius;
            transform.localScale = M.V3(scale);
            distanceFade = 1f / (1f + distance*0.02f);
            if (adapterTarget != null)
            {
                //Debug.Log($"DirectMarkerAt {adapterTarget} {adapterTarget.TargetAdapter.MaxHealth}");
                if (adapterTarget.TargetAdapter.MaxHealth > 3000
                    || adapterTarget.IsCriticalTarget)
                    color = M.V3(1.5f, 0.2f, 0.1f);
                else if (adapterTarget.TargetAdapter.MaxHealth > 1000)
                {
                    if (EchelonControl.targetArrows == TargetArrows.CriticalOnly)
                    {
                        Destroy(gameObject);
                        return;
                    }
                    color = M.V3(0.75f, 0.75f, 0);
                }
                else
                {
                    Destroy(gameObject);
                    return;
                }
            }
            else
                color = M.V3(1);
            flash = 1f - M.Smoothstep(50, 100, distance);
        }
        else
        {
            fadeIn -= Time.deltaTime * 2f;
            if (fadeIn <= 0)
                Destroy(gameObject);
            else
            {
                transform.rotation = Quaternion.LookRotation(lastDirection);
                transform.position = ep + lastDirection * radius;
                transform.localScale = M.V3(scale);

            }
        }
        float flashSin = Mathf.Sin(age * 10f)
                    * 0.5f
                    + 0.5f;
        float scaledFlashSin = M.Interpolate(
                    1,
                    flashSin,
                    flash
                    );

        m.SetVector("_Color", 
            M.V4(color * scaledFlashSin
                , fadeIn
                * distanceFade
                )
            );
        

    }
}
