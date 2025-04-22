using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoissonGenerator
{
    private Vector2 mapSize;
    private int[,] grid;
    private int cellSize;
    int radius;

    /// <summary>
    /// 맵 크기, 오브젝트끼리의 최소 소환 방지 거리
    /// </summary>
    /// <param name="mapSize"></param>
    /// <param name="radius"></param>
    public PoissonGenerator(Vector2 mapSize,int radius)
    {
        this.mapSize = mapSize;
        this.radius = radius;
    }

    /// <summary>
    /// 맵의 중앙에 포인트를 두고 해당 포인트의 주위로 새로운 포인트를 놔둔 후, 최소 거리를 비교함
    /// 최소 거리 이상인 포인트 끼리만 리스트에 추가 시켜 반환
    /// </summary>
    /// <param name="searchLimit"></param>
    /// <returns></returns>
    public List<Vector2> GeneratePoissonList(int searchLimit)
    {
        cellSize = (int)(radius / Mathf.Sqrt(2));
        grid = new int[Mathf.CeilToInt(mapSize.x / cellSize), Mathf.CeilToInt(mapSize.y / cellSize)];
        
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();
        
        spawnPoints.Add(mapSize / 2);
        
        while (spawnPoints.Count > 0)
        {
            int randomPoint = Random.Range(0, spawnPoints.Count);
            Vector2 spawnCenter = spawnPoints[randomPoint];
            bool candidateAccepted = false;
            
            for (int i = 0; i < searchLimit; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 candidate = spawnCenter + dir * Random.Range(radius, radius * 2);
                if (IsPoissonValid(candidate,points))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    // 나중에 거리 체크를 할 때, 인접 셀에 어떤 점이 있는지 빠르게 참조하려고 사용함.
                    // 예: 새로운 후보 점이 기존 점들과 너무 가까운지 검사할 때,
                    // 주변 셀을 검사 → grid[x, y]에 값이 있다면 → points[grid[x, y] - 1] 로 기존 점 위치 참조 가능.
                    grid[(int)(candidate.x/cellSize), (int)(candidate.y/cellSize)] = points.Count;
                    candidateAccepted = true;
                    break;
                }
            }
            
            if(!candidateAccepted)
                spawnPoints.RemoveAt(randomPoint);
        }
        
        return points;
    }

    /// <summary>
    /// 새로 둔 포인트 주위로 기존의 놓여진 포인터를 확인
    /// 두 포인트의 거리가 최소거리에 못 미친다면 false를 반환
    /// 이상이면 true후 리스트에 추가
    /// </summary>
    /// <param name="candidate"></param>
    /// <param name="points"></param>
    /// <returns></returns>
    bool IsPoissonValid(Vector2 candidate,List<Vector2> points)
    {
        if (candidate.x < 0 | candidate.y < 0 | candidate.x > mapSize.x | candidate.y > mapSize.y) return false;

        int cellX = (int)(candidate.x / cellSize);
        int cellY = (int)(candidate.y / cellSize);
        // int searchStartX = Mathf.Max(0, cellX - 2);
        // int searchStartY = Mathf.Max(0, cellY - 2);
        // int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
        // int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);
        
        int searchStartX = cellX - 2;
        int searchStartY = cellY - 2;
        int searchEndX = cellX + 2;
        int searchEndY = cellY + 2;

        for (int x = searchStartX; x < searchEndX; x++)
        {
            for (int y = searchStartY; y < searchEndY; y++)
            {
                if(x< 0 || x >= grid.GetLength(0) || y< 0 || y >= grid.GetLength(1)) return false;
                
                int pointIndex = grid[x, y] - 1;
                if (pointIndex != -1)
                {
                    float sqrDist = (candidate - points[pointIndex]).sqrMagnitude;
                    if (sqrDist < radius * radius)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }
}
