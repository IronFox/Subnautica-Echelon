using UnityEngine;

public interface IWeaponTriggerGuidance
{
    ITargetable MaintainedTarget { get; }
    int Mark { get; set; }
    void OnTriggerActiveOn(ITargetable liveTarget);
    void OnTriggerWasActivated(ITargetable liveTarget);
    void OnTriggerLost();
    void OnUpdate();
    void OnTriggerInactive();
    bool CanHitWithoutRotation(Vector3 position);
    bool CanHitWithRotation(Vector3 position);

}