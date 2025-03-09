using UnityEngine;

public abstract class PerformanceCaptured_U : PerformanceCaptured
{
	void Update()
	{
		perf.Update(() => P_Update());
	}

	protected abstract void P_Update();
}
