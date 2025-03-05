using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detonator : MonoBehaviour
{
    public GameObject explosionPrefab;
    public Rigidbody origin;
    public bool noExplosion;
    public int techLevel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Detonate()
    {
        if (!noExplosion && (origin == null || M.Distance(origin.position, transform.position) > ExplosionController.ExplosionRadiusAt(techLevel)))
        {
            var instance = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            var ctrl = instance.GetComponent<ExplosionController>();
            ctrl.techLevel = techLevel;
        }
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
