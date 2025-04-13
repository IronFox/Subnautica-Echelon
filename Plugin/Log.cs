using System;
using System.Collections.Generic;
using UnityEngine;

namespace Subnautica_Echelon
{

    public static class Log
    {
        public static string PathOf(Transform t)
        {
            var parts = new List<string>();
            try
            {
                while (t != null)
                {
                    parts.Add($"{t.name}[{t.GetInstanceID()}]");
                    t = t.parent;
                }
            }
            catch (UnityException)  //odd, but okay, don't care
            { }
            parts.Reverse();
            return string.Join("/", parts);

        }
        public static string PathOf(Component c)
        {
            try
            {
                return PathOf(c.transform) + $":{c.name}[{c.GetInstanceID()}]({c.GetType()})";
            }
            catch (Exception)
            {
                try
                {
                    return c.name;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }
        public static void Write(string message)
        {
            PLog.Write(message);
        }
    }



    public class MyLogger
    {
        public Component Owner { get; }

        public enum Channel
        {
            One,
            Two,
            Three,
            Four,
            Five,
            Six,

            Count
        }

        private DateTime[] LastStamp { get; } = new DateTime[(int)Channel.Count];

        public MyLogger(Component owner)
        {
            Owner = owner;
            for (int i = 0; i < LastStamp.Length; i++)
                LastStamp[i] = DateTime.MinValue;
        }

        public void WriteLowFrequency(Channel channel, string msg)
        {
            DateTime now = DateTime.Now;
            if (now - LastStamp[(int)channel] < TimeSpan.FromMilliseconds(1000))
                return;
            LastStamp[(int)channel] = now;
            Write(msg);
        }
        public void Write(string msg)
        {
            Log.Write(Log.PathOf(Owner) + $": {msg}");
        }
    }

}