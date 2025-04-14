using System.Collections.Generic;
using UnityEngine;

public class RailgunLine : MonoBehaviour
{
    private readonly List<Segment> segments = new List<Segment>();
    public GameObject segmentPrefab;
    public GameObject hitPrefab;
    public Renderer cylinder;
    public float length = 500;
    public float radius = 0.1f;
    private Color color;
    private float distance;
    public float speedMetersPerSecond = 200;
    private int atSegment;
    public EchelonControl owner;
    public float damage = 2000;
    public int upgradeLevel = 1;
    private readonly HashSet<TargetAdapter> hit = new HashSet<TargetAdapter>();
    private float scale;
    private SoundAdapter shotSound;
    //public float radius = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        shotSound = GetComponent<SoundAdapter>();
        cylinder.transform.localPosition = M.V3(0, 0, length);
        scale = M.Asymptotic(upgradeLevel, 2) * 2f;
        shotSound.pitch = 1f - scale * 0.5f;
        shotSound.volume = scale;
        var r = radius * scale;
        cylinder.transform.localScale = M.V3(r, length, r);
        color = cylinder.materials[0].color;
        float len = 1;
        float offset = 0;
        while (offset < length)
        {
            segments.Add(new Segment(offset, len));
            //segment.radius = radius;
            offset += len;
            len *= 2;
        }
        cylinder.materials[0].SetFloat($"_FadeIn", 1);
        cylinder.materials[0].SetFloat($"_FadeOut", 200);
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

            var candidates = Physics.RaycastAll(instance.transform.position, instance.transform.forward, s.Length);
            foreach (var candidate in candidates)
            {
                if (candidate.transform.IsChildOf(owner.transform))
                    continue;

                var hitInstance = Instantiate(hitPrefab, transform);
                hitInstance.transform.position = candidate.point;
                hitInstance.transform.localEulerAngles = M.V3(90, 0, 0);
                hitInstance.transform.localScale = M.V3(scale);
                if (!candidate.rigidbody)
                    continue;


                var target = TargetAdapter.ResolveTarget(candidate.rigidbody.gameObject, candidate.rigidbody);

                if (target is null)
                    continue;
                if (!target.Rigidbody.isKinematic)
                    target.Rigidbody.AddForce(transform.forward * 10000f / Time.fixedDeltaTime);
                if (target.IsInvincible)
                {
                    continue;
                }
                if (hit.Add(target))
                {
                    target.DealDamage(candidate.point, damage, owner.gameObject);
                }
            }


            var segment = instance.GetComponent<RailgunSegment>();
            segment.length = s.Length;
            segment.transform.localScale = M.V3(scale, scale, 1);
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
