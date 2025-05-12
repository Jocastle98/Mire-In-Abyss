using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleAreaEnums;
using Unity.AI.Navigation;
using UnityEngine.AI;
using Random = UnityEngine.Random;


public class DungeonController : Abyss
{
    //BSP
    private int mCellSize;
    private int mCreateSize;
    private int mMinDungeonSize;

    DungeonNode mRootNode;
    DungeonCells[,] mDungeonCells;
    private int mCreateRoomChance = 80;

    List<DungeonNode> mLeafRooms = new List<DungeonNode>();

    //BattleArea
    private int mMinRoomCount;
    private int mDivideLineWidth;
    private int mEventRoomChance;
    private int mLevelDesign;
    private GameObject mPlayer;
    
    private SODungeonList mDungeonListSo;
    
    private GameObject mTileFolder;
    private GameObject mStanderFolder;
    private GameObject mDungeonFolder;
    
    private NavMeshSurface mNavMeshSurface;

    private void Start()
    {
        DungeonInit();
    }

    public override void BattleAreaClear()
    {
        ClearField();
        OnClearBattleArea.Invoke();
        Destroy(gameObject);
    }
    
    public override void SetPortal()
    {
        portal = Instantiate(AbyssManager.Instance.portalPrefab);
        AbyssMoveController moveCon = portal.GetComponent<AbyssMoveController>();
        moveCon.battleAreaMoveDelegate = BattleAreaClear;
        DeActivatePortal();
    }
    /// <summary>
    /// 보스를 잡고 맵을 이동할 때 필드의 모든 요소들을 정리
    /// </summary>
    public void ClearField()
    {
        ClearObjs(mTileFolder);
        ClearObjs(mStanderFolder);
        ClearObjs(mDungeonFolder);
    }

    //최대 방 크기, 최소 방 크기, 방 개수, 보물상자, 함정
    public void DungeonInit()
    {
        //클리어 델리게이트 부착
        OnClearBattleArea -= AbyssManager.Instance.BattleAreaClear;
        OnClearBattleArea += AbyssManager.Instance.BattleAreaClear;
        
        mCellSize = AbyssManager.Instance.cellSize;
        mMinDungeonSize = AbyssManager.Instance.minDungeonRoomSize;
        mDivideLineWidth = AbyssManager.Instance.divideLineWidth;
        mMinRoomCount = AbyssManager.Instance.minRoomCount;
        mLevelDesign = AbyssManager.Instance.levelDesign;
        mEventRoomChance = AbyssManager.Instance.eventRoomChance;
        mDungeonListSo = AbyssManager.Instance.dungeonListSO;
        mPlayer = AbyssManager.Instance.player;

        mTileFolder = new GameObject("TileFolder");
        mStanderFolder = new GameObject("StanderFolder");
        mDungeonFolder = new GameObject("DungeonFolder");
        mTileFolder.transform.parent = gameObject.transform;
        mStanderFolder.transform.parent = gameObject.transform;
        mDungeonFolder.transform.parent = gameObject.transform;

        SetPortal();
        DungeonCreat();
    }

