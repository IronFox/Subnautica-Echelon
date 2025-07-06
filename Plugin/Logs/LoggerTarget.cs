using BepInEx.Logging;
using Subnautica_Echelon.Util;
using System;
using UnityEngine;

namespace Subnautica_Echelon.Logs
{
    internal class LoggerTarget : ILogTarget
    {
        public LoggerTarget(ManualLogSource outLogger)
        {
            OutLogger = outLogger;
        }

        public ManualLogSource OutLogger { get; }

        public void Exception(Exception exception, GameObject context)
        {
            OutLogger.LogError($"Exception from {context.NiceName()}: {exception.Message}\n{exception.StackTrace}");
        }

        public void Fail(string message)
        {
            OutLogger.LogError(message);
        }

        public void Warn(string message)
        {
            OutLogger.LogWarning(message);
        }

        public void Write(string message)
        {
            OutLogger.LogInfo(message);
        }
    }
}
