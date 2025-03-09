using UnityEngine;

public abstract class PerformanceCaptured_UF : PerformanceCaptured
{
    void Update()
    {
        perf.Update(() => P_Update());
    }
    void FixedUpdate()
    {
        perf.FixedUpdate(() => P_FixedUpdate());
    }

    protected abstract void P_Update();
    protected abstract void P_FixedUpdate();
}
