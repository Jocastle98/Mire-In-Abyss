using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;
using BattleAreaEnums;

public class DungeonCells
{
    private DungeonNode mNode;
    private Vector2Int mCellPos;

    private int mCellSize;
    private DungeonCellType mCellType;
    private DungeonCellType[] mNeighbourCellType = new DungeonCellType[4];
    
    private GameObject mCurrentDungeonPrefab;
    private GameObject mCurrentTilePrefab;
    private GameObject[] mDungeonStanderPrefab = new GameObject[4];

    private SODungeon currentSODungeon;
    private SODungeonList mDungeonListSO;

    private DungeonRoomController roomCon;

    public GameObject portal;
    public GameObject tileFolder;
    public GameObject standerFolder;
    public GameObject dungeonFolder;

    public DungeonNode dungeonNode
    {
        get { return mNode; }
    }

    public Vector2Int cellPos
    {
        get { return mCellPos; }
    }

    public int cellSize
    {
        get { return mCellSize; }
    }

    public DungeonCellType cellType
    {
        get { return mCellType; }
    }

    public void SetDungeonCells(Vector2Int cellpos, int cellSize,
        GameObject tileFolder,GameObject standerFolder, GameObject dungeonFolder, SODungeonList dungeonListSO)
    {
        mCellPos = cellpos;
        mCellType = DungeonCellType.None;
        mCellSize = cellSize;
        this.tileFolder = tileFolder;
        this.standerFolder = standerFolder;
        this.dungeonFolder = dungeonFolder;
        mDungeonListSO = dungeonListSO;
    }

    public void SetDungeonNode(DungeonNode dungeonNode)
    {
        mNode = dungeonNode;
    }

    public void ClearDungeonCells()
    {
        mNode = null;
        mCellType = DungeonCellType.None;
    }

    public void SetDungeon(DungeonCells[,] cells, DungeonRoomType roomType)
    {
        switch (roomType)
        {
            case DungeonRoomType.SafeRoom:
                currentSODungeon = mDungeonListSO.safeDungeon;
                SetRoom(cells, mNode, currentSODungeon);
                break;
            case DungeonRoomType.MonsterRoom:
                currentSODungeon =
                    mDungeonListSO.monsterDungeonList[Random.Range(0, mDungeonListSO.monsterDungeonList.Count)];
                SetRoom(cells, mNode, currentSODungeon);
                break;
            case DungeonRoomType.BossRoom:
                currentSODungeon =
                    mDungeonListSO.bossDungeonList[Random.Range(0, mDungeonListSO.bossDungeonList.Count)];
                SetRoom(cells, mNode, currentSODungeon);
                break;
            case DungeonRoomType.EventRoom:
                currentSODungeon =
                    mDungeonListSO.eventDungeonList[Random.Range(0, mDungeonListSO.eventDungeonList.Count)];
                SetRoom(cells, mNode, currentSODungeon);
                break;
            case DungeonRoomType.ShopRoom:
                currentSODungeon = mDungeonListSO.shopRoom;
                SetRoom(cells, mNode, currentSODungeon);
                break;
        }
    }

    public void SetPortal(GameObject portal)
    {
        this.portal = portal;
    }

    public void SetDungeonTileType(DungeonCellType setType, DungeonCells[,] cells)
    {

        switch (setType)
        {
            case DungeonCellType.Corridor:
                //던전셀클래스에 자식이 생기면 자식팩토링
                SetCorridor(cells, mCellPos, mDungeonListSO.dungeonTileSO);
                break;
            case DungeonCellType.Entrance:
                SetEntrance(cells, mCellPos, mDungeonListSO.dungeonTileSO);
                break;
            case DungeonCellType.None:
                if (mCurrentTilePrefab != null) GameObject.Destroy(mCurrentTilePrefab);
                break;
        }
    }

    void PrefabUpdate(int dir, DungeonCells[,] cells)
    {
        switch (mCellType)
        {
            case DungeonCellType.Corridor:
                SwitchCorridorByNeighbour(dir, mCellPos, cells, mDungeonListSO.dungeonTileSO);
                break;
            case DungeonCellType.Entrance:
                SwitchEntranceByNeighbour(dir, mCellPos, cells, mDungeonListSO.dungeonTileSO);
                break;
            case DungeonCellType.None:
                if (mCurrentTilePrefab != null) GameObject.Destroy(mCurrentTilePrefab);
                break;
        }
    }

    #region 방

    void SetRoom(DungeonCells[,] cells, DungeonNode node, SODungeon soDungeon)
    {
        SetRoomPrefabs(cells, node, soDungeon);
        roomCon = cells[dungeonNode.x, dungeonNode.y].mCurrentDungeonPrefab.GetComponent<DungeonRoomController>();
        Vector3 roomCenter = (new Vector3(dungeonNode.x, 0, dungeonNode.y) +
                              (new Vector3(currentSODungeon.width, 0, currentSODungeon.height) * 0.5f)) * cellSize;
        roomCon.DungeonRoomInit(portal, roomCenter);
    }

