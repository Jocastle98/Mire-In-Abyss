using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Attack Behaviors/Ranged")]
public class RangedAttackBehavior : ScriptableObject, IAttackBehavior
{
    [Header("원거리 공격 설정")]
    public float  range            = 10f;
    public GameObject projectilePrefab;
    public LayerMask hitLayer;
    public int damage               = 5;
    public float cooldown           = 1f;
    [Tooltip("트리거 대신 레이캐스트 즉발 궤적 사용 시 true")]
    public bool useRaycastTracer    = false;

    [Tooltip("투사체 초기 속도")]
    public float projectileSpeed    = 25f;

    private bool _ready = true;
    private Transform _self, _target;
    private EnemyBTController _controller;

    public bool IsInRange(Transform self, Transform target)
        => Vector3.Distance(self.position, target.position) <= range;

    public void Attack(Transform self, Transform target)
    {
        if (!_ready || projectilePrefab == null) return;
        _ready = false;

        // 캐싱
        _self       = self;
        _target     = target;
        _controller = self.GetComponent<EnemyBTController>();

        // 애니메이션만 트리거
        self.GetComponent<Animator>()?.SetTrigger("Attack");

        // 쿨다운 준비
        self.GetComponent<MonoBehaviour>()
            .StartCoroutine(ResetReady());
    }

    private IEnumerator ResetReady()
    {
        yield return new WaitForSeconds(cooldown);
        _ready = true;
    }

    public void FireProjectileFrom(Transform self)
    {
        var controller = self.GetComponent<EnemyBTController>();
        var target = controller?.Target;
        var fp = controller?.FirePoint;
        if (fp == null || target == null) return;

        Vector3 dir = target.position - fp.position;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.01f) dir = self.forward;
        dir.Normalize();

        if (useRaycastTracer)
        {
            if (Physics.Raycast(fp.position, dir, out var hit, range, hitLayer))
            {
                if (hit.collider.TryGetComponent<EnemyBTController>(out var e))
                    e.SetHit(damage);
                SpawnTracer(fp.position, hit.point);
            }
            else
            {
                SpawnTracer(fp.position, fp.position + dir * range);
            }
        }
        else
        {
            var proj = Object.Instantiate(projectilePrefab, fp.position, Quaternion.LookRotation(dir));
            if (proj.TryGetComponent<ProjectileSkeleton>(out var ps))
                ps.Initialize(dir, projectileSpeed, hitLayer, damage);
        }
    }



    // 간단한 LineRenderer 트레이서 생성 예시
    private void SpawnTracer(Vector3 from, Vector3 to)
    {
        var go = new GameObject("Tracer");
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPositions(new[] { from, to });
        lr.startWidth = lr.endWidth = 0.05f;
        // 원하는 머터리얼/색상 세팅
        Object.Destroy(go, 0.1f); // 짧게 보여주고 제거
    }
}
