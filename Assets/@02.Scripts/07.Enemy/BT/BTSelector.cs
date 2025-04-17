using System.Collections.Generic;

public class BTSelector : BTNode
{
    private List<BTNode> _children;
    public BTSelector(params BTNode[] children) => _children = new List<BTNode>(children);
    public override bool Tick()
    {
        // 자식 노드를 순회하며 첫 번째로 true를 반환하는 노드를 찾습니다.
        foreach (var child in _children)
            if (child.Tick()) return true;
        return false;
    }
}