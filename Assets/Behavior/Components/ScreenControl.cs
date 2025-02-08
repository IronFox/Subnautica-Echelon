using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ScreenControl : MonoBehaviour
{
    public Camera trailingColorCamera;
    public Transform rightRoomWall;
    public Transform leftRoomWall;
    public Transform roomCeiling;
    public Transform roomFloor;
    public Transform rearRoomWall;
    public Material screenMaterial;
    public Material copyMaterial;
    private int lastWidth = -1;
    private int lastHeight = -1;
    private RenderTexture colorTexture;
    private RenderTexture depthTexture;
    public float screenDistance = 0.5f;
    private Mesh screenQuad;

    private int originalCullingMask = -1;

    public bool isEnabled;

    // Start is called before the first frame update
    void Start()
    {
        screenQuad = new Mesh();
        screenQuad.vertices = new Vector3[4] {new Vector3(-1,-1,0), new Vector3(1, -1, 0), new Vector3(1, 1, 0), new Vector3(-1, 1, 0), };
        screenQuad.subMeshCount = 1;
        screenQuad.SetIndices(new int[] { 0, 1, 2, 0, 2, 3 }, MeshTopology.Triangles,0);
        screenQuad.uv = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
    }

    // Update is called once per frame
    void Update()
    {
        var size = Camera.main.pixelRect;

        int w = Camera.main.pixelWidth;
        int h = Camera.main.pixelHeight;
        if (isEnabled)
        {
            if (w != lastWidth || h != lastHeight)
            {
                if (colorTexture != null)
                {
                    Destroy(colorTexture);
                    Destroy(depthTexture);
                }
                Debug.Log($"Creating new textures at size {w}*{h}");
                lastWidth = w;
                lastHeight = h;
                colorTexture = new RenderTexture(new RenderTextureDescriptor(w, h, RenderTextureFormat.ARGBHalf, 24));
                depthTexture = new RenderTexture(new RenderTextureDescriptor(w, h, RenderTextureFormat.RFloat, 24));
                trailingColorCamera.depthTextureMode = DepthTextureMode.Depth;
                trailingColorCamera.targetTexture = colorTexture;
                //Camera.main.depthTextureMode = DepthTextureMode.Depth;
                trailingColorCamera.fieldOfView = Camera.main.fieldOfView;

                screenMaterial.SetTexture("_Color", colorTexture);
                screenMaterial.SetTexture("_Depth", depthTexture);
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
            if (!trailingColorCamera.enabled)
            {
                originalCullingMask = Camera.main.cullingMask;
                Debug.Log($"old culling mask: {Camera.main.cullingMask}");
                Camera.main.cullingMask = ~1;
                Debug.Log($"new culling mask: {Camera.main.cullingMask}");
            }
            trailingColorCamera.enabled = true;
        }
        else
        {
            trailingColorCamera.enabled = false;
            Camera.main.cullingMask = originalCullingMask;
        }


    }

    internal void CaptureDepth(Camera camera, Texture camDepthTexture)
    {
        using (CommandBuffer buffer = new CommandBuffer())
        {
            var materialProperties = new MaterialPropertyBlock();
            materialProperties.SetTexture("_Source", camDepthTexture);
            buffer.SetRenderTarget(depthTexture);
            buffer.DrawMesh(screenQuad, Matrix4x4.identity, copyMaterial, 0, -1, materialProperties);
            Graphics.ExecuteCommandBuffer(buffer);
        }
        //https://stackoverflow.com/a/58600831
        Matrix4x4 matrixCameraToWorld = camera.cameraToWorldMatrix;
        Matrix4x4 matrixProjectionInverse = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false).inverse;
        Matrix4x4 matrixHClipToWorld = matrixCameraToWorld * matrixProjectionInverse;

        screenMaterial.SetMatrix("_Unproject", matrixHClipToWorld);
        screenMaterial.SetMatrix("_UnprojectToView", matrixProjectionInverse);



        screenMaterial.SetVector("_CameraPosition", camera.transform.position);
    }
}
