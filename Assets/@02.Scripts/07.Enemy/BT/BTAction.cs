using System;

public class BTAction : BTNode
{
    private Action _action;
    public BTAction(Action action) => _action = action;
    public override bool Tick()
    {
        // 지정된 액션을 실행하고 항상 true를 반환합니다.
        _action();
        return true;
    }
}