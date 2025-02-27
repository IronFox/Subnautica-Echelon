using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Implementations of this interface are notified before and after (off)boarding has completed
/// </summary>
public interface IBoardingListener
{
    void SignalOnboardingBegin();
    void SignalOnboardingEnd();
    void SignalOffBoardingBegin();
    void SignalOffBoardingEnd();
}


public class CommonBoardingListener : MonoBehaviour, IBoardingListener
{
    public virtual void SignalOffBoardingBegin()
    {}

    public virtual void SignalOffBoardingEnd()
    {}

    public virtual void SignalOnboardingBegin()
    {}

    public virtual void SignalOnboardingEnd()
    {}
}

public class BoardingListeners
{
    public IBoardingListener[] Listeners { get; }

    public BoardingListeners(HashSet<IBoardingListener> listeners)
    {
        IBoardingListener[] listenersArray = new IBoardingListener[listeners.Count];
        listeners.CopyTo(listenersArray);
        Listeners = listenersArray;
    }

    public static BoardingListeners Off(params Component[] origins)
    {
        HashSet<IBoardingListener> listeners = new HashSet<IBoardingListener>();
        foreach (var o in origins)
        {
            var listenerArray = o.GetComponentsInChildren<IBoardingListener>();
            foreach (var listener in listenerArray)
                listeners.Add(listener);
        }
        
        return new BoardingListeners(listeners);
    }

    private void Do(string name, Action<IBoardingListener> action)
    {
        foreach (var listener in Listeners)
        {
            try
            {
                action(listener);
            }
            catch (Exception e)
            {
                ConsoleControl.WriteException(name+" on "+listener, e);
            }
        }
    }

    public void SignalOnboardingBegin()
        => Do(nameof(SignalOnboardingBegin), listener => listener.SignalOnboardingBegin());
    public void SignalOnboardingEnd()
        => Do(nameof(SignalOnboardingEnd), listener => listener.SignalOnboardingEnd());
    public void SignalOffBoardingBegin()
        => Do(nameof(SignalOffBoardingBegin), listener => listener.SignalOffBoardingBegin());
    public void SignalOffBoardingEnd()
        => Do(nameof(SignalOffBoardingEnd), listener => listener.SignalOffBoardingEnd());
}