    void SetRoomPrefabs(DungeonCells[,] cells, DungeonNode node, SODungeon soDungeon)
    {

        //노드의 크기에 던전크기가 들어가는지 확인
        if (node.width < soDungeon.width || node.height < soDungeon.height &&
            node.width < soDungeon.height || node.height < soDungeon.width) return;

        //오일러 방향 재구성
        int randEuler = 90 * Random.Range(0, 2) * 2;
        int dungeonEuler = (node.width < soDungeon.width) ? randEuler : randEuler + 90;
        Quaternion dungeonRotation = Quaternion.Euler(0, dungeonEuler, 0);

        //배열 방향 재구성
        int dungeonWidth = ((dungeonEuler / 90) % 2 == 0) ? soDungeon.width : soDungeon.height;
        int dungeonHeight = soDungeon.width == dungeonWidth ? soDungeon.height : soDungeon.width;

        //배열 배치후 던전의 중간지점 확인
        int middleX = node.x + dungeonWidth / 2;
        int middleY = node.y + dungeonHeight / 2;

        //배치된 배열들의 타입변경
        for (int x = node.x; x < node.x + dungeonWidth; x++)
        {
            for (int y = node.y; y < node.y + dungeonHeight; y++)
            {
                cells[x, y].mCellType = DungeonCellType.Room;
                cells[x, y].SetDungeonNode(node);
            }
        }

        //입구설정
        node.entrancePos[0] = new Vector2Int(middleX, node.y + dungeonHeight - 1);
        node.entrancePos[1] = new Vector2Int(node.x + dungeonWidth - 1, middleY);
        node.entrancePos[2] = new Vector2Int(middleX, node.y);
        node.entrancePos[3] = new Vector2Int(node.x, middleY);

        foreach (Vector2Int dungeon in node.entrancePos)
        {
            cells[dungeon.x, dungeon.y].SetDungeonTileType(DungeonCellType.Entrance, cells);
        }

        mCurrentDungeonPrefab = GameObject.Instantiate(soDungeon.dungeonPrefab,
            new Vector3(node.x * cellSize, 0, node.y * cellSize) +
            GetDungeonRotationOffset(dungeonEuler, soDungeon.width, soDungeon.height) * cellSize,
            dungeonRotation);

        mCurrentDungeonPrefab.transform.localScale *= cellSize;
        mCurrentDungeonPrefab.transform.SetParent(dungeonFolder.transform);

        //임시 확인용
        // Dungeon temptDungeon = mCurrentDungeonPrefab.GetComponent<Dungeon>();
        // temptDungeon.x = node.x;
        // temptDungeon.y = node.y;
        // temptDungeon.w = node.w;
        // temptDungeon.h = node.h;

        //mCells이 3차원이 될 때 높이값으로 쓰이게됨
        //int entranceCount = dungeon.entranceYPos.GetLength(0);

    }

    Vector3 GetDungeonRotationOffset(int rotation, int width, int height)
    {
        return rotation switch
        {
            0 => Vector3.zero,
            90 => new Vector3(0, 0, height),
            180 => new Vector3(width, 0, height),
            270 => new Vector3(width, 0, 0),
            _ => Vector3.zero
        };
    }


    #endregion


    #region 복도

    void SetFloorPrefabs(int cellSize, Vector2Int cellPos, SODungeonTile dungeonTileSo)
    {
        GameObject floorPrefab = dungeonTileSo.floorPrefabs[Random.Range(0, dungeonTileSo.floorPrefabs.Count)];
        mCurrentTilePrefab = GameObject.Instantiate(floorPrefab,
            new Vector3(cellPos.x * cellSize, 0, cellPos.y * cellSize),
            floorPrefab.transform.rotation);

        mCurrentTilePrefab.transform.localScale *= cellSize;
        mCurrentTilePrefab.transform.SetParent(tileFolder.transform);
    }

    void SetCorridor(DungeonCells[,] cells, Vector2Int cellPos, SODungeonTile dungeonTileSo)
    {
        mCellType = DungeonCellType.Corridor;

        SetFloorPrefabs(cells[cellPos.x, cellPos.y].cellSize, cellPos, dungeonTileSo);

        for (int i = 0; i < 4; i++)
        {
            SwitchCorridorByNeighbour(i, cellPos, cells, dungeonTileSo);

            DungeonCells dungeonOffset = SearchNeighboursCellType(i, cellPos, cells);
            dungeonOffset.PrefabUpdate((i + 2) % 4, cells);
        }
    }

    void SwitchCorridorByNeighbour(int dir, Vector2Int cellPos, DungeonCells[,] cells, SODungeonTile dungeonTileSo)
    {
        DungeonCells dungeonOffset = SearchNeighboursCellType(dir, cellPos, cells);

        if (dungeonOffset == null || dungeonOffset.mCellType == DungeonCellType.None)
        {
            mNeighbourCellType[dir] = DungeonCellType.None;
            if (mDungeonStanderPrefab[dir] == null)
            {
                mDungeonStanderPrefab[dir] = GetStanderPrefab(dir, dungeonTileSo.wallPrefabs);
            }

            return;
        }

        if (mNeighbourCellType[dir] == dungeonOffset.mCellType) return;

        mNeighbourCellType[dir] = dungeonOffset.mCellType;
        DestroyStander(dir);
    }

