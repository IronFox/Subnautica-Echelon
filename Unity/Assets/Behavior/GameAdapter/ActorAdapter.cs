using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ActorAdapter
{
    public static Func<GameObject, Vector3, bool> IsOutOfWater { get; set; } = (go, at) => false;

}
