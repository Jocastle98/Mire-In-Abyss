// RangedAttackBehavior.cs
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName="AI/Attack Behaviors/Ranged")]
public class RangedAttackBehavior : ScriptableObject, IAttackBehavior
{
    public float       range             = 10f;
    public GameObject  projectilePrefab;
    public LayerMask   hitLayer;
    public int         damage            = 5;
    public float       cooldown          = 1f;

    private bool _ready = true;

    public bool IsInRange(Transform self, Transform target)
        => Vector3.Distance(self.position, target.position) <= range;

    public void Attack(Transform self, Transform target)
    {
        if (!_ready || projectilePrefab == null) return;
        _ready = false;

        // EnemyBTController 에서 firePoint 프로퍼티 가져오기
        var controller = self.GetComponent<EnemyBTController>();
        var fp = controller?.FirePoint;
        if (fp == null) return;

        // 애니메이션
        self.GetComponent<Animator>()?.SetTrigger("Attack");

        // 투사체 생성
        var proj = Instantiate(projectilePrefab, fp.position, fp.rotation);
        if (proj.TryGetComponent<ProjectileSkeleton>(out var ps))
            ps.Initialize(target.position, hitLayer, damage);

        // 쿨다운 돌리기
        self.GetComponent<MonoBehaviour>()
            .StartCoroutine(ResetReady());
    }

    private IEnumerator ResetReady()
    {
        yield return new WaitForSeconds(cooldown);
        _ready = true;
    }
}
