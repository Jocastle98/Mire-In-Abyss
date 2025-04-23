using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBTController : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private int mMaxHealth = 100;
    private int mCurrentHealth;
    private bool mbIsDead = false;

    [Header("감지 설정")]
    [SerializeField] private float mDetectRadius = 10f;
    [SerializeField] private float mDetectAngle = 360f;
    [SerializeField] private LayerMask mPlayerMask;
    private Vector3 mLastAttackPosition;

    [Header("순찰 설정")]
    [SerializeField] private float mPatrolRadius = 5f;

    [Header("공격 전략")]
    [SerializeField] private ScriptableObject mAttackBehaviorAsset;
    private IAttackBehavior mAttackBehavior;

    private NavMeshAgent mAgent;
    private Animator mAnim;
    private Transform mTarget;
    public Transform Target => mTarget;

    private BTNode mRoot;

    private bool mbIsAttacking = false;
    private bool mbIsHit = false;

    [Header("렌더러 설정")]
    [SerializeField] private Renderer[] mRenderers;

    [Header("원거리 발사 위치")]
    [SerializeField] private Transform mFirePoint;
    public Transform FirePoint => mFirePoint;

    private void Awake()
    {
        mAgent = GetComponent<NavMeshAgent>();
        mAnim = GetComponent<Animator>();
        mAttackBehavior = mAttackBehaviorAsset as IAttackBehavior;
        mRenderers = GetComponentsInChildren<Renderer>();
    }

    private void Start()
    {
        mCurrentHealth = mMaxHealth;

        var deadSeq = new BTSequence(
            new BTCondition(() => mbIsDead),
            new BTAction(() =>
            {
                ClearAllBools();
                mAnim.SetTrigger("Dead");
                if (mAgent.enabled && mAgent.isOnNavMesh)
                {
                    mAgent.isStopped = true;
                    mAgent.enabled = false;
                }
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
                if (mAgent.enabled && mAgent.isOnNavMesh)
                {
                    mAgent.isStopped = true;
                    mAgent.ResetPath();
                }
            })
        );

        var detectCond = new BTCondition(DetectPlayer);

        BTNode attackSeq = null;
        BTNode traceSeq = null;
        BTNode engage;

        if (mAttackBehaviorAsset is MeleeAttackBehavior)
        {
            attackSeq = new BTSequence(
                new BTCondition(() => !mbIsAttacking && mAttackBehavior != null && mTarget != null && mAttackBehavior.IsInRange(transform, mTarget)),
                new BTAction(() =>
                {
                    if (mAgent.enabled && mAgent.isOnNavMesh)
                    {
                        mAgent.isStopped = true;
                        mAgent.velocity = Vector3.zero;
                        mAgent.ResetPath();
                    }
                    ClearAllBools();
                }),
                new BTAction(() =>
                {
                    mbIsAttacking = true;
                    mAttackBehavior.Attack(transform, mTarget);
                })
            );

            traceSeq = new BTAction(() =>
            {
                if (mbIsAttacking) return;
                if (mTarget == null) return;
                if (!mAttackBehavior.IsInRange(transform, mTarget))
                {
                    ClearAllBools();
                    mAnim.SetBool("Trace", true);
                    mAgent.isStopped = false;
                    mAgent.SetDestination(mTarget.position);
                }
            });
        }

        if (mAttackBehaviorAsset is RangedAttackBehavior)
        {
            var rangedAttackSeq = new BTSequence(
                new BTCondition(() =>
                    !mbIsAttacking &&
                    mAttackBehavior != null &&
                    mTarget != null &&
                    mAttackBehavior.IsInRange(transform, mTarget)
                ),
                new BTAction(() =>
                {
                    if (mAgent.isOnNavMesh)
                    {
                        mAgent.isStopped = true;
                        mAgent.ResetPath();
                    }
                    ClearAllBools();
                    mbIsAttacking = true;
                    mLastAttackPosition = mTarget.position;
                    Vector3 lookPos = mTarget.position;
                    lookPos.y = transform.position.y;
                    transform.rotation = Quaternion.LookRotation((lookPos - transform.position).normalized);
                    mAnim.SetTrigger("Attack");
                    mAttackBehavior.Attack(transform, mTarget);
                })
            );

            var rangedAimSeq = new BTSequence(
                new BTCondition(() =>
                    mTarget != null &&
                    mAttackBehavior.IsInRange(transform, mTarget)
                ),
                new BTAction(() =>
                {
                    mLastAttackPosition = mTarget.position;
                    Vector3 lookPos = mTarget.position;
                    lookPos.y = transform.position.y;
                    transform.rotation = Quaternion.LookRotation((lookPos - transform.position).normalized);
                })
            );

            var rangedTraceSeq = new BTAction(() =>
            {
                if (mbIsAttacking) return;
                if (mTarget != null && !mAttackBehavior.IsInRange(transform, mTarget))
                {
                    ClearAllBools();
                    mAnim.SetBool("Trace", true);
                    mAgent.isStopped = false;
                    mAgent.SetDestination(mTarget.position);
                }
            });

            engage = new BTSelector(rangedAttackSeq, rangedAimSeq, rangedTraceSeq);
        }
        else
        {
            engage = new BTSelector(attackSeq, traceSeq);
        }

        var patrolAction = new BTAction(() =>
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

        var idleAction = new BTAction(() =>
        {
            if (mbIsAttacking) return;
            ClearAllBools();
            mAnim.SetBool("Idle", true);
            mAgent.isStopped = true;
        });

        mRoot = new BTSelector(deadSeq, hitSeq, new BTSequence(detectCond, engage), patrolAction, idleAction);
    }

    private void Update()
    {
        mRoot.Tick();
    }

    private void ClearAllBools()
    {
        mAnim.SetBool("Patrol", false);
        mAnim.SetBool("Trace", false);
        mAnim.SetBool("Idle", false);
    }

    #region 플레이어 감지하기

    private bool DetectPlayer()
    {
        var hits = Physics.OverlapSphere(transform.position, mDetectRadius, mPlayerMask);
        if (hits.Length > 0)
        {
            mTarget = hits[0].transform;
            return true;
        }
        mTarget = null;
        return false;
    }

    #endregion
    
    #region 몬스터 Hit

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
    public void OnHitAnimationExit()
    {
        mbIsHit = false;
        if (mTarget == null)
        {
            DetectPlayer();
        }
        if (mAgent.enabled && mAgent.isOnNavMesh && mTarget != null)
        {
            mAnim.SetBool("Trace", true);
            mAgent.isStopped = false;
            mAgent.SetDestination(mTarget.position);
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
    
    #region 몬스터 공격

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
        {
            // 마지막 저장 위치로 발사
            ranged.FireLastPosition(transform, mLastAttackPosition);
        }
    }

    #endregion


    #region 디버깅

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
    }

    #endregion
    
}
