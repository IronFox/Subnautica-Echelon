using UnityEngine;

public class FirstPersonMarkers : MonoBehaviour
{
    public bool show;
    public bool firingRailgun;
    public bool firingLeft;
    public bool firingRight;
    public bool overdriveActive;

    public Renderer leftTorpedo;
    public Renderer rightTorpedo;
    public Renderer overdriveLeft;
    public Renderer overdriveRight;

    private Renderer[] childRenderers;
    private Renderer check;
    private HealingLight[] hls;
    // Start is called before the first frame update
    void Start()
    {
        childRenderers = GetComponentsInChildren<Renderer>();
        check = childRenderers.Length > 0 ? childRenderers[0] : null;
        hls = GetComponentsInChildren<HealingLight>();
    }

    // Update is called once per frame
    void Update()
    {
        if (check && check.enabled != show)
        {
            foreach (var renderer in childRenderers)
                renderer.enabled = show;

            foreach (var hl in hls)
                hl.isEnabled = show;
        }


        leftTorpedo.enabled = show && firingLeft;
        rightTorpedo.enabled = show && firingRight;
        overdriveLeft.enabled = show && overdriveActive;
        overdriveRight.enabled = show && overdriveActive;
    }

    void LateUpdate()
    {
        if (show)
        {

            var t = CameraUtil.GetTransform(nameof(FirstPersonMarkers));
            if (t != null)
                transform.position = t.position;
        }
    }
}
