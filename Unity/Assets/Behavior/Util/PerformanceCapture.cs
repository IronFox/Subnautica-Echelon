using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public readonly struct Performance
{
    public int Slot { get; }
    public Performance(MonoBehaviour owner)
    {
        Slot = PerformanceCapture.RegisterSlot(owner);
    }

    public Performance(Type ownerType)
    {
        Slot = PerformanceCapture.RegisterSlot(ownerType);
    }

    public static Performance Of<T>() where T : MonoBehaviour
    {
        return new Performance(typeof(T));
    }

    private void Capture(Action action, SampleType type)
    {
        DateTime started = DateTime.Now;
        try
        {
            action();
            PerformanceCapture.ReportCompleted(Slot, DateTime.Now - started, type, null);
        }
        catch (Exception ex)
        {
            PerformanceCapture.ReportCompleted(Slot, DateTime.Now - started, type, ex);
        }

    }

    public void Update(Action action)
    {
        Capture(action, SampleType.Update);
    }   
    public void FixedUpdate(Action action)
    {
        Capture(action, SampleType.FixedUpdate);
    }
    public void LateUpdate(Action action)
    {
        Capture(action, SampleType.LateUpdate);
    }

    public void OnCollisionEnter(Action action)
    {
        Capture(action, SampleType.OnCollisionEnter);
    }
}

internal enum SampleType
{
    Update,
    FixedUpdate,
    LateUpdate,
    OnCollisionEnter
}

internal readonly struct CapturedPerformance
{
    public readonly TimeSpan elapsed;
    public readonly Exception withException;
    public readonly SampleType type;

    public CapturedPerformance(TimeSpan elapsed, Exception withException, SampleType type)
    {
        this.elapsed = elapsed;
        this.withException = withException;
        this.type = type;
    }
}

internal class PerformanceSlot
{
    public int Slot { get; }
    public Type Type { get; }
    private bool disabled;

    private readonly List<CapturedPerformance> samples
        = new List<CapturedPerformance>();

    public PerformanceSlot(int slot, Type type)
    {
        if (type == null)
            throw new ArgumentNullException("type");
        Slot = slot;
        Type = type;
    }

    internal void ReportCompleted(TimeSpan elapsed, SampleType type, Exception ex)
    {
        if (disabled)
            return;
        if (samples.Count < 1000)
            samples.Add(new CapturedPerformance(elapsed, ex, type));
        else if (samples.Count >= 1000)
        {
            Debug.LogError($"Collected 1000 samples for {Type}.*(). Clearing captured. Disabling this channel");
            disabled = true;
            samples.Clear();
        }
    }

    internal AggregatedSlotSample AggregateAndReset()
    {
        var update = Aggregator.New();
        var lateUpdate = Aggregator.New();
        var fixedUpdate = Aggregator.New();
        var onCollisionEnter = Aggregator.New();
        var total = Aggregator.New();
        foreach (var sample in samples)
        {
            switch (sample.type)
            {
                case SampleType.Update:
                    update.Include(sample);
                    break;
                case SampleType.FixedUpdate:
                    fixedUpdate.Include(sample);
                    break;
                case SampleType.LateUpdate:
                    lateUpdate.Include(sample);
                    break;
                case SampleType.OnCollisionEnter:
                    onCollisionEnter.Include(sample);
                    break;
            }
            total.Include(sample);
        }

        samples.Clear();

        if (Type == null)
            throw new InvalidOperationException();

        return new AggregatedSlotSample(
            type: Type,
            update: update.Complete(),
            fixedUpdate: fixedUpdate.Complete(),
            lateUpdate: lateUpdate.Complete(),
            onCollisionEnter: onCollisionEnter.Complete(),
            total: total.Complete());
    }
}

internal struct Aggregator
{
    private double secondsSum;
    private double minSeconds;
    private double maxSeconds;
    private double secondsSquareSum;
    private int numFailed;
    private int total;

    public static Aggregator New()
    {
        var rs = new Aggregator();
        rs.minSeconds = TimeSpan.MaxValue.TotalSeconds-1;
        return rs;
    }

    internal void Include(TimeSpan elapsed, bool failed)
    {
        var seconds = elapsed.TotalSeconds;
        secondsSum += seconds;
        secondsSquareSum += seconds * seconds;
        minSeconds = Math.Min(minSeconds, seconds);
        maxSeconds = Math.Max(maxSeconds, seconds);
        if (failed)
            numFailed++;
        total++;

    }

    public AggregatedSample Complete()
    {
        return new AggregatedSample(
            numCalls: total,
            numFaulted: numFailed,
            timeSum: TimeSpan.FromSeconds(secondsSum),
            totalSecondsSquareSum: secondsSquareSum,
            timeMin: TimeSpan.FromSeconds(minSeconds),
            timeMax: TimeSpan.FromSeconds(maxSeconds));
    }

    internal void Include(CapturedPerformance sample)
    {
        Include(sample.elapsed, sample.withException != null);
    }
}

