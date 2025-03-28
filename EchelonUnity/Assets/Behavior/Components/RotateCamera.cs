using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    public float rotationAxisX;
    public float rotationAxisY;
    public float maxDegreesPerSecond = 800;
    public float transitionSpeedMultiplier = 2;

    private LockedEuler current;
    private Transform transitionTarget;
    private bool transitioning;
    private float transitionProgress;

    // Start is called before the first frame update
    void Start()
    {
        current = LockedEuler.FromGlobal(transform);
    }

    // Update is called once per frame
    void Update()
    {
        current = current.RotateBy(-rotationAxisY, rotationAxisX, maxDegreesPerSecond * Time.deltaTime);
        if (!transitioning)
        {
            current.ApplyTo(transform);
        }
        else
        {
            transitionProgress += Time.deltaTime * transitionSpeedMultiplier;
            transitionProgress = Mathf.Clamp01(transitionProgress);
            //Debug.Log($"@{transitionProgress}");

            var interpolated = LockedEuler.Slerp(current, LockedEuler.FromGlobal(transitionTarget), transitionProgress);

            interpolated.ApplyTo(transform);
        }
    }


    public bool IsTransitionDone => transitionProgress >= 1;

    public void CopyOrientationFrom(Transform t)
    {
        current = LockedEuler.FromGlobal(t);
    }



    public void BeginTransitionTo(Transform t)
    {
        //Debug.Log($"Begin transition to {t}");
        transitionTarget = t;
        transitioning = true;
        transitionProgress = 0;
    }

    public void AbortTransition()
    {
        if (transitioning)
        {
            current = LockedEuler.FromGlobal(transform);
            //Debug.Log($"Aborting transition. Imported current as {current}");
        }
        transitionTarget = null;
        transitioning = false;
    }



}
