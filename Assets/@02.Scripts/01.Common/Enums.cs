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
        Hit,
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

namespace BattleAreaEnum
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