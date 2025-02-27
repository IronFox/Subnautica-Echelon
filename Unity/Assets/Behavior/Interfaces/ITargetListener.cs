using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implementations of this interface are notified when a new target is focuses
/// </summary>
public interface ITargetListener
{
    void SignalNewTarget(ITargetable target, float targetSize);
}


public class CommonTargetListener : MonoBehaviour, ITargetListener
{
    protected ITargetable Target { get; private set; }
    protected TargetAdapter AdapterTarget { get; private set; }
    protected float TargetSize { get; private set; }
    public void SignalNewTarget(ITargetable target, float targetSize)
    {
        Target = target;
        AdapterTarget = (target as AdapterTargetable)?.TargetAdapter;
        TargetSize = targetSize;
    }
}


public class TargetListeners : ListenerSet<ITargetListener>
{


    public TargetListeners(HashSet<ITargetListener> listeners) : base(listeners)
    { }

    public static TargetListeners Of(params Component[] origins)
    {
        return Make<TargetListeners>(origins);
    }

    public void SignalNewTarget(ITargetable t, float targetSize)
        => Do(nameof(SignalNewTarget), listener => listener.SignalNewTarget(t, targetSize));
}


