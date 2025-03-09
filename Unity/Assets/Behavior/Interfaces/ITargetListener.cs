using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implementations of this interface are notified when a new target is focuses
/// </summary>
public interface ITargetListener
{
    void SignalNewEnvironment(
        
        ReadOnlyTargetEnvironment environment);

    void SignalNewTarget(
        EchelonControl echelon
        );
}


public abstract class CommonTargetListener : PerformanceCaptured_U, ITargetListener
{
    protected ITargetable MainTarget { get; private set; }
    protected AdapterTargetable MainAdapterTarget { get; private set; }
    protected ReadOnlyTargetEnvironment Environment { get; private set; }
    protected EchelonControl Echelon { get; private set; }

    public void SignalNewEnvironment(ReadOnlyTargetEnvironment environment)
    {
        Environment = environment;
    }
    public void SignalNewTarget(EchelonControl echelon)
    {
        MainTarget = echelon.liveTarget;
        MainAdapterTarget = (echelon.liveTarget as AdapterTargetable);
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

    public void SignalNewEnvironment(ReadOnlyTargetEnvironment environment)
        => Do(nameof(SignalNewEnvironment), listener => listener.SignalNewEnvironment(environment));
    public void SignalNewTarget(
        EchelonControl echelon
        )
        => Do(nameof(SignalNewEnvironment), listener => listener.SignalNewTarget(echelon));
}


