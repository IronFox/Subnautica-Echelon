using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorpedoLaunchControl : MonoBehaviour
{
    public GameObject torpedoPrefab;
    public float relativeExitVelocity=200;
    public Transform cover;
    public float secondsToOpenCover = 0.5f;
    public float secondsToFire = 1f;

    public float CycleTime => secondsToOpenCover * 2 + secondsToFire;

    public ITargetable fireWithTarget;

    public Vector2 coverRedactionXZ;
    private Vector2 originalCoverPosition;

    private float coverRedactionProgress;
    private float coverRecoveryProgress;
    private float fireRecoverProgress;
    private bool fired;
    private bool closing;
    public Rigidbody myBody;

    // Start is called before the first frame update
    void Start()
    {
        originalCoverPosition = cover.localPosition;
    }

    private void SetCover(float at)
    {

    }
    // Update is called once per frame
    void Update()
    {
        if (fired)
        {
            fireRecoverProgress += Time.deltaTime;
            Debug.Log("Waiting for fire recovery @" + fireRecoverProgress);
            if (fireRecoverProgress > secondsToFire)
            {
                fired = false;
                coverRecoveryProgress = 0;
                SetCover(1);
                closing = true;
            }

        }
        else if (closing)
        {
            coverRecoveryProgress += Time.deltaTime;
            Debug.Log("Closing again @" + coverRecoveryProgress);
            if (coverRecoveryProgress > secondsToOpenCover)
            {
                Debug.Log("Closed");
                closing = false;
                SetCover(0);
                coverRedactionProgress = 0;
            }
            else
                SetCover(1f - coverRecoveryProgress / secondsToOpenCover);
        }
        else if (fireWithTarget != null)
        {
            coverRedactionProgress += Time.deltaTime;
            Debug.Log("Opening @"+ coverRedactionProgress);
            if (coverRedactionProgress > secondsToOpenCover)
            {
                SetCover(1);
                Debug.Log("Firing");
                fired = true;

                var torpedo = Instantiate(torpedoPrefab, transform.position, transform.rotation);
                var rb = torpedo.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.velocity = myBody.GetPointVelocity(transform.position) + transform.forward * relativeExitVelocity;
                var tp = torpedo.GetComponent<TargetPredictor>();
                tp.target = fireWithTarget;

                var ctrl = torpedo.GetComponent<TorpedoControl>();
                ctrl.doNotCollideWith = myBody;

            }
            else
                SetCover(coverRedactionProgress / secondsToOpenCover);
        }
        else
        {
            if (coverRedactionProgress > 0)
            {
                coverRedactionProgress -= Time.deltaTime;
                coverRedactionProgress = M.Max(coverRedactionProgress, 0);
                Debug.Log("Closing @" + coverRedactionProgress);
                SetCover(coverRedactionProgress / secondsToOpenCover);
            }

        }

   }
}
