using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class TrailSpaceTargetText : CommonTargetListener
{
    public RectTransform parentCanvas;
    //public TextMeshProUGUI textMesh;
    public GameObject targetTextPrefab;
    // Start is called before the first frame update

    public TrailSpaceTargetText()
    {
        pool = new TargetPool<TextMeshProUGUI>(
            (t, immediately) => Destroy(t.gameObject),
            t =>
            {
                var instance = Instantiate(targetTextPrefab, transform);
                var textMesh = instance.GetComponent<TextMeshProUGUI>();
                return textMesh;
            });
    }


    void Start()
    {
        
    }

    [ColorUsage(true, true)]
    public Color primaryTextColor = Color.white;
    [ColorUsage(true, true)]
    public Color secondaryTextColor = Color.white;

    public static TextDisplay textDisplay = TextDisplay.All;

    private Vector2? Project(Camera camera, Vector3 point)
    {
        var p = camera.WorldToScreenPoint(point);
        if (p.z < 0)
            return null;
        p.x /= Screen.width;
        p.y /= Screen.height;
        return p;
    }

    private bool IsTarget(int gameObjectInstanceId)
    {
        switch (textDisplay)
        {
            case TextDisplay.All:
                return Environment.IsTarget(gameObjectInstanceId);
            case TextDisplay.Focused:
                return MainAdapterTarget.TargetAdapter.GameObjectInstanceId == gameObjectInstanceId;
            case TextDisplay.None:
                return false;
        }
        return false;
    }

    private IEnumerable<AdapterTargetable> Targets()
    {
        switch (textDisplay) {
            case TextDisplay.All:
                if (Environment != null)
                    foreach (var t in Environment.Targets)
                        yield return t;
                yield break;
            case TextDisplay.Focused:
                if (MainAdapterTarget != null)
                    yield return MainAdapterTarget;
                yield break;
            case TextDisplay.None:
                yield break;
            }
    }

    private readonly TargetPool<TextMeshProUGUI> pool;

    protected override void P_Update()
    {
        var camera = CameraUtil.GetCamera(nameof(TrailSpaceTargetText));
        if (camera == null)
            return;

        pool.FilterAndUpdate<(Vector2 Screen, Vector2 Screen2) >(Targets(), t =>
        {

            var screen = Project(camera, t.Position);
            var screen2 = Project(camera, t.Position + camera.transform.right * Echelon.SizeOf(t));
            if (screen is null || screen2 is null)
                return null;

            return (Screen: screen.Value, Screen2: screen2.Value);
        },
        (textMesh, s, t) =>
        {
            bool isPrimary = t.Equals(MainAdapterTarget);
            var screen = s.Screen;
            var screen2 = s.Screen2;
            screen -= M.V2(0.5f);
            screen2 -= M.V2(0.5f);
            float onScreenSize = (screen2.x - screen.x);

            float w = parentCanvas.rect.width;
            float h = parentCanvas.rect.height;

            float myWidth = w / 2;
            float myHeight = h / 2;

            float offset = onScreenSize * w / 2 + 10;
            float voffset = 45;

            textMesh.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, myWidth);
            textMesh.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, myHeight);
            textMesh.rectTransform.localPosition = M.V3(w * screen.x + myWidth / 2 + offset, h * screen.y - myHeight / 2 + voffset, 0);



            StringBuilder builder = new StringBuilder();
            var name = t.TargetAdapter.GameObject.name;
            if (name.EndsWith("(Clone)"))
                builder.Append(name.Substring(0, name.Length - 7));
            else
                builder.Append(name);
            builder.Append(" ")
                .Append(Mathf.Round(t.TargetAdapter.CurrentHealth))
                .Append(" / ")
                .Append(t.TargetAdapter.MaxHealth)
                .Append("\n");
            float range = (t.Position - Echelon.transform.position).magnitude;
            builder.Append(Mathf.Round(range)).Append("m\n");
            float velocity = t.TargetAdapter.Rigidbody.velocity.magnitude;
            builder.Append(M.Round(velocity, 1)).Append("m/s\n");


            textMesh.color = isPrimary ? primaryTextColor : secondaryTextColor;

            textMesh.text = builder.ToString();

            textMesh.enabled = true;

        });

    }
}

public enum TextDisplay
{
    None,
    Focused,
    All
}