using System;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Attack Behaviors/Dragon")]
public class DragonAttackBehavior : ScriptableObject, IAttackBehavior
{
    [Header("지상 브레스")]
    public float BreathRange;
    public float BreathCooldown = 10f;
    private float mLastBreathTime = -Mathf.Infinity;

    [Header("지상 꼬리")]
    public float TailRange;

    [Header("공중 불발사")]
    public float AirFireRange;

    public bool CanBreath(Transform self, Transform target) {
        return Time.time >= mLastBreathTime + BreathCooldown
               && Vector3.Distance(self.position, target.position) <= BreathRange;
    }
    public bool CanTail(Transform self, Transform target) {
        return Vector3.Distance(self.position, target.position) <= TailRange;
    }
    public bool CanAirFire(Transform self, Transform target) {
        return Vector3.Distance(self.position, target.position) <= AirFireRange;
    }

    public bool IsInRange(Transform self, Transform target)
    {
        throw new NotImplementedException();
    }

    public void Attack(Transform self, Transform target) {
        var anim = self.GetComponent<Animator>();
        if (CanBreath(self, target)) {
            mLastBreathTime = Time.time;
            anim.SetTrigger("Breath");
        }
        else if (CanTail(self, target)) {
            anim.SetTrigger("Tail");
        }
        else if (CanAirFire(self, target)) {
            anim.SetTrigger("AirFire");
        }
    }
}