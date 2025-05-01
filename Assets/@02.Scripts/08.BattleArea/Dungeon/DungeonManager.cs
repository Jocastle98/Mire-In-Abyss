using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DungeonManager : MonoBehaviour
{
    //BSP
    public int cellSize = 1;
    public int minDungeonSize = 10;
    [Range(0,4)]
    public int divideLineWidth = 2;
    
    //BattleArea
    [Range(2,20)]
    public int minRoomCount = 6;
    public int levelDesign = 1;
    public int eventRoomChance = 10;
    public GameObject player;
    
    public SODungeonList dungeonListSO;
    
    private DungeonGenerator mDungeonGenerator;
    
    // Start is called before the first frame update
    void Start()
    {
        mDungeonGenerator =
            new DungeonGenerator(cellSize, minDungeonSize, divideLineWidth, minRoomCount, levelDesign, eventRoomChance,
                dungeonListSO, player);
    }

    // public Vector2 gizmoCellSize = new Vector2(1, 1); // 셀 간격
    // public Color gizmoColor = Color.green;

    
    //DungeonGenerator의 GizmoSphereInSetRoom함수와 연계
    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = gizmoColor;
    //
    //     int widthCenter = maxCreateWidth;
    //     int heightCenter = maxCreateHeight;
    //
    //     for (int x = 0; x < widthCenter; x++)
    //     {
    //         for (int y = 0; y < heightCenter; y++)
    //         {
    //             // 좌하단 기준으로 셀 생성
    //             Vector3 bottomLeft = transform.position;
    //             Vector3 cellOffset = new Vector3(x * gizmoCellSize.x, 0, y * gizmoCellSize.y);
    //             Vector3 cellCenter = bottomLeft + cellOffset + new Vector3(gizmoCellSize.x / 2f, 0, gizmoCellSize.y / 2f);
    //             Vector3 cellSize = new Vector3(gizmoCellSize.x, 0, gizmoCellSize.y);
    //
    //             Gizmos.DrawWireCube(cellCenter, cellSize);
    //         }
    //     }
    // }
}
