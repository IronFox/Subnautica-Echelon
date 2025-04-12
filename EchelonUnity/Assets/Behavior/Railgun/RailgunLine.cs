using System.Collections.Generic;
using UnityEngine;

public class RailgunLine : MonoBehaviour
{
    private readonly List<Segment> segments = new List<Segment>();
    public GameObject segmentPrefab;
    public Renderer cylinder;
    public float length = 500;
    public float radius = 0.1f;
    private Color color;
    private float distance;
    public float speedMetersPerSecond = 200;
    public int atSegment;
    //public float radius = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        cylinder.transform.localPosition = M.V3(0, 0, length);
        cylinder.transform.localScale = M.V3(radius, length, radius);
        color = cylinder.materials[0].color;
        float len = 1;
        float offset = 0;
        while (offset + len < length)
        {
            segments.Add(new Segment(offset, len));
            //segment.radius = radius;
            offset += len;
            len *= 2;
        }
        cylinder.materials[0].SetFloat($"_FadeIn", 1);
        cylinder.materials[0].SetFloat($"_FadeOut", 80);
    }

    // Update is called once per frame
    void Update()
    {
        var m = cylinder.materials[0];

        distance += Time.deltaTime * speedMetersPerSecond;
        m.SetFloat("_Progress", distance);


        cylinder.materials[0] = m;

        while (atSegment < segments.Count && segments[atSegment].Offset <= distance)
        {
            var instance = GameObject.Instantiate(segmentPrefab, transform);
            instance.transform.localEulerAngles = Vector3.zero;
            instance.transform.localScale = Vector3.one;
            var s = segments[atSegment++];
            instance.transform.localPosition = M.V3(0, 0, s.Offset);
            var segment = instance.GetComponent<RailgunSegment>();
            segment.length = s.Length;
        }
        if (distance > length * 2)
        {
            Destroy(gameObject);
        }
    }
}


internal readonly struct Segment
{
    public float Offset { get; }
    public float Length { get; }

    public Segment(float offset, float length)
    {
        Offset = offset;
        Length = length;
    }
}
