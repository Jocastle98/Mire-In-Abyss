namespace GameEnums
{
    public enum GameState
    {
        MainMenu,   // 메인 메뉴 화면
        Gameplay,   // 플레이어 조작 중
        UI,         // Pause 메인 메뉴, Gameplay 화면 중 모두 될 수 있음
        GameplayPause,       
    }
}

namespace PlayerEnums
{
    public enum PlayerState
    {
        None,
        Idle,
        Move,
        Jump,
        Fall,
        Land,
        Roll,
        Attack,
        Defend,
        Parry,
        Dash,
        Skill_1,
        Skill_2,
        Skill_3,
        Skill_4,
        Interaction,
        Stun,
        Freeze,
        Dead
    }

    public enum StatusEffect
    {
        None,
        Stun,
        Freeze,
        Burn,
        Poison,
        Bleed
    }

    public enum SlashEffectType
    {
        RightToLeft,
        LeftToRight,
        TopToBottom,
    }

    public enum SkillType
    {
        DefaultAttack,
        Parry,
        Defend,
        Sprint,
        Roll,
        Dash,
        ProjectileFire,
        Skill1,
        Skill2,
        Skill3,
        Skill4,
    }
}

namespace ItemEnums
{
    public enum ItemTier
    {
        Common,
        Special,
        Epic
    }
}

namespace EnemyEnums
{
    public enum EnemyType
    {
        Common,
        Elite,
        Boss
    }

    public enum EnemySubType
    {
        MeleeSkeleton,
        RangerSkeleton,
        Golem,
        Dragon,
        Other
    }
}

namespace QuestEnums
{
    public enum QuestState
    {
        Inactive,   // 퀘스트 미수락
        Active,     // 퀘스트 진행중
        Completed,  // 퀘스트 조건 완료
        Rewarded     // 퀘스트 보상 수령
    }
}

namespace SceneEnums
{
    public enum GameScene
    {
        MainMenu,
        Town,
        Abyss,
    }
}

namespace UIEnums
{
    public enum SpriteType
    {
        Item,
        Skill,
        Buff,
    }
}

namespace UIPanelEnums
{
    public enum UIPanelType
    {
        SoulStoneShop,
        QuestBoard,
        EnterPortal,
        Setting,
        Codex,
        EscGroup,
        EscGroup_Item, // 하드코딩, Item탭으로 시작하는 EscGroup 패널
        Credit
    }
}

namespace UIHUDEnums
{
    public enum MiniMapIconType
    {
        Player,
        Enemy,
        Boss,
        Shop,
        Portal,
        QuestBoard,
        SoulStoneShop, // Upgrade
    }

    public enum ProgressBarImageType
    {
        Rect,
        RoundedRect,
    }
}

namespace BattleAreaEnums
{
    public enum DungeonCellType
    {
        None,
        Room,
        Corridor,
        Entrance,
    }

    public enum DungeonRoomType
    {
        None,
        SafeRoom,
        MonsterRoom,
        BossRoom,
        EventRoom,
        ShopRoom,
    }
    
    public enum SpawnType
    {
        None,
        Mage,
        Ranger,
        Rogue,
        Warrior,
        Boss,
    }
}

#region 사운드 

namespace AudioEnums
{
    public enum EBgmType { Intro = 0, Town = 1, Field = 2, Dungeon = 3 }

    public enum ESfxType
    {
        FootstepEffect,
        GruntVoice,
        LandVoice,
        LandEffect,
        AttackVoice,
        SwordSwingEffect,
        EnemyHitEffect,
        PlayerHitVoice,
        PlayerHitEffect,
        ShieldBlockEffect,
        StunVoice,
        DeathVoice,
        SkillVoice,
        ProjectileFire,
        Skill1Effect,
        Skill2Effect,
        Skill3Effect,
        Skill4Effect,
        InteractionVoice,
    }

    public enum ExSfxType
    {
        SkeletonHit =0,
        SkeletonVoice =1,
        SkeletonDie =2,
        MeleeAttack =3,
        ArrowStart = 4,
        DragonHit = 5,
        DragonVoice = 6,
        DragonDie =7,
        DragonBreathStart = 8,
        DragonBreath = 9,
        DragonFireBall =10,
        DragonTail =11,
        DragonFly =12,
        GolemHit = 13,
        GolemTrace =14,
        GolemDie =15,
        GolemSwing =16,
        GolemImpact =17
    }
    public enum EUiType    { Open = 0, Close = 1, Click = 2  }
}

#endregion
