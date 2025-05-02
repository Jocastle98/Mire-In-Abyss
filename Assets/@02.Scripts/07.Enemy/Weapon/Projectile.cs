using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float mSpeed = 15f;

    private Rigidbody mRb;
    private Collider mCol;
    private LayerMask mHitLayer;
    private int mDamage;

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
    public void Initialize(Vector3 direction, float speed, LayerMask hitLayer, int damage)
    {
        mRb.velocity = direction.normalized * speed;
        mHitLayer = hitLayer;
        mDamage = damage;

        Destroy(gameObject, 5f);
    }

    private void FixedUpdate()
    {
        if (mRb.velocity.sqrMagnitude > 0.1f)
        {
            var forwardRotation = Quaternion.LookRotation(mRb.velocity);
            transform.rotation = forwardRotation * Quaternion.Euler(90f, 0f, 0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((mHitLayer.value & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }

        if (other.TryGetComponent<EnemyBTController>(out var enemy))
        {
            enemy.SetHit(mDamage);
        }

        Destroy(gameObject);
    }
}