public readonly struct AggregatedSlotSample
{
    public Type Type { get; }
    public AggregatedSample Update { get; }
    public AggregatedSample FixedUpdate { get; }
    public AggregatedSample LateUpdate { get; }
    public AggregatedSample OnCollisionEnter { get; }
    public AggregatedSample Total { get; }

    public AggregatedSlotSample(
        Type type,
        AggregatedSample update,
        AggregatedSample fixedUpdate,
        AggregatedSample lateUpdate,
        AggregatedSample onCollisionEnter,
        AggregatedSample total)
    {
        Type = type;
        Update = update;
        FixedUpdate = fixedUpdate;
        LateUpdate = lateUpdate;
        OnCollisionEnter = onCollisionEnter;
        Total = total;
    }

    internal static AggregatedSlotSample WorseOf(AggregatedSlotSample a, AggregatedSlotSample b)
    {
        return a.Total.TimeSum > b.Total.TimeSum ? a : b;
    }
}

public readonly struct AggregatedSample
{
    public int NumCalls { get; }
    public TimeSpan TimeSum { get; }
    public double TotalSecondsSquareSum { get; }
    public TimeSpan TimeMin { get; }
    public TimeSpan TimeMax { get; }
    public int NumFaulted { get; }

    public AggregatedSample(
        int numCalls,
        int numFaulted,
        TimeSpan timeSum,
        double totalSecondsSquareSum,
        TimeSpan timeMin,
        TimeSpan timeMax)
    {
        NumCalls = numCalls;
        NumFaulted = numFaulted;
        TimeSum = timeSum;
        TotalSecondsSquareSum = totalSecondsSquareSum;
        TimeMin = timeMin;
        TimeMax = timeMax;
    }

    internal static AggregatedSample WorseOf(AggregatedSample a, AggregatedSample b)
    {
        if (a.TimeSum > b.TimeSum)
            return a;
        return b;
    }
}

public readonly struct AggregatedFrame
{
    public DateTime Captured { get; }
    public AggregatedSlotSample[] Samples { get; }

    public AggregatedFrame(DateTime captured, AggregatedSlotSample[] samples)
    {
        Captured = captured;
        Samples = samples;
    }
}

public class PerformanceAggregate
{
    private readonly Queue<AggregatedFrame> frames = new Queue<AggregatedFrame>();

    private AggregatedFrame newest;
    private AggregatedFrame newestDropped;
    private int numSamples;

    public AggregatedFrame NewestDropped
        => newestDropped;
    public AggregatedFrame Newest
        => newest;
    public IEnumerable<AggregatedFrame> Frames => frames;

    public AggregatedFrame GetWorst()
    {
        var samples = new AggregatedSlotSample[numSamples];
        PerformanceCapture.InitEmpty(samples);
        foreach (var frame in frames)
        {
            for (int i = 0; i < frame.Samples.Length; i++)
            {
                samples[i] = AggregatedSlotSample.WorseOf(
                    frame.Samples[i],
                    samples[i]);
            }
        }
        return new AggregatedFrame(DateTime.Now, samples);
    }

    internal void AddFrame(AggregatedFrame frame)
    {
        numSamples = Math.Max(numSamples,frame.Samples.Length);
        newest = frame;
        frames.Enqueue(frame);
        while (frames.Count > 0 && 
            DateTime.Now - frames.Peek().Captured > TimeSpan.FromSeconds(10))
            newestDropped = frames.Dequeue();
    }
}

public static class PerformanceCapture
{
    private static readonly List<PerformanceSlot> slots = new List<PerformanceSlot>();
    private static Dictionary<Type, int> TypeMap { get; }
        = new Dictionary<Type, int>();
    private static int idCounter;


    public static void AggregateAndReset(PerformanceAggregate target)
    {
        if (target == null)
        {
            Debug.LogError($"PerformanceCapture.AggregateAndReset(null)");
            return;
        }
        var frame = new AggregatedSlotSample[slots.Count];
        for (int i = 0; i < slots.Count; i++)
            frame [i] = slots[i].AggregateAndReset();
        target.AddFrame(new AggregatedFrame(DateTime.Now, frame));
    }

    public static int RegisterSlot(MonoBehaviour owner)
    {
        var t = owner.GetType();
        return RegisterSlot(t);
    }
    public static int RegisterSlot(Type t)
    {
        if (!TypeMap.TryGetValue(t, out var id))
        {
            id = ++idCounter;
            TypeMap[t] = id;
            slots.Add(new PerformanceSlot(id, t));
        }
        return id;
    }

    internal static void ReportCompleted(int slotId, TimeSpan elapsed, SampleType type, Exception ex)
    {
        try
        {
            slotId--;   //we returned 1+, so need to decrement here
            if (ex != null)
            {
                Debug.LogError($"Caught {ex} while executing {slots[slotId].Type}.{type}()");
                Debug.LogException(ex);
            }
            slots[slotId].ReportCompleted(elapsed, type, ex);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    internal static void InitEmpty(AggregatedSlotSample[] samples)
    {
        for (int i = 0; i < samples.Length; i++)
            samples[i] = new AggregatedSlotSample(slots[i].Type, default, default, default, default, default);
    }
}
