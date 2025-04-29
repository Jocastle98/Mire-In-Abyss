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

    public readonly struct GoldChanged
    {
        public readonly int Gold;
        public GoldChanged(int gold) => Gold = gold;
    }
    
    public readonly struct SoulChanged
    {
        public readonly int Soul;
        public SoulChanged(int soul) => Soul = soul;
    }

    public readonly struct BuffAdded
    {
        public readonly int ID;
        public readonly int Duration;
        public readonly bool IsDebuff;
        public BuffAdded(int id, int duration, bool isDebuff)
            => (ID, Duration, IsDebuff) = (id, duration, isDebuff);
    }
    public readonly struct PlayerExpChanged
    {
        public readonly int Current;
        public readonly int Max;
        public PlayerExpChanged(int current, int max)
            => (Current, Max) = (current, max);
    }
    public readonly struct PlayerLevelChanged
    {
        public readonly int Level;
        public PlayerLevelChanged(int level) => Level = level;
    }

    //TODO: 담당자의 스킬 Info로 대체
    public readonly struct TempSkillInfo
    {
        public readonly int ID;
        public readonly KeyCode KeyCode;
        public readonly float CooldownTime;
        public TempSkillInfo(int id, KeyCode keyCode, float cooldownTime)
            => (ID, KeyCode, CooldownTime) = (id, keyCode, cooldownTime);
    }

    public readonly struct SkillUsed
    {
        public readonly int ID;
        public SkillUsed(int id) => ID = id;
    }

    public readonly struct SkillUpdated
    {
        public readonly int ID;
        public readonly float CooldownTime;
        public readonly KeyCode KeyCode;
        public SkillUpdated(int id, float cooldownTime, KeyCode keyCode)
            => (ID, CooldownTime, KeyCode) = (id, cooldownTime, keyCode);
    }
}

namespace Events.Combat
{
    public readonly struct BossHpChanged     // 보스 전투 진입
    {
        public readonly int ID;
        public readonly string Name;
        public readonly string SubName;
        public readonly int    MaxHp;
        public readonly int    CurrentHp;
        public BossHpChanged(int id, string n, string subName, int hp, int curHp)=>(ID,Name,SubName,MaxHp,CurrentHp)=(id,n,subName,hp,curHp);
    }
    public readonly struct BossDisengage    // 보스 이탈 or 처치
    {
        public readonly int ID;
        public BossDisengage(int id)=>ID=id;
    }
    public readonly struct DamagePopup
    {
        public readonly Vector3 WorldPos;
        public readonly int     Amount;
        public readonly Color   Color;
        public DamagePopup(Vector3 pos,int amt, Color color = default)
            => (WorldPos,Amount,Color)=(pos,amt,color);
    }
    public readonly struct EnemyHpChanged
    {
        public readonly Transform Anchor;   // Enemy UI Anchor transform
        public readonly int Current;
        public readonly int Max;
        public EnemyHpChanged(Transform t,int cur,int max)
            => (Anchor,Current,Max)=(t,cur,max);
    }

}

namespace Events.Item
{
    public readonly struct ItemAdded
    {
        public readonly int ID;
        public readonly int Count;
        public ItemAdded(int id, int count = 1)
            => (ID, Count) = (id, count);
    }

    public readonly struct ItemSubTracked
    {
        public readonly int ID;
        public readonly int Count;
        public ItemSubTracked(int id, int count = 1)
            => (ID, Count) = (id, count);
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
        public readonly Transform UIAnchor;
        public EnemyDied(Transform transform, Transform uiAnchor) => (Transform, UIAnchor) = (transform, uiAnchor);
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

namespace Events.HUD
{
    // HUD안에서 buff slot -> Player Info UI로의 알림용 struct
    public readonly struct BuffEnded
    {
        public readonly int ID;
        public BuffEnded(int id) => ID = id;
    }

    public readonly struct ToastPopup
    {
        public readonly string Message;
        public readonly float Duration;
        public readonly Color Color;
        public ToastPopup(string message, float duration = 2f, Color color = default)
            => (Message, Duration, Color) = (message, duration, color);
    }
}