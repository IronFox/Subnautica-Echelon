using UnityEngine;

public class RailgunShot : MonoBehaviour
{
    public EchelonControl owner;
    public GameObject railgunLine;
    public RailgunCharge railgunCharge;

    public bool doContinue = true;
    public bool canFire = true;
    public float damage = 2000;
    public int upgradeLevel = 1;
    private bool terminal = false;

    private RailgunLine line;
    public RailgunLine Line => line;
    public float speedMetersPerSecond = 800;

    // Start is called before the first frame update
    void Start()
    {
        railgunCharge.doCharge = doContinue;
        railgunCharge.upgradeLevel = upgradeLevel;
    }

    public bool HasFired => terminal && line;
    public float SecondsAfterFired { get; set; }
    public bool IsCharging => railgunCharge.doCharge && !railgunCharge.EndReached;
    public bool IsDischarging => !railgunCharge.doCharge && !terminal;

    // Update is called once per frame
    void Update()
    {
        railgunCharge.doCharge = doContinue && !terminal;
        railgunCharge.upgradeLevel = upgradeLevel;
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
            this.line.upgradeLevel = upgradeLevel;
            this.line.speedMetersPerSecond = speedMetersPerSecond;

            var scale = M.Asymptotic(upgradeLevel, 2) * 2f;

            owner.GetComponent<Rigidbody>().AddForce(
                -transform.forward
                * (25000f / Time.fixedDeltaTime * scale)
                );
        }
        if (terminal && !line)
        {
            Destroy(gameObject);
        }

    }
}
