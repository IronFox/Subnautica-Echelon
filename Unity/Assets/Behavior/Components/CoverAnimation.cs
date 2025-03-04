using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// if you see this and wander wtf I'm doing:
/// I couldn't figure out how to do animations in Unity properly,
/// so I settled for this quick solution
/// </summary>
public class CoverAnimation : MonoBehaviour
{

    public Transform[] keyPoints;
    public float[] keyTimes;
    public bool animateForward;
    private float progress = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public bool IsAtBeginning => progress <= 0;

    // Update is called once per frame
    void Update()
    {
        if (!animateForward)
        {
            if (progress <= 0)
                return;
            progress = Mathf.Max(0,progress - Time.deltaTime);
        }
        else
        {
            float end = keyTimes[keyTimes.Length - 1];
            if (progress >= end)
                return;
            progress = Mathf.Min(end, progress + Time.deltaTime);
        }

        for (int i = 0; i+1 < keyTimes.Length; i++)
        {
            var t0 = keyTimes[i];
            var t1 = keyTimes[i+1];
            var trans0 = keyPoints[i];
            var trans1 = keyPoints[i+1];
            if (progress >= t0 && progress < t1)
            {
                float relative = M.LinearStep(t0, t1, progress);
                transform.position = Vector3.Lerp(trans0.position, trans1.position, relative);
                transform.rotation = Quaternion.Slerp(trans0.rotation, trans1.rotation, relative);
                break;
            }
        }


    }
}
