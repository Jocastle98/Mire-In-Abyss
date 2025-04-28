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

    [Header("임팩트 프로젝터")]
    [SerializeField] private GameObject ImpactProjectorPrefab;
    [SerializeField] private LayerMask  ImpactHitLayer;
    
    [Header("렌더러 설정")]
    [SerializeField] private Renderer[] mRenderers;

    private Projector currentProjector;

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
        mRenderers    = GetComponentsInChildren<Renderer>();
    }

    void Start()
    {
        mCurrentHealth = mMaxHealth;

        // Dead 시퀀스
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

        // Hit 시퀀스
        var hitSeq = new BTSequence(
            new BTCondition(() => mbIsHit),
            new BTAction(() =>
            {
                ClearAllBools();
                mAnim.SetTrigger("Hit");
                StartCoroutine(HitColorChange());
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

    #region 플레이어 감지하기

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

    #endregion
    

    #region 골렘 Hit 

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

    public void SetHit(int damage)
    {
        if (mbIsDead) return;

        mCurrentHealth -= damage;
        Debug.Log($"현재 체력: {mCurrentHealth}");

        if (mCurrentHealth <= 0)
        {
            mbIsDead = true;
        }
        else
        {
            mbIsHit = true;
        }
    }
    private IEnumerator HitColorChange()
    {
        var block = new MaterialPropertyBlock();
        const float Duration = 0.5f;
        float elapsed = 0f;

        ChangeColorRenderer(Color.red, block);
        while (elapsed < Duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / Duration);
            var color = Color.Lerp(Color.red, Color.white, t);
            ChangeColorRenderer(color, block);
            yield return null;
        }
        ChangeColorRenderer(Color.white, block);
    }
    private void ChangeColorRenderer(Color color, MaterialPropertyBlock block)
    {
        foreach (var renderer in mRenderers)
        {
            renderer.GetPropertyBlock(block);
            block.SetColor("_Color", color);
            renderer.SetPropertyBlock(block);
        }
    }
    
    public void OnDeadAnimationExit()
    {
        StartCoroutine(Dissolve());
    }

    private IEnumerator Dissolve()
    {
        var block = new MaterialPropertyBlock();
        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime;
            float clamped = Mathf.Clamp01(alpha);
            foreach (var renderer in mRenderers)
            {
                renderer.GetPropertyBlock(block);
                var color = block.GetColor("_Color");
                color.a = clamped;
                block.SetColor("_Color", color);
                renderer.SetPropertyBlock(block);
            }
            yield return null;
        }
        Destroy(gameObject);
    }

    #endregion

    #region 골렘 Attack

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

    public void OnImpactIndicator()
    {
        if (ImpactProjectorPrefab == null || mTarget == null) return;
        var go = Instantiate(ImpactProjectorPrefab);
        currentProjector = go.GetComponent<Projector>();

        currentProjector.orthographicSize = 0f;

        go.transform.position = mTarget.position + Vector3.up * 5f;
        go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        StartCoroutine(ScaleUpProjector());
    }

    // Projector의 크기를 Impact의 크기만큼 키우기(GolemAttackBehavior에서 참고가능)
    private IEnumerator ScaleUpProjector()
    {
        float t        = 0f;
        float duration = mAttackBehavior.ImpactChargeTime;
        float maxSize  = mAttackBehavior.ImpactRange;

        while (t < duration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / duration);
            currentProjector.orthographicSize = Mathf.Lerp(0f, maxSize, ratio);
            yield return null;
        }
        currentProjector.orthographicSize = maxSize;
        OnImpactLand();
    }

    public void OnImpactLand()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            mAttackBehavior.ImpactRange,
            ImpactHitLayer
        );
        foreach (var col in hits)
        {
            if (col.CompareTag("Player"))
            {
                Debug.Log("임팩트 데미지! ");
                // TODO: playerHealth.TakeDamage(mAttackBehavior.ImpactDamage);
            }
        }

        if (currentProjector != null)
            Destroy(currentProjector.gameObject, 0.5f);
        currentProjector = null;
    }

    public void OnSwingAttack()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            mAttackBehavior.SwingRange,
            ImpactHitLayer
        );

        foreach (var col in hits)
        {
            if (col.CompareTag("Player"))
            {
                Debug.Log("스윙 데미지! ");
                // TODO: playerHealth.TakeDamage(mAttackBehavior.SwingDamage);
            }
        }
    }


    #endregion

    #region 디버깅 기즈모

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, mDetectRadius);

        Vector3 l = Quaternion.Euler(0, -mDetectAngle * 0.5f, 0) * transform.forward;
        Vector3 r = Quaternion.Euler(0,  mDetectAngle * 0.5f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, l * mDetectRadius);
        Gizmos.DrawRay(transform.position, r * mDetectRadius);

        if (mAttackBehavior != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, mAttackBehavior.SwingRange);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, mAttackBehavior.ImpactRange);
        }
    }

    #endregion

    
}
