using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxFlightTime : MonoBehaviour
{
    public float maxLifetimeSeconds = 30;
    private float age;
    private Detonator det;
    public float currentFlightTime;
    // Start is called before the first frame update
    void Start()
    {
        det = GetComponent<Detonator>();
    }

    // Update is called once per frame
    void Update()
    {
        age += Time.deltaTime;
        currentFlightTime = age;
        if (age > maxLifetimeSeconds)
        {
            ConsoleControl.Write($"Maximum lifetime reached ({maxLifetimeSeconds}). Detonating");
            det.Detonate();
        }
    }
}
