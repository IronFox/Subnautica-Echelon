using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    public GameObject fireRingPrefab;
    public GameObject pressureRingPrefab;
    private GameObject 
        fireRing0,
        fireRing1,
        fireRing2,
        pressureRing0,
        pressureRing1
        
        ;



    private float time;

    // Start is called before the first frame update
    void Start()
    {
        fireRing0 = Instantiate(fireRingPrefab, transform);
        fireRing1 = Instantiate(fireRingPrefab, transform);
        fireRing2 = Instantiate(fireRingPrefab, transform);
        fireRing1.transform.localEulerAngles = M.V3(Random.Range(-180f,180f),Random.Range(-90f,90f),0);
        pressureRing0 = Instantiate(pressureRingPrefab, transform);   
        pressureRing1 = Instantiate(pressureRingPrefab, transform);
        pressureRing1.transform.localEulerAngles = fireRing1.transform.localEulerAngles;
        AudioPatcher.PatchAll(transform);

        HierarchyAnalyzer hierarchyAnalyzer = new HierarchyAnalyzer();
        hierarchyAnalyzer.LogToJson(transform, @"C:\temp\logs\explosion.json");
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        var t1 = 1f + 1f * time;
        var t2 = 1f + 2f * time;
        var t4 = 1f + 4f * time;
        var t6 = 1f + 6f * time;
        var t7 = 1f + 7f * time;
        var t8 = 1f + 8f * time;
        var t10 = 1f + 10f * time;
        var t12 = 1f + 12f * time;
        var t14 = 1f + 14f * time;

        fireRing2.transform.localScale = M.V3(t6);
        fireRing0.transform.localScale = M.V3(t8, t4, t8);
        fireRing1.transform.localScale = M.V3(t10, t4, t10);

        pressureRing0.transform.localScale = M.V3(t14,1f, t14);
        pressureRing1.transform.localScale = M.V3(t12,t2, t12);

        if (time > 8)
            Destroy(gameObject);
    }
}
