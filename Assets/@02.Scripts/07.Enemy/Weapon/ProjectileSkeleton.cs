using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ProjectileSkeleton : MonoBehaviour
{
    public float speed = 15f;

    private Rigidbody _rb;
    private Collider _col;
    private LayerMask _hitLayer;
    private int _damage;

    void Awake()
    {
        // Rigidbody와 Collider 컴포넌트 초기화
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();

        // 물리 정확도 향상을 위한 설정
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _rb.useGravity = false;     // 중력 필요 시 true로 변경

        // 트리거 사용
        _col.isTrigger = true;
    }

    /// <summary>
    /// RangedAttackBehavior에서 호출하세요.
    /// </summary>
    /// <param name="direction">발사 방향 (normalized)</param>
    /// <param name="speed">초기 속도</param>
    public void Initialize(Vector3 direction, float speed, LayerMask hitLayer, int damage)
    {
        _rb.velocity = direction.normalized * speed;
        _hitLayer = hitLayer;
        _damage = damage;

        Destroy(gameObject, 5f); // 수명
    }

    void FixedUpdate()
    {
        // 날아가는 방향으로 회전
        if (_rb.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion forwardRotation = Quaternion.LookRotation(_rb.velocity);
            transform.rotation = forwardRotation * Quaternion.Euler(90f, 0f, 0f); // X축 90도 회전 보정
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 목표 레이어 체크
        if ((_hitLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        if (other.TryGetComponent<EnemyBTController>(out var enemy))
        {
            enemy.SetHit(_damage);
        }

        Destroy(gameObject);
    }
}