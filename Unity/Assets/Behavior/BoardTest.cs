using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardTest : MonoBehaviour
{
    private Vector3 preBoardingPosition;
    private LockedEuler preBoardingEuler;
    private Transform preBoardingParent;
    private bool isOnboarded;
    public EchelonControl subControl;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isOnboarded)
        {
            //Debug.Log(Input.GetAxis("Vertical"));
            transform.position += transform.forward * Input.GetAxis("Vertical") * 10* Time.deltaTime;
            transform.position += transform.right * Input.GetAxis("Horizontal") * 10 * Time.deltaTime;

            LockedEuler
                .FromLocal(transform)
                .RotateBy(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), Time.deltaTime * 800)
                .ApplyTo(transform);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            subControl.outOfWater = !subControl.outOfWater;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("B");
            if (!isOnboarded)
            {
                Debug.Log("Boarding");
                preBoardingPosition = transform.position;
                preBoardingEuler = LockedEuler.FromGlobal(transform);
                preBoardingParent = transform.parent;
                subControl.Onboard(transform);
                isOnboarded = true;
                Debug.Log("Boarded");
            }
            else
            {
                Debug.Log("Offboarding");
                subControl.Offboard();
                transform.parent = preBoardingParent;
                transform.position = preBoardingPosition;
                preBoardingEuler.ApplyTo(transform);
                isOnboarded = false;
                Debug.Log("Offboarded");
            }

        }
    }
}
