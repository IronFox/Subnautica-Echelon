public abstract class PerformanceCaptured_L: PerformanceCaptured
{
    void LateUpdate()
    {
        perf.LateUpdate(() => P_LateUpdate());
    }

    protected abstract void P_LateUpdate();
}