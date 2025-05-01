using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonNode 
{
    public int x,y,w,h;
    public bool IsVisited =false;
    
    DungeonNode parent;
    DungeonNode lNode,rNode;
    
    //util
    public int width, height;
    public Vector2Int[] entrancePos = new Vector2Int[4]; //SO의 입구를 할당받음
    public List<DungeonNode> connectedRoom = new List<DungeonNode>();

    public DungeonNode(int x,int y,int w,int h)
    {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
        
        width = w - x;
        height = h - y;
    }

    public void SetChildrenNode(DungeonNode lNode,DungeonNode rNode)
    {
        this.lNode = lNode;
        this.rNode = rNode;
    }

    public void SetParent(DungeonNode parent)
    {
        this.parent = parent;
    }
    
}
