using System;


namespace Events.Player
{
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

    public readonly struct SkillCooldown
    {
        public readonly float Ratio;
        public readonly int   Index;
        public SkillCooldown(float ratio, int index)
            => (Ratio, Index) = (ratio, index);
    }

    public readonly struct CurrencyChanged
    {
        public readonly long Gold;
        public readonly long Soul;
        public CurrencyChanged(long gold, long soul)
            => (Gold, Soul) = (gold, soul);
    }

    public readonly struct DifficultyTick
    {
        public readonly int      Level;
        public readonly TimeSpan Elapsed;
        public DifficultyTick(int level, TimeSpan elapsed)
            => (Level, Elapsed) = (level, elapsed);
    }

    public readonly struct BossEngaged
    {
        public readonly string Name;
        public readonly int    MaxHp;
        public BossEngaged(string name, int maxHp)
            => (Name, MaxHp) = (name, maxHp);
    }

    public readonly struct BossHpChanged
    {
        public readonly int Current;
        public BossHpChanged(int current) => Current = current;
    }
}