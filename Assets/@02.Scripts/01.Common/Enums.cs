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
        Dead
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
}