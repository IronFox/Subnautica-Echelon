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

public class BoardingListeners : ListenerSet<IBoardingListener>
{
    

    public BoardingListeners(HashSet<IBoardingListener> listeners):base(listeners)
    {}

    public static BoardingListeners Of(params Component[] origins)
    {
        return Make<BoardingListeners>(origins);
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

