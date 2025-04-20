using System.Collections.Generic;

public class BTSelector : BTNode
{
    private List<BTNode> _children;
    public BTSelector(params BTNode[] children) => _children = new List<BTNode>(children);
    public override bool Tick()
    {
        foreach (var child in _children)
            if (child.Tick()) return true;
        return false;
    }
}