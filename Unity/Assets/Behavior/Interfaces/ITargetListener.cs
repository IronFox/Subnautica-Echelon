using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implementations of this interface are notified when a new target is focuses
/// </summary>
public interface ITargetListener
{
    void SignalNewTarget(
        EchelonControl echelon,
        ReadOnlyTargetEnvironment environment,
        ITargetable mainTarget);
}


public abstract class CommonTargetListener : PerformanceCaptured_U, ITargetListener
{
    protected ITargetable MainTarget { get; private set; }
    protected AdapterTargetable MainAdapterTarget { get; private set; }
    protected ReadOnlyTargetEnvironment Environment { get; private set; }
    protected EchelonControl Echelon { get; private set; }

    public void SignalNewTarget(EchelonControl echelon, ReadOnlyTargetEnvironment environment, ITargetable mainTarget)
    {
        MainTarget = mainTarget;
        MainAdapterTarget = (mainTarget as AdapterTargetable);
        Environment = environment;
        Echelon = echelon;
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

    public void SignalNewTarget(EchelonControl echelon, ReadOnlyTargetEnvironment environment, ITargetable mainTarget)
        => Do(nameof(SignalNewTarget), listener => listener.SignalNewTarget(echelon, environment, mainTarget));
}


