using UnityEngine;

public class OnCollideShakeCamera : MonoBehaviour
{
    public CameraShake cameraShake;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }



    public void OnControllerColliderHit(ControllerColliderHit hit)
    {

    }

    public void OnCollisionEnter(Collision collision)
    {
        cameraShake.SignalCollision(collision);
    }
}
