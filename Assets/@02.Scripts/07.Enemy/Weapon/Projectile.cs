using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float mSpeed = 15f;

    public Transform ShooterTransform { get; private set; }
    private Rigidbody mRb;
    private Collider mCol;
    private LayerMask mHitLayer;
    private int mDamage;

    
    private bool  mIsBreath = false;
    private float mLastDamageTime;
    private float mDamageInterval = 0.3f;
    private void Awake()
    {
        mRb = GetComponent<Rigidbody>();
        mCol = GetComponent<Collider>();

        mRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        mRb.useGravity = false;
        mCol.isTrigger = true;
    }

    /// <summary>
    /// RangedAttackBehavior에서 호출하세요.
    /// </summary>
    public void Initialize(Vector3 direction, float speed, LayerMask hitLayer, int damage, Transform shooter = null)
    {
        ShooterTransform = shooter;
        mRb.velocity = direction.normalized * speed;
        mHitLayer = hitLayer;
        mDamage = damage;

        Destroy(gameObject, 5f);
    }
    
    public void InitializeBreath(LayerMask hitLayer, int damage)
    {
        mRb.velocity      = Vector3.zero;
        mHitLayer         = hitLayer;
        mDamage           = damage;
        mIsBreath         = true;
        mLastDamageTime   = Time.time;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (mIsBreath) return;

        if ((mHitLayer.value & 1 << other.gameObject.layer) == 0) return;
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            player.SetHit(mDamage, transform, 1);
        }
        else if (other.TryGetComponent<EnemyBTController>(out var enemy)) // 발사체에 맞은게 몬스터인데
        {
            // ShooterTransform이(발사한 객체가) 있고, ShooterTransform이(발사한 객체가) 몬스터라면
            if (ShooterTransform != null && ShooterTransform.GetComponent<EnemyBTController>() != null)
            {
                Destroy(gameObject);
                return; // 아군이니까 피해 없음
            }
            
            // ShooterTransform이(발사한 객체가) 없거나, ShooterTransform이(발사한 객체가) 몬스터가 아니면 피해를 줌
            enemy.SetHit(mDamage, -1);
        }
        Destroy(gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!mIsBreath) return;
        if ((mHitLayer.value & 1 << other.gameObject.layer) == 0) return;

        // 간격 체크
        if (Time.time < mLastDamageTime + mDamageInterval) return;
        mLastDamageTime = Time.time;

        if (other.TryGetComponent<PlayerController>(out var player))
            player.SetHit(mDamage, transform, 2); // 2 = 브레스 연속딜 타입
    }
}