    void DungeonCreat()
    {

        mCreateSize = mMinDungeonSize * (mMinRoomCount / 3 + 1);
        mMinRoomCount = Mathf.Max(2,
            Random.Range(mMinRoomCount - (int)(mLevelDesign * .1f), mMinRoomCount + (int)(mLevelDesign * .1f))) - 1;

        int whileCount = 0;
        while (mLeafRooms.Count <= mMinRoomCount)
        {
            ClearObjs(mTileFolder);
            ClearObjs(mStanderFolder);
            ClearObjs(mDungeonFolder);
            mLeafRooms.Clear();
            mDungeonCells = null;

            whileCount++;
            if (10 < whileCount)
            {
                mCreateRoomChance += 10;
                mCreateSize += mMinDungeonSize;
            }

            mDungeonCells = new DungeonCells[mCreateSize, mCreateSize];

            for (int i = 0; i < mCreateSize; i++)
            {
                for (int j = 0; j < mCreateSize; j++)
                {
                    mDungeonCells[i, j] = new DungeonCells();
                    mDungeonCells[i, j].SetDungeonCells(new Vector2Int(i, j),
                        mCellSize, mTileFolder, mStanderFolder, mDungeonFolder, mDungeonListSo);
                }
            }

            mRootNode = new DungeonNode(0, 0, mCreateSize, mCreateSize);
            DivideNode(mRootNode);
        }

        for (int i = 0; i < mLeafRooms.Count; i++)
        {
            SetRoom(mLeafRooms[i], i);
        }

        FindClosestRoom(mLeafRooms[0]);

        // NavMeshSurface 생성 및 설정
        mNavMeshSurface = gameObject.AddComponent<NavMeshSurface>();
        mNavMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

        // Collect Objects 방식을 Children으로 설정하면 하위 오브젝트만 수집
        mNavMeshSurface.collectObjects = CollectObjects.All;

        // Ground 레이어만 포함되도록 LayerMask 설정
        mNavMeshSurface.layerMask = LayerMask.GetMask("Ground");

        // NavMesh 빌드
        mNavMeshSurface.BuildNavMesh();
    }

    void DivideNode(DungeonNode node)
    {
        int randDivide = Random.Range(0, 2) == 0 ? 4 : 6;

        if (node.w - node.x < node.h - node.y)
        {
            int divideY = Mathf.FloorToInt(node.height * randDivide / 10f);
            if (divideY > mMinDungeonSize)
            {
                DungeonNode lNode = new DungeonNode(node.x, node.y, node.w, node.y + divideY - (1 + mDivideLineWidth));
                DungeonNode rNode = new DungeonNode(node.x, node.y + divideY + mDivideLineWidth,
                    node.w, node.h);
                node.SetChildrenNode(lNode, rNode);
                lNode.SetParent(node);
                rNode.SetParent(node);

                DivideNode(lNode);
                DivideNode(rNode);
            }
            else
            {
                SetNode(node);
            }
        }
        else
        {
            int divideX = Mathf.FloorToInt(node.width * randDivide / 10f);
            if (divideX > mMinDungeonSize)
            {
                DungeonNode lNode = new DungeonNode(node.x, node.y, node.x + divideX - (1 + mDivideLineWidth), node.h);
                DungeonNode rNode = new DungeonNode(node.x + divideX + mDivideLineWidth, node.y, node.w, node.h);
                node.SetChildrenNode(lNode, rNode);
                lNode.SetParent(node);
                rNode.SetParent(node);

                DivideNode(lNode);
                DivideNode(rNode);
            }
            else
            {
                SetNode(node);
            }
        }
    }

    void SetNode(DungeonNode node)
    {
        int setNodeChance = Random.Range(0, 100);

        if (node.width < mMinDungeonSize || node.height < mMinDungeonSize ||
            mCreateRoomChance < setNodeChance || mMinRoomCount < mLeafRooms.Count) return;

        mLeafRooms.Add(node);
    }

    void SetRoom(DungeonNode node, int count)
    {
        mDungeonCells[node.x, node.y].SetDungeonNode(node);
        if (count == 0)
        {
            mDungeonCells[node.x, node.y].SetDungeon(mDungeonCells, DungeonRoomType.SafeRoom);
            Vector3 safeRoomCenter = new Vector3(7, 1, 7) * .5f;
            mPlayer.transform.position = (new Vector3(node.x, 0, node.y) + safeRoomCenter) * mCellSize;
            
            node.SetCell(mDungeonCells[node.x, node.y]);
        }
        else if (mLeafRooms.Count - 1 == count)
        {
            mDungeonCells[node.x, node.y].SetPortal(portal);
            mDungeonCells[node.x, node.y].SetDungeon(mDungeonCells, DungeonRoomType.BossRoom);
        }
        else if (count == (int)(mLeafRooms.Count * 0.5f))
        {
            mDungeonCells[node.x, node.y].SetDungeon(mDungeonCells, DungeonRoomType.ShopRoom);
        }
        else if (Random.Range(0, 100) < mEventRoomChance)
        {
            mDungeonCells[node.x, node.y].SetDungeon(mDungeonCells, DungeonRoomType.EventRoom);
            mEventRoomChance = (int)(mEventRoomChance * 0.5f);
        }
        else
        {
            mDungeonCells[node.x, node.y].SetDungeon(mDungeonCells, DungeonRoomType.MonsterRoom);
        }
    }

