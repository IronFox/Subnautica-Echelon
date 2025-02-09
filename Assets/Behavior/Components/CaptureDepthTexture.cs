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

    private void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext arg1, Camera camera)
    {
        //if (camera == myCamera)
        //{
        //    Debug.Log(myCamera.fieldOfView);
        //    myCamera.fieldOfView = 30;
        //    lastPosition = camera.transform.position;
        //    lastFov = camera.fieldOfView;
        //    lastMatrix = camera.previousViewProjectionMatrix;
        //}
    }

    private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext arg1, Camera camera)
    {
        if (camera == myCamera)
        {
            Debug.Log("end");
            screen.CaptureCameraProperties(myCamera);
            //Texture _camDepthTexture = Shader.GetGlobalTexture("_CameraDepthTexture");
            //screen.CaptureDepth(myCamera, _camDepthTexture);
        }
    }

    // Update is called once per frame
    void Update()
    {
        myCamera.Render();
        Texture _camDepthTexture = Shader.GetGlobalTexture("_CameraDepthTexture");
        screen.CaptureDepth(myCamera, _camDepthTexture);

    }


}
