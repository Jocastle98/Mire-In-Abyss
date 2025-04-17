using System.Collections.Generic;

public class BTSequence : BTNode
{
    private List<BTNode> _children;
    public BTSequence(params BTNode[] children) => _children = new List<BTNode>(children);
    public override bool Tick()
    {
        // 자식 노드를 순서대로 실행하여 하나라도 실패(false)이면 실패를 반환합니다.
        foreach (var child in _children)
            if (!child.Tick()) return false;
        return true;
    }
}