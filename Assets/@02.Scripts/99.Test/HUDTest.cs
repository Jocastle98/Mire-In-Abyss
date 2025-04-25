using System;
using Events.Abyss;
using UnityEngine;
using UnityEngine.Serialization;

public class HUDTest: MonoBehaviour
{
    public GameObject Enemy;
    public bool IsSpawned = false;
    public int DifficultyLevel = 1;
    public float DifficultyProgress = 0;

    DateTime mStartUtc;
    
    private void Start()
    {
        mStartUtc     = DateTime.UtcNow;
    }

    private void Update()
    {
        DifficultyProgress += Time.deltaTime * 0.3f;
        if (DifficultyProgress >= 1)
        {
            DifficultyProgress = 0;
            DifficultyLevel++;
            R3EventBus.Instance.Publish(new DifficultyChanged(DifficultyLevel));
        }
        
        R3EventBus.Instance.Publish(new DifficultyProgressed(DifficultyProgress));
        
        TimeSpan elapsed = DateTime.UtcNow - mStartUtc;
        R3EventBus.Instance.Publish(new PlayTimeChanged(elapsed));
    }

    public void OnToggleEnemyExist()
    {
        if (IsSpawned)
        {
            IsSpawned = false;
            R3EventBus.Instance.Publish(new EnemyDied(Enemy.transform));
        }
        else
        {
            IsSpawned = true;
            R3EventBus.Instance.Publish(new EnemySpawned(Enemy.transform));
        }
    }

    public void OnDifficultyLevelUp()
    {
        DifficultyLevel++;
        DifficultyProgress = 0;
        R3EventBus.Instance.Publish(new DifficultyChanged(DifficultyLevel));
        R3EventBus.Instance.Publish(new DifficultyProgressed(DifficultyProgress));
    }
}