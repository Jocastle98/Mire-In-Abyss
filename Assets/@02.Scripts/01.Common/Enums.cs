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
    public enum EBgmType { Intro = 0, Town = 1, Battle = 2, Dungeon = 3 }
    public enum ESfxType   { Arrow = 0, Death = 1 }
    public enum EUiType    { Open = 0, Close = 1, Click = 2  }
}

#endregion
