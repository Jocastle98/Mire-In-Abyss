// GolemAttackBehavior.cs
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Attack Behaviors/Golem")]
public class GolemAttackBehavior : ScriptableObject, IAttackBehavior
{
    [Header("스윙 공격")]
    [SerializeField] private float mSwingRange = 3f;
    [SerializeField] private int   mSwingDamage = 20;

    [Header("임팩트 공격")]
    [SerializeField] private float mImpactRange      = 6f;
    [SerializeField] private int   mImpactDamage     = 35;
    [SerializeField] private float mImpactChargeTime = 1.5f;
    [SerializeField] private float mImpactCooldown   = 10f;

    private float mLastImpactTime = -Mathf.Infinity;

    // ── 외부 참조용 프로퍼티 ──
    public float SwingRange       => mSwingRange;
    public int   SwingDamage      => mSwingDamage;
    public float ImpactRange      => mImpactRange;
    public int   ImpactDamage     => mImpactDamage;
    public float ImpactChargeTime => mImpactChargeTime;
    public float ImpactCooldown   => mImpactCooldown;
    // ────────────────────────

    public bool IsInRange(Transform self, Transform target)
    {
        if (target == null) return false;
        float dist = Vector3.Distance(self.position, target.position);
        return dist <= mSwingRange || dist <= mImpactRange;
    }

    public bool CanImpact(Transform self, Transform target)
    {
        if (target == null) return false;
        float dist = Vector3.Distance(self.position, target.position);
        return Time.time >= mLastImpactTime + mImpactCooldown
               && dist <= mImpactRange;
    }

    public bool CanSwing(Transform self, Transform target)
    {
        if (target == null) return false;
        float dist = Vector3.Distance(self.position, target.position);
        return dist <= mSwingRange;
    }

    public void Attack(Transform self, Transform target)
    {
        if (target == null) return;
        Animator anim = self.GetComponent<Animator>();

        if (CanImpact(self, target))
        {
            mLastImpactTime = Time.time;
            anim.SetTrigger("AttackImpact");
        }
        else if (CanSwing(self, target))
        {
            anim.SetTrigger("AttackSwing");
        }
    }
}
