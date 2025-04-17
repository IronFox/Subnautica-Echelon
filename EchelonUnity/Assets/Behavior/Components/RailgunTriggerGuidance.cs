using UnityEngine;

public class RailgunTriggerGuidance : IWeaponTriggerGuidance
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
    public float Mk1RotationAngleTolerance { get; set; } = 10;
    public static float Mk1Damage { get; set; } = 1700;
    public static float Mk2Damage { get; set; } = 4000;
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

    public bool IsCharging => Railgun.IsCharging;
    public bool IsDischarging => Railgun.IsDischarging;
    public void OnUpdate()
    {
        Railgun.holdFireOnBadAim = Mark > 1;
        Railgun.FireWithTarget = Mark > 0 ? target : null;
        switch (Mark)
        {
            case 1:
                Railgun.damage = Mk1Damage;
                break;
            case 2:
                Railgun.damage = Mk2Damage;
                break;
            default:
                Railgun.damage = 100;
                break;
        }
        Railgun.mark = Mark;
        FirstPersonMarkers.firingRailgun = target != null;
        StatusConsole.Set(StatusProperty.RailgunMark, Mark);
    }

    public void OnTriggerInactive()
    {
        target = null;
    }

    public bool CanHitWithoutRotation(Vector3 position)
        => Railgun.CanHitWithoutRotation(position);

    public bool CanHitWithRotation(Vector3 position)
        => Mark > 1 || /*CanHitWithoutRotation(position) || */Railgun.AngleError(position) < Mk1RotationAngleTolerance;
}