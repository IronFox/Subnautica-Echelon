public abstract class PerformanceCaptured_F: PerformanceCaptured
{
    void FixedUpdate()
    {
        perf.FixedUpdate(() => P_FixedUpdate());
    }

    protected abstract void P_FixedUpdate();
}