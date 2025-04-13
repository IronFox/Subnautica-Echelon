using System;

internal class TorpedoTriggerGuidance : IWeaponTriggerGuidance
{
    public TorpedoTriggerGuidance(StatusConsole statusConsole, FirstPersonMarkers firstPersonMarkers, TorpedoLaunchControl leftLaunch, TorpedoLaunchControl rightLaunch)
    {
        StatusConsole = statusConsole;
        FirstPersonMarkers = firstPersonMarkers;
        LeftLaunch = leftLaunch;
        RightLaunch = rightLaunch;
    }
    private bool maintainTriggerUntilFired;
    private ITargetable maintainTarget;
    private bool firingLeft;

    public StatusConsole StatusConsole { get; }
    public FirstPersonMarkers FirstPersonMarkers { get; }
    public TorpedoLaunchControl LeftLaunch { get; }
    public TorpedoLaunchControl RightLaunch { get; }

    public ITargetable MaintainedTarget =>
        maintainTriggerUntilFired && maintainTarget?.Exists == true
            ? maintainTarget
            : null;

    public int Mark { get; set; }

    public void OnTriggerWasActivated(ITargetable liveTarget)
    {
        maintainTriggerUntilFired = true;
        maintainTarget = liveTarget;
    }

    public void OnTriggerActiveOn(ITargetable liveTarget)
    {
        maintainTarget = liveTarget;
    }

    public void OnTriggerLost()
    {
        maintainTarget = null;
        maintainTriggerUntilFired = false;
        LeftLaunch.FireWithTarget = null;
        RightLaunch.FireWithTarget = null;
    }

    public void OnUpdate()
    {
        var control = firingLeft ? LeftLaunch : RightLaunch;
        RightLaunch.torpedoTechLevel =
        LeftLaunch.torpedoTechLevel =
            Math.Max(0, Mark - 1);

        if (control.CycleProgress > control.CycleTime * 0.5f)
        {
            ULog.Write($"Switching tube");
            control.FireWithTarget = null;
            firingLeft = !firingLeft;
            maintainTriggerUntilFired = false;
            maintainTarget = null;
        }
        (firingLeft ? LeftLaunch : RightLaunch).FireWithTarget = Mark > 0 ? maintainTarget : null;

        bool doFire = maintainTarget != null;
        FirstPersonMarkers.firingLeft = doFire && firingLeft;
        FirstPersonMarkers.firingRight = doFire && !firingLeft;

        StatusConsole.Set(StatusProperty.LeftLauncherTarget, LeftLaunch.FireWithTarget);
        StatusConsole.Set(StatusProperty.RightLauncherTarget, RightLaunch.FireWithTarget);
        StatusConsole.Set(StatusProperty.TorpedoMark, Mark);

    }

    public void OnTriggerInactive()
    {
        if (!maintainTriggerUntilFired)
            maintainTarget = null;
    }
}