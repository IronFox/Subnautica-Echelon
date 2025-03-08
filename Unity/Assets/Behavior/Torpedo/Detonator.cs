using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detonator : MonoBehaviour
{
    public GameObject explosionPrefab;
    public Rigidbody origin;
    public bool noExplosion;
    public int techLevel;

    public float ExplosionDamage => 2000f / 25 * Mathf.Pow(5, techLevel);
    public static float ExplosionRadiusAt(int techLevel) => 15f / 4 * Mathf.Pow(2, techLevel);
    public float ExplosionRadius => ExplosionRadiusAt(techLevel);


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Detonate()
    {
        if (!noExplosion && (origin == null || M.Distance(origin.position, transform.position) > ExplosionRadiusAt(techLevel)))
        {
            var instance = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            var ctrl = instance.GetComponent<ExplosionController>();
            ctrl.explosionDamage = ExplosionDamage;
            ctrl.explosionRadius = ExplosionRadius;
        }
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
