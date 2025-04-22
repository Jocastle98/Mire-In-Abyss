using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
    public float radius;

    private Color gizmoColor = Color.white;

    /// <summary>
    /// 컨트롤러를 컴포넌트로 둔 게임오브젝트의 주위로 랜덤하게 오브젝트를 생성
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="parent"></param>
    public void SpawnObj(GameObject obj, GameObject parent)
    {
        Vector3 randomPointOnCircle = Random.insideUnitSphere;
        randomPointOnCircle.Normalize(); // 방향만 남김 (길이 1)
        randomPointOnCircle *= Random.Range(5,radius);   // 원하는 반지름으로 스케일 조정
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position + randomPointOnCircle, Vector3.down, out hit))
        {
            GameObject spawnObj = Instantiate(obj, hit.point, Quaternion.identity);
            spawnObj.transform.parent = parent.transform;
        }
    }

    /// <summary>
    /// 구역 가시화
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(transform.position, new Vector3(radius * 2, 1, radius * 2));
    }
}
