using System.Collections;
using System.Collections.Generic;
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


    void FixedUpdate()
    {
        if (target?.Equals(lastTarget) == true)
        {
            observedVelocity = target.InherentVelocity ?? (target.Position - lastPosition) / Time.fixedDeltaTime;
            lastPosition = target.Position;
        }
    }

        // Update is called once per frame
    void Update()
    {
        if (target?.Equals(lastTarget) == true)
        {
            //observedVelocity = target.InherentVelocity ?? (target.Position - lastPosition) / Time.deltaTime;
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
    bool Exists { get; }
}


public class TransformTargetable : ITargetable
{
    public Transform Transform { get; }

    public Vector3 Position => Transform.position;

    public Vector3? InherentVelocity => null;

    public Vector3 GlobalSize { get; } = Vector3.one;

    public bool Exists => Transform != null;

    public TransformTargetable(Transform transform)
    {
        Transform = transform;

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

    public override bool Equals(object obj)
    {
        return obj is TransformTargetable t && t.Transform == Transform;
    }

    public override int GetHashCode()
    {
        return -1190553613 + EqualityComparer<Transform>.Default.GetHashCode(Transform);
    }
}

public class RigidbodyTargetable : ITargetable
{
    public Rigidbody Rigidbody { get; }

    public Vector3 Position => Rigidbody.position;

    public Vector3? InherentVelocity => Exists ? Rigidbody.velocity : (Vector3 ? )null;

    public Vector3 GlobalSize { get; }

    public bool Exists => Rigidbody != null;

    public RigidbodyTargetable(Rigidbody rigidbody)
    {
        Rigidbody = rigidbody;

        var colliders = Rigidbody.transform.GetComponentsInChildren<Collider>();
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

    public override bool Equals(object obj)
    {
        return obj is RigidbodyTargetable t && t.Rigidbody == Rigidbody;
    }

    public override int GetHashCode()
    {
        return -1854442488 + EqualityComparer<Rigidbody>.Default.GetHashCode(Rigidbody);
    }
}