    void FindClosestRoom(DungeonNode startRoom)
    {
        startRoom.IsVisited = true;

        Stack<DungeonNode> closestRoom = new Stack<DungeonNode>();
        float closestDValue = float.MaxValue;

        for (int i = 0; i < mLeafRooms.Count; i++)
        {
            if (mLeafRooms[i].IsVisited) continue;

            float newDistance = new Vector2(mLeafRooms[i].x - startRoom.x, mLeafRooms[i].y - startRoom.y).magnitude;

            if (newDistance < closestDValue)
            {
                closestDValue = newDistance;
                closestRoom.Push(mLeafRooms[i]);
            }
        }

        while (closestRoom.Count > 0)
        {
            DungeonNode expectRoom = closestRoom.Pop();

            if (ConnectRoom(startRoom, expectRoom, true))
            {
                if (expectRoom.cell.roomType == DungeonRoomType.BossRoom)
                {
                    expectRoom.IsVisited = true;
                    continue;
                }
                FindClosestRoom(expectRoom);
                return;
            }
        }
    }

    bool ConnectRoom(DungeonNode fromRoom, DungeonNode toRoom, bool isSet)
    {
        int dirX = toRoom.x - fromRoom.x;
        int dirY = toRoom.y - fromRoom.y;

        int indexY = dirY > 0 ? 0 : 2; // 위 : 아래
        int indexX = dirX > 0 ? 1 : 3; // 오른쪽 : 왼쪽

        Vector2 fromHorizontal = fromRoom.entrancePos[(indexX) % 4];
        Vector2 fromVertical = fromRoom.entrancePos[(indexY) % 4];
        Vector2 toHorizontal = toRoom.entrancePos[(indexX + 2) % 4];
        Vector2 toVertical = toRoom.entrancePos[(indexY + 2) % 4];

        // 수평 Z형
        if (Mathf.Abs(dirX) >= 3 && TryConnectZHorizontal(fromHorizontal, toHorizontal, fromRoom, toRoom, isSet))
            return true;

        // 수직 Z형
        if (Mathf.Abs(dirY) >= 3 && TryConnectZVertical(fromVertical, toVertical, fromRoom, toRoom, isSet))
            return true;

        // ㄴ형
        if (dirX != 0 && TryConnectLShape(fromHorizontal, toVertical, fromRoom, toRoom, isSet))
            return true;
        if (dirY != 0 && TryConnectLShape(fromVertical, toHorizontal, fromRoom, toRoom, isSet))
            return true;


        return false;
    }


