using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CaptureDepthTexture : MonoBehaviour
{
    public ScreenControl screen;
    private Camera myCamera;

    // Start is called before the first frame update
    void Start()
    {
        myCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void PostRender(ScriptableRenderContext _context, Camera _camera)
    {
        // Get Camera depth texture (Must be rendering to a used display OR a render texture)
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination);
        Texture _camDepthTexture = Shader.GetGlobalTexture("_CameraDepthTexture");
        screen.CaptureDepth(myCamera, _camDepthTexture);


    }
}
