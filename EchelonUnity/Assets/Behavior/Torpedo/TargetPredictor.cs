﻿using System.Collections.Generic;
using UnityEngine;

public class TargetPredictor : MonoBehaviour, ITargetPredictor
{
    public ITargetable target;
    private ITargetable lastTarget;
    private Vector3 lastPosition;
    private Vector3 observedVelocity;

    public LinearPrediction? CurentPrediction =>
        target != null && target.Exists ?
            (LinearPrediction?)new LinearPrediction(
                target.InherentVelocity ?? observedVelocity
                //Vector3.zero
                , target.Position)
            : null;


    // Start is called before the first frame update
    void Start()
    {

    }



    // Update is called once per frame
    void Update()
    {
        if (target?.Equals(lastTarget) == true && target.Exists)
        {
            if (Time.deltaTime > 0)
            {
                observedVelocity = target.InherentVelocity ?? (target.Position - lastPosition) / Time.deltaTime;
                lastPosition = target.Position;
            }
        }
        else
        {
            observedVelocity = target?.InherentVelocity ?? Vector3.zero;
            lastPosition = target?.Position ?? Vector3.zero;
            lastTarget = target;
        }
    }
}

public interface ITargetPredictor
{
    LinearPrediction? CurentPrediction { get; }
}




public readonly struct LinearPrediction
{
    public Vector3 Velocity { get; }
    public Vector3 Position { get; }

    public LinearPrediction(Vector3 velocity, Vector3 position)
    {
        Velocity = velocity;
        Position = position;
    }

    public Vector3 PredictLocation(float deltaT)
    {
        return Position + deltaT * Velocity;
    }
}

public interface ITargetable
{
    Vector3 GlobalSize { get; }
    Vector3 Position { get; }
    Vector3? InherentVelocity { get; }

    bool IsCriticalTarget { get; }
    bool Exists { get; }
    int GameObjectInstanceId { get; }
    bool Is(GameObject gameObject);
}


public class TransformTargetable : ITargetable
{
    public Transform Transform { get; }

    public Vector3 Position => Transform.position;

    public Vector3? InherentVelocity => null;

    public Vector3 GlobalSize { get; } = Vector3.one;

    public bool Exists => Transform != null;

    public int GameObjectInstanceId { get; }

    public bool IsCriticalTarget { get; }

    public TransformTargetable(Transform transform)
    {
        Transform = transform;
        GameObjectInstanceId = transform.gameObject.GetInstanceID();
        IsCriticalTarget = TargetScanner.IsCriticalTarget(transform.gameObject);

        var colliders = Transform.GetComponentsInChildren<Collider>();
        if (colliders.Length > 0)
        {
            Bounds b = new Bounds(Vector3.zero, M.V3(float.MinValue));
            foreach (var collider in colliders)
            {
                b.Encapsulate(collider.bounds);
            }
            GlobalSize = b.max - b.min;
        }
        else
            GlobalSize = Vector3.zero;
    }
    public override string ToString() => $"Transform Target {Transform.name}";

    public override bool Equals(object obj)
    {
        return obj is TransformTargetable t && t.Transform == Transform;
    }

    public override int GetHashCode()
    {
        return -1190553613 + EqualityComparer<Transform>.Default.GetHashCode(Transform);
    }

    public bool Is(GameObject gameObject)
        => Transform != null && Transform.gameObject == gameObject;
}

public class AdapterTargetable : ITargetable
{
    public TargetAdapter TargetAdapter { get; }

    public Vector3 GlobalSize { get; }
    public bool IsCriticalTarget { get; }

    public Vector3 Position => TargetAdapter.GameObject.transform.position;

    public Vector3? InherentVelocity => TargetAdapter.Rigidbody.velocity;

    public bool Exists => TargetAdapter.IsAlive;

    public int GameObjectInstanceId { get; }

    public AdapterTargetable(TargetAdapter targetAdapter)
    {
        TargetAdapter = targetAdapter;
        GameObjectInstanceId = targetAdapter.GameObjectInstanceId;
        GlobalSize = SizeFromRigidbody(targetAdapter.Rigidbody);
        IsCriticalTarget = targetAdapter.IsCriticalTarget;
    }

    public static Vector3 SizeFromRigidbody(Rigidbody rigidbody)
    {
        var colliders = rigidbody.transform.GetComponentsInChildren<Collider>();
        if (colliders.Length > 0)
        {
            Bounds b = new Bounds(Vector3.zero, M.V3(float.MinValue));
            foreach (var collider in colliders)
            {
                b.Encapsulate(collider.bounds);
            }
            return b.max - b.min;
        }
        else
            return Vector3.zero;
    }

    public override string ToString() => $"AT Target {TargetAdapter.GameObject.name} (maxhealth={TargetAdapter.MaxHealth}, invincible={TargetAdapter.IsInvincible})";

    public override bool Equals(object obj)
    {
        return obj is AdapterTargetable targetable &&
               EqualityComparer<TargetAdapter>.Default.Equals(TargetAdapter, targetable.TargetAdapter);
    }

    public override int GetHashCode()
    {
        return -1904651305 + EqualityComparer<TargetAdapter>.Default.GetHashCode(TargetAdapter);
    }

    public bool Is(GameObject gameObject)
        => TargetAdapter.GameObject == gameObject;
}

public class RigidbodyTargetable : ITargetable
{
    public Rigidbody Rigidbody { get; }

    public Vector3 Position => Rigidbody.position;

    public Vector3? InherentVelocity => Exists ? Rigidbody.velocity : (Vector3?)null;

    public Vector3 GlobalSize { get; }
    public bool IsCriticalTarget { get; }

    public bool Exists => Rigidbody != null;

    public int GameObjectInstanceId { get; }

    public RigidbodyTargetable(Rigidbody rigidbody)
    {
        Rigidbody = rigidbody;
        GameObjectInstanceId = rigidbody.gameObject.GetInstanceID();
        GlobalSize = AdapterTargetable.SizeFromRigidbody(rigidbody);
        IsCriticalTarget = TargetScanner.IsCriticalTarget(rigidbody.gameObject);
    }

    public override string ToString() => $"RB Target {Rigidbody.name}";

    public override bool Equals(object obj)
    {
        return obj is RigidbodyTargetable t && t.Rigidbody == Rigidbody;
    }

    public override int GetHashCode()
    {
        return -1854442488 + EqualityComparer<Rigidbody>.Default.GetHashCode(Rigidbody);
    }

    public bool Is(GameObject gameObject)
        => Rigidbody != null && Rigidbody.gameObject == gameObject;
}

public class PositionTargetable : ITargetable
{
    public PositionTargetable(Vector3 position, float atDistance)
    {
        Position = position;
        AtDistance = atDistance;
    }

    public Vector3 Position { get; }
    public float AtDistance { get; }

    public Vector3 GlobalSize => Vector3.one;

    public Vector3? InherentVelocity => Vector3.zero;

    public override string ToString() => $"Position Target {Position}";

    public override bool Equals(object obj)
    {
        return obj is PositionTargetable targetable &&
               Position.Equals(targetable.Position);
    }

    public override int GetHashCode()
    {
        return -425505606 + Position.GetHashCode();
    }

    public bool Is(GameObject gameObject)
        => false;

    public bool Exists => true;

    public int GameObjectInstanceId { get; } = -1;

    public bool IsCriticalTarget => false;
}