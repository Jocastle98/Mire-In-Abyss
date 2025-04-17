using System;

public class BTCondition : BTNode
{
    private Func<bool> _condition;
    public BTCondition(Func<bool> condition) => _condition = condition;
    public override bool Tick()
    {
        return _condition();
    }
}