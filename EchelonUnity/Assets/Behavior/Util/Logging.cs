using System;
using UnityEngine;

public class Logging
{
    public static ILogTarget Target { get; set; } = new UnityLogTarget();

    public Logging(string tag)
    {
        Tag = tag;
    }

    public string Tag { get; }

    public void Write(string message)
    {
        Target.Write(MakeLine(message));
    }

    public void Warn(string message)
    {
        Target.Warn(MakeLine(message));
    }

    public void Fail(string message)
    {
        Target.Fail(MakeLine(message));
    }

    public void Exception(Exception exception, GameObject context)
    {
        Target.Fail(MakeLine(exception.Message));
        Target.Exception(exception, context);
    }

    public void Exception(string whileDoing, Exception exception, GameObject context)
    {
        Target.Fail(MakeLine(whileDoing + $": " + exception.Message));
        Target.Exception(exception, context);
    }

    private string MakeLine(string message)
        => string.IsNullOrEmpty(Tag)
            ? $"{DateTime.Now:HH:mm:ss.fff} {message}"
            : $"{DateTime.Now:HH:mm:ss.fff} [{Tag}] {message}"
        ;
}


public interface ILogTarget
{
    void Write(string message);
    void Warn(string message);
    void Fail(string message);
    void Exception(Exception exception, GameObject context);
}

public class UnityLogTarget : ILogTarget
{
    public void Write(string message)
        => Debug.Log(message);
    public void Warn(string message)
        => Debug.LogWarning(message);
    public void Fail(string message)
        => Debug.LogError(message);
    public void Exception(Exception exception, GameObject context)
        => Debug.LogException(exception, context);
}

public static class ULog
{
    public static ILogTarget Target { get; set; } = new UnityLogTarget();

    private static Logging Log { get; } = new Logging($"Unity");

    public static void Write(string message)
        => Log.Write(message);
    public static void Warn(string message)
        => Log.Warn(message);
    public static void Fail(string message)
        => Log.Fail(message);
    public static void Exception(Exception exception, GameObject context)
        => Log.Exception(exception, context);
    public static void Exception(string whileDoing, Exception exception, GameObject context)
        => Log.Exception(whileDoing, exception, context);
}

public static class PLog
{
    private static Logging Log { get; } = new Logging(null);
    public static void Write(string message)
        => Log.Write(message);
    public static void Warn(string message)
        => Log.Warn(message);
    public static void Fail(string message)
        => Log.Fail(message);
    public static void Exception(Exception exception, GameObject context)
        => Log.Exception(exception, context);
    public static void Exception(string whileDoing, Exception exception, GameObject context)
        => Log.Exception(whileDoing, exception, context);
}