using UnityEngine;

public class Railgun : MonoBehaviour, IFirable
{
    public EchelonControl echelon;
    public GameObject shotPrefab;
    private RailgunShot shot;
    public CoverAnimation openCoverAnimation;
    public ITargetable FireWithTarget { get; set; }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var doFire = FireWithTarget?.Exists == true;
        if (doFire)
        {
            openCoverAnimation.animateForward = true;
            if (openCoverAnimation.IsAtEnd)
            {
                if (!shot)
                {
                    var instance = Instantiate(shotPrefab, transform);
                    instance.transform.localRotation = Quaternion.identity;
                    instance.transform.localScale = Vector3.one;
                    instance.transform.localPosition = Vector3.zero;
                    shot = instance.GetComponent<RailgunShot>();
                    shot.owner = echelon;
                }
            }
        }


        if (shot)
        {
            if (shot.HasFired)
            {
                shot.transform.SetParent(echelon.transform.parent);
            }
            shot.doContinue = doFire;
            if (shot.SecondsAfterFired >= 1)
                openCoverAnimation.animateForward = false;
        }
    }
}
