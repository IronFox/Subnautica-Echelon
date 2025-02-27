using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TargetAdapter
{
    public abstract Rigidbody Rigidbody { get; }
    public abstract GameObject GameObject { get; }
    public abstract float CurrentHealth { get; }
    public abstract float MaxHealth { get; }

    public abstract bool IsInvincible { get; }

    public abstract bool IsAlive { get; } //also checks existence
    public abstract void DealDamage(Vector3 origin, float damage, GameObject dealer);

    public override abstract string ToString();

    public static Func<GameObject, Rigidbody, TargetAdapter> ResolveTarget { get; set; }
        = (go,rb) => new CommonTargetAdapter(go,rb);
}


public class CommonTargetAdapter : TargetAdapter
{
    public override GameObject GameObject { get; }
    public override Rigidbody Rigidbody { get; }
    public int GameObjectInstanceId { get; }

    private float _currentHealth = 1000;
    public override float CurrentHealth => _currentHealth;

    public override float MaxHealth => 1000;

    public CommonTargetAdapter(GameObject source, Rigidbody rigidbody)
    {
        GameObject = source;
        Rigidbody = rigidbody;
        GameObjectInstanceId = source.GetInstanceID();

    }

    public override bool IsAlive => GameObject != null && _currentHealth > 0;

    public override bool IsInvincible => false;

    public override void DealDamage(Vector3 origin, float damage, GameObject dealer)
    {
        _currentHealth -= damage;
    }

    public override string ToString() => $"CommonTargetAdapter:{GameObject.name}";

    public override bool Equals(object obj)
    {
        return obj is CommonTargetAdapter adapter &&
               GameObjectInstanceId == adapter.GameObjectInstanceId;
    }

    public override int GetHashCode()
    {
        return 1732276549 + GameObjectInstanceId.GetHashCode();
    }
}