    bool PathConnectable(List<DungeonCells> foundPath, DungeonNode fromRoom, DungeonNode toRoom, bool IsSet)
    {
        if (toRoom.cell.roomType == DungeonRoomType.BossRoom && toRoom.connectedRoom.Count > 0) return false;
        //이어지는 경로에 방이 있을경우 해당 방과 연결
        foreach (DungeonCells cell in foundPath)
        {
            if (cell.cellType != DungeonCellType.Room) continue;

            if (cell.dungeonNode != fromRoom && cell.dungeonNode != toRoom)
            {
                if (ConnectRoom(fromRoom, cell.dungeonNode, false) && ConnectRoom(cell.dungeonNode, toRoom, false))
                {
                    ConnectRoom(fromRoom, cell.dungeonNode, true);
                    ConnectRoom(cell.dungeonNode, toRoom, true);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        if (IsSet)
        {
            SetCorridorAndExpand(foundPath, fromRoom, toRoom);
        }

        return true;
    }

    void SetCorridorAndExpand(List<DungeonCells> corridorNominate, DungeonNode fromRoom, DungeonNode toRoom)
    {
        if (fromRoom.connectedRoom.Contains(toRoom) && toRoom.connectedRoom.Contains(fromRoom)) return;

        DungeonCells firstCell = corridorNominate[0];
        int lastdir = 0;

        //복도 세팅
        for (int k = 1; k < corridorNominate.Count; k++)
        {
            DungeonCells cell = corridorNominate[k];
            int x = cell.cellPos.x - 1;
            int y = cell.cellPos.y - 1;
            for (int i = x; i < x + 3; i++)
            {
                for (int j = y; j < y + 3; j++)
                {
                    if (mDungeonCells[i, j].cellType == DungeonCellType.None)
                    {
                        mDungeonCells[i, j].SetDungeonTileType(DungeonCellType.Corridor, mDungeonCells);
                    }
                }
            }

            if (lastdir != 0 && lastdir != CheckDir(corridorNominate[k - 1].cellPos, cell.cellPos))
            {
                firstCell.SetCorridorCollider(firstCell.cellPos, corridorNominate[k - 1].cellPos,true);
                firstCell = corridorNominate[k];
            }

            if (k == corridorNominate.Count - 1)
            {
                firstCell.SetCorridorCollider(firstCell.cellPos, corridorNominate[k].cellPos,false);
            }
            
            lastdir = CheckDir(firstCell.cellPos, cell.cellPos);
        }

        fromRoom.connectedRoom.Add(toRoom);
        toRoom.connectedRoom.Add(fromRoom);

    }

    int CheckDir(Vector2Int fromRoom, Vector2Int toRoom)
    {
        if (toRoom.x != fromRoom.x) return 1;
        if (toRoom.y != fromRoom.y) return 2;
        return 0; // 같은 위치
    }

    bool TryConnectZHorizontal(Vector2 from, Vector2 to, DungeonNode fromRoom, DungeonNode toRoom, bool isSet)
    {
        List<DungeonCells> corridor = new List<DungeonCells>();
        int stepX = (to.x > from.x) ? 1 : -1;
        int stepY = (to.y > from.y) ? 1 : -1;
        int midX = (int)from.x + ((int)((to.x - from.x) / 2));

        for (int x = (int)from.x + stepX; x != midX; x += stepX)
        {
            if (x < 0 || x >= mCreateSize) return false;
            corridor.Add(mDungeonCells[x, (int)from.y]);
        }

        for (int y = (int)from.y; y != (int)to.y; y += stepY)
            corridor.Add(mDungeonCells[midX, y]);

        for (int x = midX; x != (int)to.x; x += stepX)
            corridor.Add(mDungeonCells[x, (int)to.y]);

        return PathConnectable(corridor, fromRoom, toRoom, isSet);
    }

    bool TryConnectZVertical(Vector2 from, Vector2 to, DungeonNode fromRoom, DungeonNode toRoom, bool isSet)
    {
        List<DungeonCells> corridor = new List<DungeonCells>();
        int stepY = (to.y > from.y) ? 1 : -1;
        int stepX = (to.x > from.x) ? 1 : -1;
        int midY = (int)from.y + ((int)((to.y - from.y) / 2));

        for (int y = (int)from.y + stepY; y != midY; y += stepY)
            corridor.Add(mDungeonCells[(int)from.x, y]);

        for (int x = (int)from.x; x != (int)to.x; x += stepX)
            corridor.Add(mDungeonCells[x, midY]);

        for (int y = midY; y != (int)to.y; y += stepY)
            corridor.Add(mDungeonCells[(int)to.x, y]);

        return PathConnectable(corridor, fromRoom, toRoom, isSet);
    }

    bool TryConnectLShape(Vector2 from, Vector2 to, DungeonNode fromRoom, DungeonNode toRoom, bool isSet)
    {
        List<DungeonCells> corridor = new List<DungeonCells>();

        int dirX = (to.x > from.x) ? 1 : -1;
        int dirY = (to.y > from.y) ? 1 : -1;

        for (int x = (int)from.x + dirX; x != (int)to.x; x += dirX)
            corridor.Add(mDungeonCells[x, (int)from.y]);

        for (int y = (int)from.y; y != (int)to.y; y += dirY)
            corridor.Add(mDungeonCells[(int)to.x, y]);

        return PathConnectable(corridor, fromRoom, toRoom, isSet);
    }
}
