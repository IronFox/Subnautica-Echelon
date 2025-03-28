
using System;
using System.Collections.Generic;

/// <summary>
/// Limited queue of timestamped samples
/// </summary>
public class TimeFrame<T>
{
    public TimeFrame(TimeSpan timeWindow)
    {
        TimeWindow = timeWindow;
    }


    public readonly struct Sample
    {
        public readonly T value;
        public readonly DateTime captured;

        public Sample(T value)
        {
            this.value = value;
            this.captured = DateTime.Now;
        }
    }

    public readonly struct InterpolationData
    {
        public readonly T older;
        public readonly T newer;
        public readonly float t;

        public InterpolationData(T older, T newer, float t)
        {
            this.older = older;
            this.newer = newer;
            this.t = t;
        }
    }

    private readonly Queue<Sample> samples = new Queue<Sample>();
    public Sample? LatestFlushed { get; private set; }
    public Sample? OldestOnRecord => samples.Count > 0 ? samples.Peek() : (Sample ? )null;
    public Sample? LatestAdded {get; private set; }
    public TimeSpan TimeWindow { get; }

    public void Add(T value)
    {
        var s = new Sample(value);
        LatestAdded = s;
        samples.Enqueue(s);
        Clean(s.captured);
    }

    private void Clean(DateTime now)
    {
        while (samples.Count > 0
            && (now - samples.Peek().captured) > TimeWindow)
        {
            LatestFlushed = samples.Dequeue();
        }
    }

    public InterpolationData? GetEdgeInterpolationData()
    {
        var at = DateTime.Now;
        Clean(at);
        var older = LatestFlushed;
        var newer = OldestOnRecord;
        if (older is null || newer is null)
            return null;
        at -= TimeWindow;
        var t = (at - older.Value.captured).TotalSeconds
              / (newer.Value.captured - older.Value.captured).TotalSeconds;
        return new InterpolationData(older.Value.value, newer.Value.value, (float)t);
    }
}

public class FloatTimeFrame : TimeFrame<float>
{
    public FloatTimeFrame(TimeSpan timeWindow) : base(timeWindow)
    {
    }

    public float? GetEdge()
    {
        var inter = GetEdgeInterpolationData();
        if (inter is null)
            return null;

        return M.Interpolate(inter.Value.older, inter.Value.newer, inter.Value.t);
    }
}
