using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbyssMoveController : MonoBehaviour
{
    public delegate void BattleAreaMoveDelegate();
    public BattleAreaMoveDelegate battleAreaMoveDelegate;

    public void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("Player", System.StringComparison.OrdinalIgnoreCase))
        {
            battleAreaMoveDelegate.Invoke();
        }
    }
}
