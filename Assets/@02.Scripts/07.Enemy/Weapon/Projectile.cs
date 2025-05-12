using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
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


    public void Initialize(Vector3 direction, float speed, LayerMask hitLayer, int damage)
    {
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
            player.SetHit(mDamage, transform, 1);
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