using UnityEngine;

public class PerformanceCaptured : MonoBehaviour
{
    protected readonly Performance perf;


    protected PerformanceCaptured()
    {
        perf = new Performance(this);
    }

}