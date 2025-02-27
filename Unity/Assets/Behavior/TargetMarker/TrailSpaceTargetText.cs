using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class TrailSpaceTargetText : CommonTargetListener
{
    public RectTransform parentCanvas;
    public TextMeshProUGUI textMesh;
    // Start is called before the first frame update
    void Start()
    {
        
    }


    private Vector2 Project(Vector3 point)
    {
        var p = Camera.main.WorldToScreenPoint(point);
        p.x /= Screen.width;
        p.y /= Screen.height;
        return p;
    }

    // Update is called once per frame
    void Update()
    {
        if (AdapterTarget != null)
        {
            var screen = Project(Target.Position);
            screen -= M.V2(0.5f);
            var screen2 = Project(Target.Position + Camera.main.transform.right * TargetSize);
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
            textMesh.rectTransform.localPosition = M.V3(w * screen.x + myWidth/2 + offset , h * screen.y - myHeight/2 + voffset, 0);



            StringBuilder builder = new StringBuilder();
            var name = AdapterTarget.GameObject.name;
            if (name.EndsWith("(Clone)"))
                builder.Append(name.Substring(0, name.Length - 7));
            else
                builder.Append(name);
            builder.Append(" ")
                .Append(Mathf.Round(AdapterTarget.CurrentHealth))
                .Append(" / ")
                .Append(AdapterTarget.MaxHealth)
                .Append("\n");
            float range = (AdapterTarget.GameObject.transform.position - transform.parent. position).magnitude;
            builder.Append(Mathf.Round(range)).Append("m\n");
            float velocity = AdapterTarget.Rigidbody.velocity.magnitude;
            builder.Append(M.Round(velocity, 1)).Append("m/s\n");



            textMesh.text = builder.ToString();
            
            textMesh.enabled = true;
        }
        else
            textMesh.enabled = false;
    }
}
