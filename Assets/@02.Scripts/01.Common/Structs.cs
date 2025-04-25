using System;
using UnityEngine;


namespace Events.Player
{
    public readonly struct PlayerHpChanged
    {
        public readonly int Current;
        public readonly int Max;

        public PlayerHpChanged(int current, int max)
            => (Current, Max) = (current, max);
    }
}

namespace Events.Abyss
{
    public readonly struct PlayTimeChanged
    {
        public readonly TimeSpan Elapsed;
        public PlayTimeChanged(TimeSpan elapsed)
            => Elapsed = elapsed;
    }
    public readonly struct DifficultyProgressed
    {
        public readonly float DifficultyProgress;

        public DifficultyProgressed(float difficultyProgress)
            => (DifficultyProgress) = (difficultyProgress);
    }
    public readonly struct DifficultyChanged
    {
        public readonly int DifficultyLevel;
        public DifficultyChanged(int difficultyLevel)
            => DifficultyLevel = difficultyLevel;
    }

    #region Object Spawn Despawn

    /// 전투지역(어비스)에서의 적, 상점, 포탈 등이 소환, 소멸 이벤트
    public readonly struct EnemySpawned
    {
        public readonly Transform Transform;
        public EnemySpawned(Transform transform) => Transform = transform;
    }

    public readonly struct EnemyDied
    {
        public readonly Transform Transform;
        public EnemyDied(Transform transform) => Transform = transform;
    }

    public readonly struct BossSpawned
    {
        public readonly Transform Transform;
        public BossSpawned(Transform transform) => Transform = transform;
    }

    public readonly struct BossDied
    {
        public readonly Transform Transform;
        public BossDied(Transform transform) => Transform = transform;
    }

    public readonly struct ShopSpawned
    {
        public readonly Transform Transform;
        public ShopSpawned(Transform transform) => Transform = transform;
    }

    public readonly struct ShopClosed
    {
        public readonly Transform Transform;
        public ShopClosed(Transform transform) => Transform = transform;
    }

    public readonly struct PortalSpawned
    {
        public readonly Transform Transform;
        public PortalSpawned(Transform transform) => Transform = transform;
    }

    public readonly struct PortalClosed
    {
        public readonly Transform Transform;
        public PortalClosed(Transform transform) => Transform = transform;
    }

    #endregion Object Spawn Despawn
}