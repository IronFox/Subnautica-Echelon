using UnityEngine;

internal class RailgunTriggerGuidance : IWeaponTriggerGuidance
{
    public RailgunTriggerGuidance(StatusConsole statusConsole, FirstPersonMarkers firstPersonMarkers, Railgun railgun)
    {
        StatusConsole = statusConsole;
        FirstPersonMarkers = firstPersonMarkers;
        Railgun = railgun;
    }

    public StatusConsole StatusConsole { get; }
    public FirstPersonMarkers FirstPersonMarkers { get; }
    public Railgun Railgun { get; }
    private ITargetable target;
    public ITargetable MaintainedTarget => null;

    public int Mark { get; set; }

    public void OnTriggerActiveOn(ITargetable liveTarget)
    {
        target = liveTarget;
    }

    public void OnTriggerLost()
    {
        target = null;
    }

    public void OnTriggerWasActivated(ITargetable liveTarget)
    { }

    public void OnUpdate()
    {
        Railgun.FireWithTarget = Mark > 0 ? target : null;
        Railgun.damage = 2000 * Mathf.Pow(2, Mark);
        FirstPersonMarkers.firingRailgun = target != null;
        StatusConsole.Set(StatusProperty.RailgunMark, Mark);
    }

    public void OnTriggerInactive()
    {
        target = null;
    }
}