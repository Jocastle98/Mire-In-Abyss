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
    
}

namespace UIPanelEnums
{
    public enum UIPanelType
    {
        SoulStoneShop,
        QuestBoard,
        EnterPortal
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

    public enum QuestState
    {
        Active,
        Completed
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