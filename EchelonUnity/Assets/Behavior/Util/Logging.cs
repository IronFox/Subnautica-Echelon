using System;
using UnityEngine;

public class Logging
{
    public Logging(string tag)
    {
        Tag = tag;
    }

    public string Tag { get; }

    public void Write(string message)
    {
        Debug.Log(MakeLine(message));
    }

    public void Warn(string message)
    {
        Debug.LogWarning(MakeLine(message));
    }

    public void Fail(string message)
    {
        Debug.LogError(MakeLine(message));
    }

    public void Exception(Exception exception, GameObject context)
    {
        Debug.LogError(MakeLine(exception.Message));
        Debug.LogException(exception, context);
    }

    public void Exception(string whileDoing, Exception exception, GameObject context)
    {
        Debug.LogError(MakeLine(whileDoing + $": " + exception.Message));
        Debug.LogException(exception, context);
    }

    private string MakeLine(string message)
        => $"{DateTime.Now:HH:mm:ss.fff} [{Tag}] {message}";
}

public static class ULog
{
    private static Logging Log { get; } = new Logging($"EchelonU");

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
    private static Logging Log { get; } = new Logging($"Echelon");
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