using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBTController : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private int mMaxHealth = 100;
    private int mCurrentHealth;
    private bool mbIsDead;

    [Header("방어력 설정")]
    [SerializeField] private int mDefense = 5;

    [Header("감지 설정")]
    [SerializeField] private float mDetectRadius = 10f;
    [SerializeField] private float mDetectAngle = 360f;
    [SerializeField] private LayerMask mPlayerMask;
    private Transform mTarget;
    public Transform Target => mTarget;

    [Header("순찰 설정")]
    [SerializeField] private float mPatrolRadius = 5f;

    [Header("공격 설정")]
    [SerializeField] private ScriptableObject mAttackBehaviorAsset;
    private IAttackBehavior mAttackBehavior;

    [Header("원거리 발사 위치 (원거리 스켈레톤)")]
    [SerializeField] private Transform mFirePoint;
    public Transform FirePoint => mFirePoint;
    

    [Header("임팩트 설정 (골렘)")]
    [SerializeField] private GameObject ImpactProjectorPrefab;
    [SerializeField] private LayerMask ImpactHitLayer;

    [Header("렌더러 설정")]
    [SerializeField] private Renderer[] mRenderers;

    private NavMeshAgent mAgent;
    private Animator mAnim;
    private BTNode mRoot;
    private bool mbIsAttacking;
    private bool mbIsHit;
    private Projector currentProjector;

    void Awake()
    {
        mAgent = GetComponent<NavMeshAgent>();
        mAnim = GetComponent<Animator>();
        mAttackBehavior = mAttackBehaviorAsset as IAttackBehavior;
        mRenderers = GetComponentsInChildren<Renderer>();
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
                mAgent.enabled = false;
                mbIsDead = false;
            })
        );

        var hitSeq = new BTSequence(
            new BTCondition(() => mbIsHit),
            new BTAction(() =>
            {
                ClearAllBools();
                mAnim.SetTrigger("Hit");
                StartCoroutine(HitColorChange());
                mAgent.isStopped = true;
                mbIsHit = false;
            })
        );

        var detectCond = new BTCondition(DetectPlayer);

        BTNode engage;

        if (mAttackBehaviorAsset is GolemAttackBehavior)
        {
            var golem = mAttackBehavior as GolemAttackBehavior;
            var impactSeq = new BTSequence(
                new BTCondition(() => !mbIsAttacking && golem.CanImpact(transform, mTarget)),
                new BTAction(() =>
                {
                    FaceTarget();
                    mbIsAttacking = true;
                    ClearAllBools();
                    mAgent.isStopped = true;
                    mAttackBehavior.Attack(transform, mTarget);
                })
            );

            var swingSeq = new BTSequence(
                new BTCondition(() => !mbIsAttacking && golem.CanSwing(transform, mTarget)),
                new BTAction(() =>
                {
                    FaceTarget();
                    mbIsAttacking = true;
                    ClearAllBools();
                    mAgent.isStopped = true;
                    mAttackBehavior.Attack(transform, mTarget);
                })
            );

            var traceSeq = new BTAction(() =>
            {
                if (mbIsAttacking || mTarget == null) return;
                float d = Vector3.Distance(transform.position, mTarget.position);
                if (d > golem.SwingRange && d > golem.ImpactRange)
                {
                    ClearAllBools();
                    mAnim.SetBool("Trace", true);
                    mAgent.isStopped = false;
                    mAgent.SetDestination(mTarget.position);
                }
            });

            engage = new BTSelector(impactSeq, swingSeq, traceSeq);
        }
        else if (mAttackBehaviorAsset is RangedAttackBehavior)
        {
            var ranged = mAttackBehavior as RangedAttackBehavior;
            var rangedAttackSeq = new BTSequence(
                new BTCondition(() => !mbIsAttacking && ranged.IsInRange(transform, mTarget)),
                new BTAction(() =>
                {
                    FaceTarget();
                    mbIsAttacking = true;
                    ClearAllBools();
                    mAgent.isStopped = true;
                    mAnim.SetTrigger("Attack");
                    mAttackBehavior.Attack(transform, mTarget);
                })
            );
            var rangedAimSeq = new BTSequence(
                new BTCondition(() => mTarget != null && mAttackBehavior.IsInRange(transform, mTarget)),
                new BTAction(FaceTarget)
            );
            var rangedTrace = new BTAction(() =>
            {
                if (mbIsAttacking || mTarget == null) return;
                if (!mAttackBehavior.IsInRange(transform, mTarget))
                {
                    ClearAllBools();
                    mAnim.SetBool("Trace", true);
                    mAgent.isStopped = false;
                    mAgent.SetDestination(mTarget.position);
                }
            });
            engage = new BTSelector(rangedAttackSeq, rangedAimSeq, rangedTrace);
        }
        else if(mAttackBehaviorAsset is MeleeAttackBehavior)
        {
            var attackSeq = new BTSequence(
                new BTCondition(() => !mbIsAttacking && mTarget != null && mAttackBehavior.IsInRange(transform, mTarget)),
                new BTAction(() =>
                {
                    mbIsAttacking = true;
                    ClearAllBools();
                    mAgent.isStopped = true;
                    mAttackBehavior.Attack(transform, mTarget);
                })
            );
            var traceSeq = new BTAction(() =>
            {
                if (mbIsAttacking || mTarget == null) return;
                if (!mAttackBehavior.IsInRange(transform, mTarget))
                {
                    ClearAllBools();
                    mAnim.SetBool("Trace", true);
                    mAgent.isStopped = false;
                    mAgent.SetDestination(mTarget.position);
                }
            });
            engage = new BTSelector(attackSeq, traceSeq);
        }
        else
        {
            engage = new BTAction(() => {});
            Debug.LogWarning("다른 AttackBehavior를 추가해서 넣을 예쩡");
        }

        var patrol = new BTAction(() =>
        {
            if (mbIsAttacking) return;
            ClearAllBools();
            mAnim.SetBool("Patrol", true);
            mAgent.isStopped = false;
            if (!mAgent.pathPending && (mAgent.remainingDistance <= mAgent.stoppingDistance || !mAgent.hasPath))
            {
                var rnd = Random.insideUnitSphere * mPatrolRadius + transform.position;
                if (NavMesh.SamplePosition(rnd, out var hit, mPatrolRadius, NavMesh.AllAreas))
                    mAgent.SetDestination(hit.position);
            }
        });
        var idle = new BTAction(() =>
        {
            if (mbIsAttacking) return;
            ClearAllBools();
            mAnim.SetBool("Idle", true);
            mAgent.isStopped = true;
        });

        mRoot = new BTSelector(deadSeq, hitSeq, new BTSequence(detectCond, engage), patrol, idle);
    }

    void Update() => mRoot.Tick();

    private void FaceTarget()
    {
        if (mTarget == null) return;
        Vector3 dir = (mTarget.position - transform.position).normalized;
        dir.y = 0;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    private void ClearAllBools()
    {
        mAnim.SetBool("Patrol", false);
        mAnim.SetBool("Trace", false);
        mAnim.SetBool("Idle", false);
    }

    #region 플레이어 감지
    private bool DetectPlayer()
    {
        var hits = Physics.OverlapSphere(transform.position, mDetectRadius, mPlayerMask);
        foreach (var h in hits)
        {
            var dir = (h.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dir) <= mDetectAngle * 0.5f)
            {
                mTarget = h.transform;
                return true;
            }
        }
        mTarget = null;
        return false;
    }
    #endregion

    #region Hit & Death 처리
    
    public void SetHit(int damage)
    {
        if (mbIsDead) return;
        int effective = Mathf.Max(0, damage - mDefense);
        mCurrentHealth -= effective;
        Debug.Log($"받은 대미지:{damage} 방어력:{mDefense} 최종:{effective} 남은체력:{mCurrentHealth}");
        if (mCurrentHealth <= 0) mbIsDead = true; else mbIsHit = true;
    }

    public void OnHitAnimationExit()
    {
        mbIsHit = false;
        if (DetectPlayer())
        {
            mAnim.SetBool("Trace", true);
            mAgent.isStopped = false;
            mAgent.SetDestination(mTarget.position);
        }
    }

    public void OnDeadAnimationExit()
    {
        StartCoroutine(Dissolve());
    }
    private IEnumerator HitColorChange()
    {
        var block = new MaterialPropertyBlock();
        float t = 0f;
        const float duration = 0.5f;
        while (t < duration)
        {
            t += Time.deltaTime;
            ChangeColorRenderer(Color.Lerp(Color.red, Color.white, t / duration), block);
            yield return null;
        }
        ChangeColorRenderer(Color.white, block);
    }

    private IEnumerator Dissolve()
    {
        var block = new MaterialPropertyBlock();
        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime;
            ChangeColorRenderer(new Color(1, 1, 1, alpha), block);
            yield return null;
        }
        Destroy(gameObject);
    }

    private void ChangeColorRenderer(Color color, MaterialPropertyBlock block)
    {
        foreach (var r in mRenderers)
        {
            r.GetPropertyBlock(block);
            block.SetColor("_Color", color);
            r.SetPropertyBlock(block);
        }
    }

    #endregion

    #region Attack 이벤트
    public void OnAttackAnimationExit()
    {
        mbIsAttacking = false;
        ClearAllBools();
        if (DetectPlayer())
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

    public void FireProjectile()
    {
        if (mAttackBehaviorAsset is RangedAttackBehavior ranged)
            ranged.FireLastPosition(transform, mTarget != null ? mTarget.position : transform.position + transform.forward * 10f);
    }

    public void OnImpactIndicator()
    {
        if (ImpactProjectorPrefab == null || !(mAttackBehaviorAsset is GolemAttackBehavior golem) || mTarget == null) return;
        var go = Instantiate(ImpactProjectorPrefab);
        currentProjector = go.GetComponent<Projector>();
        go.transform.position = mTarget.position + Vector3.up * 5f;
        go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        StartCoroutine(ScaleUpProjector(golem));
    }

    private IEnumerator ScaleUpProjector(GolemAttackBehavior golem)
    {
        float elapsed = 0f;
        while (elapsed < golem.ImpactChargeTime)
        {
            elapsed += Time.deltaTime;
            currentProjector.orthographicSize = Mathf.Lerp(0f, golem.ImpactRange, elapsed / golem.ImpactChargeTime);
            yield return null;
        }
        currentProjector.orthographicSize = golem.ImpactRange;
        OnImpactLand();
    }

    public void OnImpactLand()
    {
        if (!(mAttackBehaviorAsset is GolemAttackBehavior golem)) return;
        var hits = Physics.OverlapSphere(transform.position, golem.ImpactRange, ImpactHitLayer);
        foreach (var col in hits)
            if (col.CompareTag("Player"))
            {
                GolemAttackBehavior golemAttackPower = mAttackBehaviorAsset as GolemAttackBehavior;
                col.GetComponent<PlayerController>().SetHit(golemAttackPower.ImpactDamage, this.transform, 1);
                Debug.Log("임팩트 데미지 적용");
            }
        if (currentProjector) Destroy(currentProjector.gameObject, 0.5f);
    }

    public void OnSwingAttack()
    {
        if (!(mAttackBehaviorAsset is GolemAttackBehavior golem)) return;
        var hits = Physics.OverlapSphere(transform.position, golem.SwingRange, ImpactHitLayer);
        foreach (var col in hits)
            if (col.CompareTag("Player"))
            {
                GolemAttackBehavior golemAttackPower = mAttackBehaviorAsset as GolemAttackBehavior;
                col.GetComponent<PlayerController>().SetHit(golemAttackPower.SwingDamage, this.transform, 0);
                Debug.Log("스윙 데미지 적용");
            }
    }
    #endregion

    #region 디버그 기즈모
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, mDetectRadius);
        if (mAttackBehaviorAsset is MeleeAttackBehavior melee)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, melee.Range);
        }
        else if (mAttackBehaviorAsset is RangedAttackBehavior ranged)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, ranged.Range);
        }
        else if (mAttackBehaviorAsset is GolemAttackBehavior golem)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, golem.SwingRange);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, golem.ImpactRange);
        }
    }
    #endregion
}