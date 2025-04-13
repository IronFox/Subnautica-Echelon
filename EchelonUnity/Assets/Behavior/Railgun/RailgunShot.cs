using UnityEngine;

public class RailgunShot : MonoBehaviour
{
    public EchelonControl owner;
    public GameObject railgunLine;
    public RailgunCharge railgunCharge;

    public bool doContinue = true;
    public bool canFire = true;
    public float damage = 2000;
    private bool terminal = false;

    private RailgunLine line;
    public float speedMetersPerSecond = 800;

    // Start is called before the first frame update
    void Start()
    {
        railgunCharge.doCharge = doContinue;
    }

    public bool HasFired => terminal;
    public float SecondsAfterFired { get; set; }

    // Update is called once per frame
    void Update()
    {
        railgunCharge.doCharge = doContinue && !terminal;
        if (terminal)
            SecondsAfterFired += Time.deltaTime;
        if (!terminal && railgunCharge.EndReached && canFire)
        {
            if (!doContinue)
            {
                Destroy(gameObject);
                return;
            }
            terminal = true;
            railgunCharge.doCharge = false;

            var line = Instantiate(railgunLine, transform);
            line.transform.localScale = Vector3.one;
            line.transform.localPosition = Vector3.zero;
            line.transform.localRotation = Quaternion.identity;
            this.line = line.GetComponent<RailgunLine>();
            this.line.owner = owner;
            this.line.damage = damage;
            this.line.speedMetersPerSecond = speedMetersPerSecond;
            owner.GetComponent<Rigidbody>().AddForce(-transform.forward * 10000f / Time.fixedDeltaTime);
        }
        if (terminal && !line)
        {
            Destroy(gameObject);
        }

    }
}
