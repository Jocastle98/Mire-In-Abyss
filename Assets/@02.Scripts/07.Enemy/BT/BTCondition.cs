using System;

public class BTCondition : BTNode
{
    private Func<bool> _condition;
    public BTCondition(Func<bool> condition) => _condition = condition;
    public override bool Tick()
    {
        // 지정된 조건 함수의 반환값을 그대로 반환합니다.
        return _condition();
    }
}