using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Attack Behaviors/Dragon")]
public class DragonAttackBehavior : ScriptableObject, IAttackBehavior
{
    [Header("꼬리 공격 (기본)")]
    public float TailRange = 3f;
    public int TailDamage = 10;

    [Header("파이어볼 공격")]
    public float FireballRange = 8f;
    public GameObject ProjectilePrefab;
    public LayerMask HitLayer;
    public int FireballDamage = 20;
    public float Cooldown = 5f;
    public float ProjectileSpeed = 20f;

    private Transform mSelf;
    private Transform mTarget;
    private EnemyBTController mController;
    private bool mbReady = true;

    [Header("브레스 공격")]
    public float BreathRange = 10f;
    public GameObject BreathProjectorPrefab;
    public GameObject BreathVFXPrefab;
    public LayerMask BreathHitLayer;
    public int BreathDamage = 30;
    public float BreathCooldown = 15f;
    private float mLastBreathTime = -Mathf.Infinity;

    public bool CanFireball(Transform self, Transform target)
    {
        return mbReady
               && target != null
               && Vector3.Distance(self.position, target.position) <= FireballRange;
    }

    public bool CanBreath(Transform self, Transform target)
    {
        return Time.time >= mLastBreathTime + BreathCooldown
               && target != null
               && Vector3.Distance(self.position, target.position) <= BreathRange;
    }

    public bool CanTail(Transform self, Transform target)
    {
        return target != null
               && Vector3.Distance(self.position, target.position) <= TailRange;
    }

    public bool IsInRange(Transform self, Transform target)
    {
        if (target == null) return false;
        float d = Vector3.Distance(self.position, target.position);
        return d <= TailRange || d <= FireballRange || d <= BreathRange;
    }

    public void Attack(Transform self, Transform target)
    {
        mSelf = self;
        mTarget = target;
        mController = self.GetComponent<EnemyBTController>();
        var anim = self.GetComponent<Animator>();

        if (CanFireball(self, target))
        {
            mbReady = false;
            anim.SetTrigger("FireBall");
            mController.StartCoroutine(ResetReady());
        }
        else if (CanBreath(self, target))
        {
            mLastBreathTime = Time.time;
            anim.SetTrigger("Breath");
        }
        else if (CanTail(self, target))
        {
            anim.SetTrigger("TailAttack");
        }
    }

    private IEnumerator ResetReady()
    {
        yield return new WaitForSeconds(Cooldown);
        mbReady = true;
    }

    // Ranger 참고해서 설정
    public void FireLastPosition(Transform self, Vector3 targetPosition)
    {
        var fp = self.GetComponent<EnemyBTController>().FirePoint;
        if (fp == null || ProjectilePrefab == null) return;

        Vector3 dir = targetPosition - fp.position;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.01f) dir = self.forward;
        dir.Normalize();

        var proj = Instantiate(ProjectilePrefab, fp.position, Quaternion.LookRotation(dir));
        if (proj.TryGetComponent<Projectile>(out var ps))
            ps.Initialize(dir, ProjectileSpeed, HitLayer, FireballDamage);
    }
}
