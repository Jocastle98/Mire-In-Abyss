namespace GameEnums
{
    public enum GameState
    {
        MainMenu,
        Gameplay,   // 플레이어 조작 중
        UI,         // Pause
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
        JumpVoice,
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
        Skill1Effect,
        Skill2Effect,
        Skill3Effect,
        Skill4Effect,
        InteractionVoice,
    }

    public enum EUiType    { Open = 0, Close = 1, Click = 2  }
}

#endregion
