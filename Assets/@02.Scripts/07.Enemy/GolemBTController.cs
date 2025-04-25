// GolemBTController.cs
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GolemBTController : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private int mMaxHealth = 500;
    private int mCurrentHealth;
    private bool mbIsDead;

    [Header("감지 설정")]
    [SerializeField] private float mDetectRadius = 15f;
    [SerializeField] private float mDetectAngle  = 120f; // 시야 각도
    [SerializeField] private LayerMask mPlayerMask;
    private Transform mTarget;

    [Header("순찰 설정")]
    [SerializeField] private float mPatrolRadius = 8f;

    [Header("공격 전략")]
    [SerializeField] private ScriptableObject mAttackBehaviorAsset;
    private GolemAttackBehavior mAttackBehavior;

    private NavMeshAgent mAgent;
    private Animator mAnim;
    private BTNode mRoot;
    private bool mbIsAttacking;
    private bool mbIsHit;

    void Awake()
    {
        mAgent = GetComponent<NavMeshAgent>();
        mAnim  = GetComponent<Animator>();
        mAttackBehavior = mAttackBehaviorAsset as GolemAttackBehavior;
    }

    void Start()
    {
        mCurrentHealth = mMaxHealth;

        // 1. Dead
        var deadSeq = new BTSequence(
            new BTCondition(() => mbIsDead),
            new BTAction(() => {
                ResetBools();
                mAnim.SetTrigger("Dead");
                mAgent.isStopped = true;
                mAgent.enabled   = false;
                mbIsDead         = false;
            })
        );

        // 2. Hit
        var hitSeq = new BTSequence(
            new BTCondition(() => mbIsHit),
            new BTAction(() => {
                ResetBools();
                mAnim.SetTrigger("Hit");
                StartCoroutine(HitFlash());
                mAgent.isStopped = true;
                mbIsHit          = false;
            })
        );

        // 3. Detect
        var detectCond = new BTCondition(DetectPlayer);

        // 4. Impact ▶ 5. Swing ▶ 6. Trace ▶ 7. Patrol ▶ 8. Idle
        var impactSeq = new BTSequence(
            new BTCondition(() => !mbIsAttacking && mAttackBehavior != null && mAttackBehavior.CanImpact(transform, mTarget)),
            new BTAction(() => {
                mbIsAttacking = true;
                ResetBools();
                mAgent.isStopped = true;
                mAnim.SetBool("Trace", false);
                mAttackBehavior.Attack(transform, mTarget);
            })
        );

        var swingSeq = new BTSequence(
            new BTCondition(() => !mbIsAttacking && mAttackBehavior !=
                null && mAttackBehavior.CanSwing(transform, mTarget)),
            new BTAction(() => {
                mbIsAttacking = true;
                ResetBools();
                mAgent.isStopped = true;
                mAnim.SetBool("Trace", false);
                mAttackBehavior.Attack(transform, mTarget);
            })
        );

        var traceSeq = new BTAction(() => {
            if (mbIsAttacking || mTarget == null) return;
            float d = Vector3.Distance(transform.position, mTarget.position);
            if (d > mAttackBehavior.SwingRange && d > mAttackBehavior.ImpactRange)
            {
                ResetBools();
                mAnim.SetBool("Trace", true);
                mAgent.isStopped = false;
                mAgent.SetDestination(mTarget.position);
            }
        });

        var patrolSeq = new BTAction(() => {
            if (mbIsAttacking) return;
            ResetBools();
            mAnim.SetBool("Patrol", true);
            mAgent.isStopped = false;
            if (!mAgent.pathPending &&
                (mAgent.remainingDistance <= mAgent.stoppingDistance || !mAgent.hasPath))
            {
                Vector3 rnd = Random.insideUnitSphere * mPatrolRadius + transform.position;
                if (NavMesh.SamplePosition(rnd, out var hit, mPatrolRadius, NavMesh.AllAreas))
                    mAgent.SetDestination(hit.position);
            }
        });

        var idleSeq = new BTAction(() => {
            if (mbIsAttacking) return;
            ResetBools();
            mAnim.SetBool("Idle", true);
            mAgent.isStopped = true;
        });

        mRoot = new BTSelector(
            deadSeq,
            hitSeq,
            new BTSequence(detectCond, new BTSelector(impactSeq, swingSeq, traceSeq)),
            patrolSeq,
            idleSeq
        );
    }

    void Update() => mRoot.Tick();

    private void ResetBools()
    {
        mAnim.SetBool("Trace", false);
        mAnim.SetBool("Patrol", false);
        mAnim.SetBool("Idle", false);
    }

    private bool DetectPlayer()
    {
        var hits = Physics.OverlapSphere(transform.position, mDetectRadius, mPlayerMask);
        foreach (var h in hits)
        {
            Vector3 dir   = (h.transform.position - transform.position).normalized;
            float   angle = Vector3.Angle(transform.forward, dir);
            if (angle <= mDetectAngle * 0.5f)
            {
                mTarget = h.transform;
                return true;
            }
        }
        mTarget = null;
        return false;
    }

    private IEnumerator HitFlash()
    {
        var rends = GetComponentsInChildren<Renderer>();
        float t = 0f;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            Color c = Color.Lerp(Color.red, Color.white, t / 0.5f);
            foreach (var r in rends) r.material.color = c;
            yield return null;
        }
    }

    public void SetHit(int dmg)
    {
        if (mbIsDead) return;
        mCurrentHealth -= dmg;
        if (mCurrentHealth <= 0) mbIsDead = true;
        else mbIsHit = true;
    }

    public void OnHitAnimationExit()
    {
        mbIsHit = false;
        if (mTarget != null)
        {
            mAnim.SetBool("Trace", true);
            mAgent.isStopped = false;
            mAgent.SetDestination(mTarget.position);
        }
    }

    // StateMachineBehaviour(OnStateExit) 에서 호출
    public void OnAttackAnimationExit()
    {
        mbIsAttacking = false;
        ResetBools();
        if (mTarget != null)
        {
            mAnim.SetBool("Trace", true);
            mAgent.isStopped = false;
            mAgent.SetDestination(mTarget.position);
        }
        else
        {
            mAnim.SetBool("Patrol", true);
            mAgent.isStopped = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, mDetectRadius);

        // 시야 각도 표시 (양쪽 레이)
        Vector3 leftDir  = Quaternion.Euler(0, -mDetectAngle * 0.5f, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0,  mDetectAngle * 0.5f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftDir * mDetectRadius);
        Gizmos.DrawRay(transform.position, rightDir * mDetectRadius);

        if (mAttackBehavior != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, mAttackBehavior.SwingRange);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, mAttackBehavior.ImpactRange);
        }
    }
}
