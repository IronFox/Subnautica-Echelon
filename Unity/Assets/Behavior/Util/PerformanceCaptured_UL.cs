public abstract class PerformanceCaptured_UL : PerformanceCaptured
{
    void Update()
    {
        perf.Update(() => P_Update());
    }

    void LateUpdate()
    {
        perf.LateUpdate(() => P_LateUpdate());
    }

    protected abstract void P_Update();
    protected abstract void P_LateUpdate();
}