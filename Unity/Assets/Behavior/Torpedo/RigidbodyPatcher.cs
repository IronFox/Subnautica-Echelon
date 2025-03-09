using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyPatcher : MonoBehaviour
{
    public static Action<GameObject, Rigidbody> Patch { get; set; } = (go, rb) =>
    {
        ConsoleControl.Write($"Rigidbody default patcher called for {go}. Doing nothing");
    };

    private Rigidbody rb;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Patch(gameObject, rb);
    }

}



