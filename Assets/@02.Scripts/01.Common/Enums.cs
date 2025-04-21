

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
        Hit,
        Dead
    }
}

namespace ItemEnums
{
    public enum ItemTier {Common, Special, Epic }
    public enum ValueType{Add, Mul, Fixed, Unique}
}