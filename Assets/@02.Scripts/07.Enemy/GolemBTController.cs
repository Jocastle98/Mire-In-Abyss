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
    [SerializeField] private float    mDetectRadius = 15f;
    [SerializeField] private float    mDetectAngle  = 120f;
    [SerializeField] private LayerMask mPlayerMask;
    private Transform mTarget;

    [Header("순찰 설정")]
    [SerializeField] private float mPatrolRadius = 8f;

    [Header("공격 전략")]
    [SerializeField] private ScriptableObject mAttackBehaviorAsset;
    private GolemAttackBehavior        mAttackBehavior;

    [Header("임팩트 장판")]
    [SerializeField] private GameObject ImpactIndicatorPrefab;
    [SerializeField] private LayerMask  ImpactHitLayer;

    private GameObject currentIndicator;
    private Material   indicatorMat;

    private NavMeshAgent mAgent;
    private Animator     mAnim;
    private BTNode       mRoot;
    private bool         mbIsAttacking;
    private bool         mbIsHit;

    void Awake()
    {
        mAgent          = GetComponent<NavMeshAgent>();
        mAnim           = GetComponent<Animator>();
        mAttackBehavior = mAttackBehaviorAsset as GolemAttackBehavior;
    }

    void Start()
    {
        mCurrentHealth = mMaxHealth;

        var deadSeq = new BTSequence(
            new BTCondition(() => mbIsDead),
            new BTAction(() =>
            {
                ClearAllBools();
                mAnim.SetTrigger("Dead");
                mAgent.isStopped = true;
                mAgent.enabled   = false;
                mbIsDead         = false;
            })
        );

        var hitSeq = new BTSequence(
            new BTCondition(() => mbIsHit),
            new BTAction(() =>
            {
                ClearAllBools();
                mAnim.SetTrigger("Hit");
                StartCoroutine(HitFlash());
                mAgent.isStopped = true;
                mbIsHit          = false;
            })
        );

        var detectCond = new BTCondition(DetectPlayer);

        var impactSeq = new BTSequence(
            new BTCondition(() =>
                !mbIsAttacking
             && mAttackBehavior != null
             && mAttackBehavior.CanImpact(transform, mTarget)
            ),
            new BTAction(() =>
            {
                mbIsAttacking    = true;
                ClearAllBools();
                mAgent.isStopped = true;
                mAttackBehavior.Attack(transform, mTarget);
            })
        );

        var swingSeq = new BTSequence(
            new BTCondition(() =>
                !mbIsAttacking
             && mAttackBehavior != null
             && mAttackBehavior.CanSwing(transform, mTarget)
            ),
            new BTAction(() =>
            {
                mbIsAttacking    = true;
                ClearAllBools();
                mAgent.isStopped = true;
                mAttackBehavior.Attack(transform, mTarget);
            })
        );

        var traceSeq = new BTAction(() =>
        {
            if (mbIsAttacking || mTarget == null) return;
            float d = Vector3.Distance(transform.position, mTarget.position);
            if (d > mAttackBehavior.SwingRange
             && d > mAttackBehavior.ImpactRange)
            {
                ClearAllBools();
                mAnim.SetBool("Trace", true);
                mAgent.isStopped = false;
                mAgent.SetDestination(mTarget.position);
            }
        });

        var patrolSeq = new BTAction(() =>
        {
            if (mbIsAttacking) return;
            ClearAllBools();
            mAnim.SetBool("Patrol", true);
            mAgent.isStopped = false;
            if (!mAgent.pathPending
             && (mAgent.remainingDistance <= mAgent.stoppingDistance
              || !mAgent.hasPath))
            {
                Vector3 rnd = Random.insideUnitSphere * mPatrolRadius + transform.position;
                if (NavMesh.SamplePosition(rnd, out var h, mPatrolRadius, NavMesh.AllAreas))
                    mAgent.SetDestination(h.position);
            }
        });

        var idleSeq = new BTAction(() =>
        {
            if (mbIsAttacking) return;
            ClearAllBools();
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

    private void ClearAllBools()
    {
        mAnim.SetBool("Trace",  false);
        mAnim.SetBool("Patrol", false);
        mAnim.SetBool("Idle",   false);
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

    public void OnAttackAnimationExit()
    {
        mbIsAttacking = false;
        ClearAllBools();
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

    // === 애니메이션 이벤트 핸들러 ===

    public void OnImpactIndicator()
    {
        if (ImpactIndicatorPrefab == null || mTarget == null) return;
        Vector3 pos = mTarget.position;
        currentIndicator = Instantiate(
            ImpactIndicatorPrefab, 
            pos, 
            Quaternion.Euler(90f,0f,0f)
        );
        indicatorMat = currentIndicator.GetComponent<Renderer>().material;
        StartCoroutine(IndicatorRoutine());
    }

    private IEnumerator IndicatorRoutine()
    {
        float t        = 0f;
        float charge   = mAttackBehavior.ImpactChargeTime;
        float diameter = mAttackBehavior.ImpactRange * 2f;

        while (t < charge)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / charge);
            currentIndicator.transform.localScale = Vector3.one * (diameter * p);
            Color c = indicatorMat.color;
            c.a = p;
            indicatorMat.color = c;
            yield return null;
        }

        Color fc = indicatorMat.color;
        fc.a = 1f;
        indicatorMat.color = fc;
    }

    public void OnImpactLand()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position, 
            mAttackBehavior.ImpactRange, 
            ImpactHitLayer
        );
        foreach (var col in hits)
            if (col.TryGetComponent<GolemBTController>(out var e))
                e.SetHit(mAttackBehavior.ImpactDamage);
        if (currentIndicator != null)
            Destroy(currentIndicator, 0.5f);
        currentIndicator = null;
        indicatorMat     = null;
    }

    public void OnSwingAttack()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position, 
            mAttackBehavior.SwingRange, 
            ImpactHitLayer
        );
        foreach (var col in hits)
            if (col.TryGetComponent<GolemBTController>(out var e))
                e.SetHit(mAttackBehavior.SwingDamage);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, mDetectRadius);

        Vector3 l = Quaternion.Euler(0,-mDetectAngle*0.5f,0)*transform.forward;
        Vector3 r = Quaternion.Euler(0, mDetectAngle*0.5f,0)*transform.forward;
        Gizmos.DrawRay(transform.position, l*mDetectRadius);
        Gizmos.DrawRay(transform.position, r*mDetectRadius);

        if (mAttackBehavior != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, mAttackBehavior.SwingRange);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, mAttackBehavior.ImpactRange);
        }
    }
}
