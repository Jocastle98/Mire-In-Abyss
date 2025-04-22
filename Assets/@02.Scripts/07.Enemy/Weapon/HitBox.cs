using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HitBox : MonoBehaviour
{
    [Tooltip("이 레이어에 속한 오브젝트만 데미지를 입힙니다.")]
    [SerializeField] private LayerMask mHitLayer;
    [SerializeField] private int mDamage = 10;

    private Collider mCollider;

    private void Awake()
    {
        mCollider = GetComponent<Collider>();
        mCollider.isTrigger = true;
        mCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((mHitLayer.value & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }

        var enemy = other.GetComponent<EnemyBTController>();
        if (enemy != null)
        {
            enemy.SetHit(mDamage);
            Debug.Log("무기 히트! 데미지 적용");
        }
    }
}