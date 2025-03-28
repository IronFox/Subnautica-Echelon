using System;
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
    

    public KeyCode boardKey = KeyCode.B;
    public KeyCode centerKey = KeyCode.C;
    public KeyCode outOfWaterKey = KeyCode.F;
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

        if (Input.GetKeyDown(outOfWaterKey))
        {
            subControl.outOfWater = !subControl.outOfWater;
        }

        if (Input.GetKeyDown(centerKey))
        {
            subControl.cameraCenterIsCockpit = !subControl.cameraCenterIsCockpit;
        }

        if (Input.GetKeyDown(boardKey))
        {
            ConsoleControl.Write(boardKey.ToString());
            if (!isOnboarded)
            {
                ConsoleControl.Write("Boarding");
                try
                {
                    preBoardingPosition = transform.position;
                    preBoardingEuler = LockedEuler.FromGlobal(transform);
                    preBoardingParent = transform.parent;
                    subControl.Localize(transform);
                    subControl.Onboard();
                }
                catch (Exception ex)
                {
                    ConsoleControl.WriteException("Onboarding failed", ex);
                }
                isOnboarded = true;
                ConsoleControl.Write("Boarded");
            }
            else
            {
                ConsoleControl.Write("Offboarding");
                subControl.Offboard();
                transform.parent = preBoardingParent;
                transform.position = preBoardingPosition;
                preBoardingEuler.ApplyTo(transform);
                isOnboarded = false;
                ConsoleControl.Write("Offboarded");
            }

        }
    }
}
