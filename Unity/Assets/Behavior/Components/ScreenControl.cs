using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ScreenControl : MonoBehaviour
{
    public Camera trailingColorCamera;
    public Camera canvasCamera;
    public Transform rightRoomWall;
    public Transform leftRoomWall;
    public Transform roomCeiling;
    public Transform roomFloor;
    public Transform rearRoomWall;
    public Material screenMaterial;
    public Material copyMaterial;
    private CaptureDepthTexture captureDepthTexture;
    private int lastWidth = -1;
    private int lastHeight = -1;
    private RenderTexture canvasTexture;
    private RenderTexture colorTexture;
    private RenderTexture depthTexture;
    public float screenDistance = 0.5f;
    private Mesh screenQuad;
    private float enableVisualizationProgress;
    public float visualizationSpeedMultiplier = 2;
    private MeshRenderer screenRenderer;
    public Shader screenShader;

    private int originalCullingMask = -1;
    private Matrix4x4 lastUnprojectView;
    private Matrix4x4 lastUnproject;
    private Vector3 lastCameraPosition;

    public bool isEnabled;
    private bool? lastEnabled;

    // Start is called before the first frame update
    void Start()
    {
        screenQuad = new Mesh();
        screenQuad.vertices = new Vector3[4] {new Vector3(-1,-1,0), new Vector3(1, -1, 0), new Vector3(1, 1, 0), new Vector3(-1, 1, 0), };
        screenQuad.subMeshCount = 1;
        screenQuad.SetIndices(new int[] { 0, 1, 2, 0, 2, 3 }, MeshTopology.Triangles,0);
        screenQuad.uv = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };

        screenRenderer = GetComponent<MeshRenderer>();
        captureDepthTexture = trailingColorCamera.GetComponent<CaptureDepthTexture>();
    }

    void OnDestroy()
    {
        Destroy(colorTexture);
        Destroy(depthTexture);
        Destroy(canvasTexture);
    }

    // Update is called once per frame
    void Update()
    {

        screenRenderer.material = screenMaterial;
        if (!screenRenderer.enabled)
        {
            ConsoleControl.Write($"Screen renderer was disabled");
            screenRenderer.enabled = true;
        }


        if (lastEnabled != isEnabled)
        {
            lastEnabled = isEnabled;
            if (isEnabled)
                ConsoleControl.Write($"Screen on");
            else
                ConsoleControl.Write($"Screen off");
        }
        var size = Camera.main.pixelRect;

        int w = Camera.main.pixelWidth;
        int h = Camera.main.pixelHeight;

        if (screenShader != screenMaterial.shader)
        {
            ConsoleControl.Write($"Resetting material shader");
            screenMaterial.shader = screenShader;

            if (colorTexture != null)
            {
                screenMaterial.SetTexture("_Color", colorTexture);
                screenMaterial.SetTexture("_Depth", depthTexture);
                screenMaterial.SetTexture("_Canvas", canvasTexture);
            }
            screenMaterial.SetMatrix("_Unproject", lastUnproject);
            screenMaterial.SetMatrix("_UnprojectToView", lastUnprojectView);
            screenMaterial.SetVector("_CameraPosition", lastCameraPosition);
            screenMaterial.SetFloat("_EnabledProgress", enableVisualizationProgress);

            screenMaterial.SetFloat("_PixelSizeX", 1f / w);
            screenMaterial.SetFloat("_PixelSizeY", 1f / h);
            screenMaterial.SetFloat("_PixelAspect", (float)w / h);
        }


        if (w != lastWidth || h != lastHeight)
        {
            if (colorTexture != null)
            {
                Destroy(colorTexture);
                Destroy(depthTexture);
                Destroy(canvasTexture);
            }
            Debug.Log($"Creating new textures at size {w}*{h}");
            lastWidth = w;
            lastHeight = h;
            canvasTexture = new RenderTexture(new RenderTextureDescriptor(w, h, RenderTextureFormat.ARGBHalf, 16));
            canvasTexture.name = $"Echelon Screen Text Canvas Capture";
            colorTexture = new RenderTexture(new RenderTextureDescriptor(w, h, RenderTextureFormat.ARGBHalf, 24));
            colorTexture.name = $"Echelon Screen Capture";
            depthTexture = new RenderTexture(new RenderTextureDescriptor(w, h, RenderTextureFormat.RFloat, 16));
            depthTexture.name = $"Echelon Depth Capture";
            trailingColorCamera.depthTextureMode = DepthTextureMode.Depth;
            trailingColorCamera.targetTexture = colorTexture;
            trailingColorCamera.fieldOfView = Camera.main.fieldOfView;

            canvasCamera.targetTexture = colorTexture;// canvasTexture;
            canvasCamera.enabled = true;
            captureDepthTexture.doRender = true;

            
            screenMaterial.SetTexture("_Color", colorTexture);
            screenMaterial.SetTexture("_Depth", depthTexture);
            screenMaterial.SetTexture("_Canvas", canvasTexture);
            screenMaterial.SetFloat("_PixelSizeX", 1f / w);
            screenMaterial.SetFloat("_PixelSizeY", 1f / h);
            screenMaterial.SetFloat("_PixelAspect", (float)w / h);


            float screenHeightAtOne = 2 * Mathf.Tan(Camera.main.fieldOfView / 2 * Mathf.PI / 180);
            //Camera.main.fieldOfView / 90f;
            float screenHeight = screenHeightAtOne * screenDistance;

            var roomSizeY = screenHeight;// * 0.98f;    //make a little smaller to see room
            var roomSizeX = (float)w / h * roomSizeY;
            var wallFull = 0.05f;
            var wall = wallFull / 2;
            transform.localScale = new Vector3(roomSizeX, roomSizeY, 1);
            leftRoomWall.localScale = new Vector3(wallFull, roomSizeY, 1);
            leftRoomWall.localPosition = new Vector3(roomSizeX / 2 + wall, 0, 0);
            rightRoomWall.localScale = new Vector3(wallFull, roomSizeY, 1);
            rightRoomWall.localPosition = new Vector3(-roomSizeX / 2 - wall, 0, 0);
            roomCeiling.localScale = new Vector3(wallFull, roomSizeX + wallFull, 1);
            roomCeiling.localPosition = new Vector3(0, roomSizeY / 2 + wall, 0);
            roomFloor.localScale = new Vector3(wallFull, roomSizeX + wallFull, 1);
            roomFloor.localPosition = new Vector3(0, -roomSizeY / 2 - wall, 0);
            rearRoomWall.transform.localScale = new Vector3(wallFull, roomSizeY + wallFull, roomSizeX + wallFull);

        }


        if (isEnabled)
        {


            if (!captureDepthTexture.doRender)
            {
                enableVisualizationProgress = 0;
                originalCullingMask = Camera.main.cullingMask;
                trailingColorCamera.cullingMask = originalCullingMask;
                captureDepthTexture.doRender = true;
                //Camera.main.cullingMask = ~1;
                Camera.main.cullingMask |= 1 << 28; //make sure we see the screen
                ConsoleControl.Write($"Main camera culling mask changed: {originalCullingMask} -> {Camera.main.cullingMask}");
                screenMaterial.SetFloat("_EnabledProgress", enableVisualizationProgress);
            }
            else
            {
                if (enableVisualizationProgress < 2)    //some margin for error
                {
                    enableVisualizationProgress += Time.deltaTime* visualizationSpeedMultiplier;
                    screenMaterial.SetFloat("_EnabledProgress", enableVisualizationProgress);
                }
            }

            //screenMaterial.SetFloat("_EnabledProgress", progressOverride);
            //screenMaterial.SetFloat("_NoiseImpact", noiseImpact);
        }
        else
        {
            if (enableVisualizationProgress > 0)    //some margin for error
            {
                enableVisualizationProgress = Mathf.Min(enableVisualizationProgress, 1.05f);
                enableVisualizationProgress -= Time.deltaTime*visualizationSpeedMultiplier;
                screenMaterial.SetFloat("_EnabledProgress", enableVisualizationProgress);
            }


            if (captureDepthTexture.doRender)
            {
                captureDepthTexture.doRender = false;
                Camera.main.cullingMask = originalCullingMask;
                ConsoleControl.Write($"Main camera culling mask reverted");
            }
        }


    }

    public void CaptureCameraProperties(Camera camera)
    {
        //https://stackoverflow.com/a/58600831
        Matrix4x4 matrixCameraToWorld = camera.cameraToWorldMatrix;
        Matrix4x4 matrixProjectionInverse = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false).inverse;
        Matrix4x4 matrixHClipToWorld = matrixCameraToWorld * matrixProjectionInverse;

        lastUnproject = matrixHClipToWorld;
        lastUnprojectView = matrixProjectionInverse;
        screenMaterial.SetMatrix("_Unproject", matrixHClipToWorld);
        screenMaterial.SetMatrix("_UnprojectToView", matrixProjectionInverse);


        lastCameraPosition = camera.transform.position;
        screenMaterial.SetVector("_CameraPosition", camera.transform.position);
    }

    public void CaptureDepth(Camera camera, Texture camDepthTexture)
    {
        using (CommandBuffer buffer = new CommandBuffer())
        {
            var materialProperties = new MaterialPropertyBlock();
            materialProperties.SetTexture("_Source", camDepthTexture);
            buffer.SetRenderTarget(depthTexture);
            buffer.DrawMesh(screenQuad, Matrix4x4.identity, copyMaterial, 0, -1, materialProperties);
            Graphics.ExecuteCommandBuffer(buffer);
        }
        CaptureCameraProperties(camera);
    }
}
