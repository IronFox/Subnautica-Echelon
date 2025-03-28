using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideIfModuleCoverClosed : MonoBehaviour
{
    internal void SignalCoverClosed()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
            renderer.enabled = false;
    }

    internal void SignalCoverOpening()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
            renderer.enabled = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
