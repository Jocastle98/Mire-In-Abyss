using System;
using Events.Abyss;
using UnityEngine;
using UnityEngine.Serialization;

public class MinimapTest: MonoBehaviour
{
    public GameObject Enemy;
    [FormerlySerializedAs("isSpawned")] public bool IsSpawned = false;

    public void OnToggleEnemyExist()
    {
        if (IsSpawned)
        {
            IsSpawned = false;
            R3EventBus.Instance.Publish(new PortalClosed(Enemy.transform));
        }
        else
        {
            IsSpawned = true;
            R3EventBus.Instance.Publish(new PortalSpawned(Enemy.transform));
        }
        // if (IsSpawned)
        // {
        //     IsSpawned = false;
        //     R3EventBus.Instance.Publish(new EnemyDied(Enemy.transform));
        // }
        // else
        // {
        //     IsSpawned = true;
        //     R3EventBus.Instance.Publish(new EnemySpawned(Enemy.transform));
        // }
    }
}