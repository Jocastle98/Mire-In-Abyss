// Projectile.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    public Transform ShooterTransform { get; private set; }

    private Rigidbody   mRb;
    private Collider    mCol;
    private LayerMask   mHitLayer;
    private int         mDamage;
    private bool        mIsBreath = false;
    private float       mLastDamageTime;
    private float       mDamageInterval = 0.3f;

    private void Awake()
    {
        mRb = GetComponent<Rigidbody>();
        mCol = GetComponent<Collider>();

        mRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        mRb.useGravity = false;
        mCol.isTrigger = true;
    }

    // 발사자 정보를 함께 넘기도록 시그니처 변경
    public void Initialize(Vector3 direction, float speed, LayerMask hitLayer, int damage, Transform shooter)
    {
        ShooterTransform = shooter;
        mRb.velocity     = direction.normalized * speed;
        mHitLayer        = hitLayer;
        mDamage          = damage;
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
        if (mIsBreath) 
            return;

        // 1) 비대상 레이어(장애물 등)에 부딪히면 파괴
        if ((mHitLayer.value & (1 << other.gameObject.layer)) == 0)
        {
            Destroy(gameObject,2f);
            return;
        }

        // 2) 발사자 자신과 부딪히면 무시
        if (ShooterTransform != null && other.transform == ShooterTransform)
            return;

        // 3) 발사자가 몬스터 → 플레이어만 때리고 파괴
        if (ShooterTransform != null && ShooterTransform.GetComponent<EnemyBTController>() != null)
        {
            if (other.TryGetComponent<PlayerController>(out var player))
                player.SetHit(mDamage, transform, 1);
            Destroy(gameObject,2f);
            return;
        }

        // 4) 발사자가 플레이어 → 몬스터만 때리고 파괴
        if (ShooterTransform != null && ShooterTransform.GetComponent<PlayerController>() != null)
        {
            if (other.TryGetComponent<EnemyBTController>(out var enemy))
                enemy.SetHit(mDamage, -1);
            Destroy(gameObject,1f);
            return;
        }

        // 5) 발사자 정보 없을 때 기본 처리 (플레이어에만 데미지)
        if (other.TryGetComponent<PlayerController>(out var fallbackPlayer))
            fallbackPlayer.SetHit(mDamage, transform, 1);
        Destroy(gameObject,2f);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!mIsBreath) return;
        if ((mHitLayer.value & (1 << other.gameObject.layer)) == 0) return;

        if (Time.time < mLastDamageTime + mDamageInterval) return;
        mLastDamageTime = Time.time;

        if (other.TryGetComponent<PlayerController>(out var player))
            player.SetHit(mDamage, transform, 2);
    }
}
