using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detonator : MonoBehaviour
{
    public GameObject explosionPrefab;
    public Rigidbody origin;
    public bool noExplosion;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Detonate()
    {
        if (!noExplosion && (origin == null || M.Distance(origin.position, transform.position) > ExplosionController.ExplosionRadius))
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
