using System;

public class BTAction : BTNode
{
    private Action _action;
    public BTAction(Action action) => _action = action;
    public override bool Tick()
    {
        _action();
        return true;
    }
}