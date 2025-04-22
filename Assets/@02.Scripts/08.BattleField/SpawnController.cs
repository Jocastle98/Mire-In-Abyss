using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
    public float radius;

    private Color gizmoColor = Color.white;

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

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(transform.position, new Vector3(radius * 2, 1, radius * 2));
    }
}
