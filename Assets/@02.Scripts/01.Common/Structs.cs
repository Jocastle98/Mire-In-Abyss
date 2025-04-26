using System;
using UIHUDEnums;
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

    public readonly struct CurrencyChanged
    {
        public readonly int Gold;
        public readonly int Soul;

        public CurrencyChanged(int gold, int soul)
            => (Gold, Soul) = (gold, soul);
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

namespace Events.Quest
{
    // TODO: 아래의 QuestInfo를 퀘스트 담당자가 작성한 타입으로 대체(여기 작성할 필요 없음)
    public readonly struct TempQuestInfo
    {
        public readonly int Id;
        public readonly string Title;
        public readonly string ShortDesc;
        public readonly QuestState State;

        public TempQuestInfo(int id, string title, string desc, QuestState state)
            => (Id, Title, ShortDesc, State) = (id, title, desc, state);
    }

    public readonly struct QuestAddedOrUpdated
    {
        public readonly TempQuestInfo Info;
        public QuestAddedOrUpdated(TempQuestInfo i) => Info = i;
    }

    public readonly struct QuestCompleted
    {
        public readonly int QuestId;
        public QuestCompleted(int id) => QuestId = id;
    }
    
    public readonly struct QuestRemoved
    {
        public readonly int QuestId;
        public QuestRemoved(int id) => QuestId = id;
    }
}