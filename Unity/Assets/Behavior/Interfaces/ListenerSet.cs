using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListenerSet<Listener>
{
    public Listener[] Listeners { get; }

    public ListenerSet(HashSet<Listener> listeners)
    {
        Listener[] listenersArray = new Listener[listeners.Count];
        listeners.CopyTo(listenersArray);
        Listeners = listenersArray;
    }

    public static D Make<D>(params Component[] origins) where D:ListenerSet<Listener>
    {
        HashSet<Listener> listeners = new HashSet<Listener>();
        foreach (var o in origins)
        {
            var listenerArray = o.GetComponentsInChildren<Listener>();
            foreach (var listener in listenerArray)
                listeners.Add(listener);
        }

        return (D)Activator.CreateInstance(typeof(D), listeners);
    }

    protected void Do(string name, Action<Listener> action)
    {
        foreach (var listener in Listeners)
        {
            try
            {
                action(listener);
            }
            catch (Exception e)
            {
                ConsoleControl.WriteException(name + " on " + listener, e);
            }
        }
    }
}
