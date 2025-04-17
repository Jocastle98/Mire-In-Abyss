using UnityEngine;

[CreateAssetMenu(fileName = "MeleeAttackBehavior", menuName = "AI/Attack Behaviors/Melee")]
public class MeleeAttackBehavior : ScriptableObject, IAttackBehavior
{
    [Header("근접 공격 설정")]
    public float range = 2f;   // 공격 범위 (BTController에서 IsInRange로 사용)
    public int damage   = 10;  // 데미지

    public bool IsInRange(Transform self, Transform target)
    {
        // BT 트리에서 이 값으로 공격/추적 분기
        return Vector3.Distance(self.position, target.position) <= range;
    }

    public void Attack(Transform self, Transform target)
    {
        // 애니메이션 재생만 트리거
        var anim = self.GetComponent<Animator>();
        anim.SetTrigger("Attack");
        // 실제 데미지는 히트박스(WeaponHitboxController)에서 처리
    }
}