    #endregion


    #region 입구

    void SetEntrance(DungeonCells[,] cells, Vector2Int cellPos, SODungeonTile dungeonTileSo)
    {
        mCellType = DungeonCellType.Entrance;

        for (int i = 0; i < 4; i++)
        {
            SwitchEntranceByNeighbour(i, cellPos, cells, dungeonTileSo);

            DungeonCells dungeonOffset = SearchNeighboursCellType(i, cellPos, cells);
            if (dungeonOffset == null) continue;
            dungeonOffset.PrefabUpdate((i + 2) % 4, cells);
        }

        //비밀문일 경우 파괴가 가능
    }

    void SwitchEntranceByNeighbour(int dir, Vector2Int cellPos, DungeonCells[,] cells, SODungeonTile dungeonTileSo)
    {
        DungeonCells dungeonOffset = SearchNeighboursCellType(dir, cellPos, cells);

        if (dungeonOffset == null || dungeonOffset.mCellType == DungeonCellType.None)
        {
            mNeighbourCellType[dir] = DungeonCellType.None;
            mDungeonStanderPrefab[dir] = GetStanderPrefab(dir, dungeonTileSo.wallPrefabs);
            return;
        }

        if (mNeighbourCellType[dir] == dungeonOffset.mCellType) return;

        DestroyStander(dir);
        mNeighbourCellType[dir] = dungeonOffset.mCellType;

        if (dungeonOffset.mCellType == DungeonCellType.Room)
        {
            return;
        }

        if (dungeonOffset.mCellType == DungeonCellType.Corridor)
        {
            mDungeonStanderPrefab[dir] = GetStanderPrefab(dir, dungeonTileSo.entrancePrefabs);

            DungeonEntranceController enCon = mDungeonStanderPrefab[dir].AddComponent<DungeonEntranceController>();
            roomCon = cells[dungeonNode.x, dungeonNode.y].mCurrentDungeonPrefab.GetComponent<DungeonRoomController>();

            enCon.EntranceInit(roomCon);
        }
    }

    #endregion

    GameObject GetStanderPrefab(int dir, List<GameObject> dungeonTileSo)
    {

        Vector3 offset = dir switch //신기허넹
        {
            0 => new Vector3(0, 0, 1), // 상
            1 => new Vector3(1, 0, 1), // 우
            2 => new Vector3(1, 0, 0), // 하
            3 => new Vector3(0, 0, 0), // 좌
            _ => Vector3.zero
        };

        float eulerY = (90 + 90 * dir) % 360;
        Quaternion rotation = Quaternion.Euler(0, eulerY, 0);
        Vector3 position = new Vector3(mCellPos.x * cellSize, 0, mCellPos.y * cellSize) + offset * cellSize;

        GameObject prefab =
            GameObject.Instantiate(dungeonTileSo[Random.Range(0, dungeonTileSo.Count)],
                position, rotation);
        prefab.transform.localScale *= cellSize;
        prefab.transform.SetParent(standerFolder.transform);
        
        return prefab;
    }

    DungeonCells SearchNeighboursCellType(int dir, Vector2Int cellPos, DungeonCells[,] cells)
    {
        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { 1, 0, -1, 0 };

        int dirX = cellPos.x + dx[dir];
        int dirY = cellPos.y + dy[dir];

        if (dirX < 0 || dirX >= cells.GetLength(0) || dirY < 0 || dirY >= cells.GetLength(1))
        {
            return null;
        }

        return cells[dirX, dirY];
    }

    public void SetCorridorCollider(Vector2Int fromCoord, Vector2Int toCoord,bool extraForCorner)
    {
        BoxCollider boxCol = mCurrentTilePrefab.gameObject.AddComponent<BoxCollider>();
        boxCol.center += new Vector3(0.5f, -0.5f, 0.5f);

        Vector2 center = toCoord - fromCoord;
        if (extraForCorner) center += new Vector2(Mathf.Sign(center.x), Mathf.Sign(center.y));

        if (Mathf.Abs(center.x) < Mathf.Abs(center.y))
        {
            boxCol.center += new Vector3(0, 0, center.y * 0.5f);
            boxCol.size = new Vector3(3, 1, center.y + Mathf.Sign(center.y));
        }
        else
        {
            boxCol.center += new Vector3(center.x * 0.5f, 0, 0);
            boxCol.size = new Vector3(center.x + Mathf.Sign(center.x), 1, 3);
        }
    }

    void DestroyStander(int dir)
    {
        if (mDungeonStanderPrefab[dir] != null)
        {
            GameObject.Destroy(mDungeonStanderPrefab[dir]);
        }
    }
}
