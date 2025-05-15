using UnityEngine;
using System.Collections.Generic;

public sealed class CameraObstacleHandler : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] LayerMask obstacleMask;
    
    readonly List<ObstacleFader> lastHits = new();

    void LateUpdate()
    {
        Vector3 camPos = transform.position;
        Vector3 dir = player.position - camPos;
        float dist = dir.magnitude;

        var hits = Physics.RaycastAll(camPos, dir.normalized, dist, obstacleMask);
        HashSet<ObstacleFader> current = new();

        foreach (var h in hits)
        {
            if (h.collider.TryGetComponent(out ObstacleFader f))
            {
                f.FadeOut();
                current.Add(f);
            }
        }
        // 지난 프레임이었는데 이번엔 안 걸린 애들 복원
        foreach (var f in lastHits)
            if (!current.Contains(f)) f.FadeIn();

        lastHits.Clear();
        lastHits.AddRange(current);
    }
}
