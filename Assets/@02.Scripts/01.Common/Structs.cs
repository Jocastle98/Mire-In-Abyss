using System;
using SceneEnums;
using UIHUDEnums;
using UnityEngine;


namespace Events.Data
{
    public readonly struct Preloaded
    {
    }
}

namespace Events.Gameplay
{
    public readonly struct GameplayModeChanged
    {
        public readonly GameplayMode NewMode;
        public GameplayModeChanged(GameplayMode newMode) => NewMode = newMode;
    }
}

namespace AchievementStructs
{
    //TODO: 업적 정보 type과 업적 패널 연결 후 삭제
    // 업적 패널용 임시 업적 정보
    public readonly struct TempAchievementInfo
    {
        public readonly int Id;
        public readonly string Title;
        public readonly string Description;
        public readonly bool IsClear;
        public readonly DateTime ClearDate;

        public TempAchievementInfo(int id,
                                   string title,
                                   string desc,
                                   bool isClear,
                                   DateTime clearDate = default)
            => (Id, Title, Description, IsClear, ClearDate)
               = (id, title, desc, isClear, clearDate);
    }
}

namespace Events.Player
{
    public readonly struct PlayerGrounded
    {
        public readonly bool IsGrounded;
        public PlayerGrounded(bool isGrounded) => IsGrounded = isGrounded;
    }

    public readonly struct PlayerHpChanged
    {
        public readonly int Current;
        public readonly int Max;

        public PlayerHpChanged(int current, int max)
            => (Current, Max) = (current, max);
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
    public readonly struct BossHpChanged
    {
        public readonly int ID;
        public readonly string Name;
        public readonly string SubName;
        public readonly int MaxHp;
        public readonly int CurrentHp;
        public BossHpChanged(int id, string n, string subName, int hp, int curHp) => (ID, Name, SubName, MaxHp, CurrentHp) = (id, n, subName, hp, curHp);
    }
    public readonly struct BossDisengage    // 보스 이탈 or 처치
    {
        public readonly int ID;
        public BossDisengage(int id) => ID = id;
    }
    public readonly struct DamagePopup
    {
        public readonly Vector3 WorldPos;
        public readonly int Amount;
        public readonly Color Color;
        public DamagePopup(Vector3 pos, int amt, Color color = default)
            => (WorldPos, Amount, Color) = (pos, amt, color);
    }
    public readonly struct EnemyHpChanged   // 일반몹 체력 변경
    {
        public readonly int ID;
        public readonly int Current;
        public readonly int Max;
        public EnemyHpChanged(int id, int cur, int max)
            => (ID, Current, Max) = (id, cur, max);
    }

}

namespace Events.Player.Modules
{
    /* ─── Item ─── */
    public readonly struct ItemAdded
    {
        public readonly int ID;
        public readonly int AddedAmount;
        public readonly int Total;
        public ItemAdded(int id, int amt, int total) => (ID, AddedAmount, Total) = (id, amt, total);
    }
    public readonly struct ItemSubtracked
    {
        public readonly int ID;
        public readonly int RemovedAmount;
        public readonly int Total;
        public ItemSubtracked(int id, int amt, int total) => (ID, RemovedAmount, Total) = (id, amt, total);
    }

    /* ─── Buff ─── */
    public readonly struct BuffAdded
    {
        public readonly int ID;
        public readonly float Duration;
        public readonly bool IsDebuff;
        public BuffAdded(int id, float duration, bool isDebuff = false) => (ID, Duration, IsDebuff) = (id, duration, isDebuff);
    }
    public readonly struct BuffRefreshed
    {
        public readonly int ID;
        public readonly float NewRemain;
        public BuffRefreshed(int id, float newRemain) => (ID, NewRemain) = (id, newRemain);
    }
    public readonly struct BuffEnded
    {
        public readonly int ID;
        public BuffEnded(int id) => ID = id;
    }

    /* ─── Quest ─── */
    public readonly struct QuestAccepted
    {
        public readonly string ID;
        public QuestAccepted(string id) => ID = id;
    }
    public readonly struct QuestUpdated
    {
        public readonly string ID;
        public readonly int CurrentAmount;
        public QuestUpdated(string id, int cur) => (ID, CurrentAmount) = (id, cur);
    }
    public readonly struct QuestCompleted
    {
        public readonly string ID;
        public QuestCompleted(string id) => ID = id;
    }
    public readonly struct QuestRewarded // Remove와 같은 경우
    {
        public readonly string ID;
        public QuestRewarded(string id) => ID = id;
    }

    public readonly struct GoldAdded
    {
        public readonly int AddedAmount;
        public readonly int Total;
        public GoldAdded(int amount, int total) => (AddedAmount, Total) = (amount, total);
    }
    public readonly struct GoldSubTracked
    {
        public readonly int RemovedAmount;
        public readonly int Total;
        public GoldSubTracked(int amount, int total) => (RemovedAmount, Total) = (amount, total);
    }

    /* ─── Soul ─── */
    public readonly struct SoulAdded
    {
        public readonly int AddedAmount;
        public readonly int Total;
        public SoulAdded(int amount, int total) => (AddedAmount, Total) = (amount, total);
    }
    public readonly struct SoulSubTracked
    {
        public readonly int RemovedAmount;
        public readonly int Total;
        public SoulSubTracked(int amount, int total) => (RemovedAmount, Total) = (amount, total);
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
}

namespace Events.HUD
{
    public readonly struct ToastPopup
    {
        public readonly string Message;
        public readonly float Duration;
        public readonly Color Color;
        public ToastPopup(string message, float duration = 2f, Color color = default)
            => (Message, Duration, Color) = (message, duration, color);
    }

    public readonly struct EntitySpawned<T> where T : class
    {
        public readonly int ID;
        public readonly T Entity;
        public EntitySpawned(int id, T e) => (ID, Entity) = (id, e);
    }

    public readonly struct EntityDestroyed<T> where T : class
    {
        public readonly int ID;
        public readonly T Entity;
        public EntityDestroyed(int id, T e) => (ID, Entity) = (id, e);
    }
}

namespace Events.UI
{
    public readonly struct EnterInGameScene { }
    public readonly struct EnterAbyssScene { }
    public readonly struct EnterTownScene { }

    public readonly struct LastUIPopup { }
}