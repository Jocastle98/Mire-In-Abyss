using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HitBox : MonoBehaviour
{
    [Tooltip("이 레이어에 속한 오브젝트만 데미지를 입힙니다.")]
    public LayerMask hitLayer;
    public int damage = 10;

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        col.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((hitLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        //var health = other.GetComponent<PlayerHealth>();
        // if (health != null)
        // {
        //     health.ApplyDamage(damage);
        // }
        // 몬스터 컨트롤러 찾아서 SetHit 호출
        var enemy = other.GetComponent<EnemyBTController>();
        if (enemy != null)
        {
            enemy.SetHit(damage);
            Debug.Log("무기 히트! 데미지 적용");

        }

    }
}