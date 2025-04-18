using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProjectileSkeleton : MonoBehaviour
{
    [Tooltip("투사체 속도")]
    public float speed = 15f;
    private Vector3 _targetPosition;
    private LayerMask _hitLayer;
    private int _damage;

    /// <summary>
    /// RangedAttackBehavior에서 호출하세요.
    /// </summary>
    public void Initialize(Vector3 targetPosition, LayerMask hitLayer, int damage)
    {
        _targetPosition = targetPosition;
        _hitLayer       = hitLayer;
        _damage         = damage;
        Destroy(gameObject, 5f); // 최대 수명
    }

    void Update()
    {
        // 목표 방향으로 이동
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 목표 레이어 체크
        if (( _hitLayer.value & (1 << other.gameObject.layer) ) == 0)
            return;

        // Player나 몬스터 등 피해 대상에 SetHit 호출
        var enemy = other.GetComponent<EnemyBTController>();
        if (enemy != null)
        {
            enemy.SetHit(_damage);
        }
        // TODO: 플레이어 체력이면 PlayerHealth.ApplyDamage(_damage);

        Destroy(gameObject);
    }
}