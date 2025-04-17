using UnityEngine;

// 공격 동작을 정의하는 인터페이스
public interface IAttackBehavior
{
    // 공격 범위 내에 있는지 여부를 반환합니다.
    bool IsInRange(Transform self, Transform target);
    // 실제 공격 동작을 수행합니다.
    void Attack(Transform self, Transform target);
}