using System.Collections.Generic;

public class BTSequence : BTNode
{
    private List<BTNode> _children;
    public BTSequence(params BTNode[] children) => _children = new List<BTNode>(children);
    public override bool Tick()
    {
        foreach (var child in _children)
            if (!child.Tick()) return false;
        return true;
    }
}