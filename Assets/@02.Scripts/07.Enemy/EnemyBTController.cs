using System.Collections;
using Unity.VisualScripting;
using EnemyEnums;
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

    [Header("원거리 발사 위치 (원거리 스켈레톤, 드래곤)")]
    [SerializeField] private Transform mFirePoint;
    public Transform FirePoint => mFirePoint;
    
    [Header("임팩트 설정 (골렘)")]
    [SerializeField] private GameObject ImpactProjectorPrefab;
    [SerializeField] private LayerMask ImpactHitLayer;

    [Header("렌더러 설정")]  
    [SerializeField] private Renderer[] mRenderers;
    
    [Header("순찰 대기 시간 (초)")]
    [SerializeField] private float mPatrolWaitTime = 2f;
    private float mPatrolWaitTimer = 0f;
    private bool mPatrolPointAssigned = false;
    
    [Header("드래곤 공중")]
    [SerializeField] private float airInterval     = 30f; 
    [SerializeField] private float airDuration     = 10f; 
    private float mLastAirTime = -Mathf.Infinity;
    private bool  mIsFlying    = false;

    [Header("경험치 설정")] 
    [SerializeField] private EnemyType mEnemyType = EnemyType.Common;
    public EnemyType EnemyType => mEnemyType;
    [SerializeField] private EnemyExpRewardController mExpRewardController;
    
    private NavMeshAgent mAgent;
    private Animator mAnim;
    private BTNode mRoot;
    private bool mbIsAttacking;
    private bool mbIsHit;
    private bool mHasTriggeredDead;
    private Projector currentProjector;
    private bool mExpGiven = false;
    private ItemDropper itemDropper;

    
    void Awake()
    {
        mAgent = GetComponent<NavMeshAgent>();
        mAnim = GetComponent<Animator>();
        mAttackBehavior = mAttackBehaviorAsset as IAttackBehavior;
        mRenderers = GetComponentsInChildren<Renderer>();
        mExpRewardController = GetComponent<EnemyExpRewardController>();
        itemDropper = GetComponent<ItemDropper>();
    }

    void Start()
    {
        mCurrentHealth = mMaxHealth;
        
        var deadSeq = new BTSequence(
            new BTCondition(() => mbIsDead && !mHasTriggeredDead),
            new BTAction(() =>
            {
                mHasTriggeredDead = true;
                ClearAllBools();
                mAnim.SetTrigger("Dead");
                if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                    mAgent.isStopped = true;
                mAgent.enabled = false;
            })
        );

        var hitSeq = new BTSequence(
            new BTCondition(() => mbIsHit),
            new BTAction(() =>
            {
                ClearAllBools();
                mAnim.SetTrigger("Hit");
                StartCoroutine(HitColorChange());
                if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                    mAgent.isStopped = true;
                mbIsHit = false;
            })
        );

        var detectCond = new BTCondition(DetectPlayer);
        var flightSeq = new BTSequence(
            new BTCondition(() =>
                !mIsFlying
                &&!mbIsAttacking 
                && mAttackBehaviorAsset is DragonAttackBehavior
                && mCurrentHealth <= mMaxHealth * 0.5f
                && mTarget != null
                && (mAttackBehavior as DragonAttackBehavior)
                .IsInRange(transform, mTarget)
                && Time.time >= mLastAirTime + airInterval 
            ),
            new BTAction(() =>
            {
                mbIsAttacking = false;
                ClearAllBools();

                mIsFlying    = true;
                mLastAirTime = Time.time;
                mAnim.SetBool("IsFlying", true);
                if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                    mAgent.isStopped = true;
                mAgent.enabled = false;

                StartCoroutine(FlyAttackRoutine());
            })
        );
        

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
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
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
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
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
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                    {
                        mAgent.isStopped = false;
                        mAgent.SetDestination(mTarget.position);
                    }
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
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
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
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                    {
                        mAgent.isStopped = false;
                        mAgent.SetDestination(mTarget.position);
                    }
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
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
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
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                    {
                        mAgent.isStopped = false;
                        mAgent.SetDestination(mTarget.position);
                    }
                }
            });

            engage = new BTSelector(attackSeq, traceSeq);
        }
        else if (mAttackBehaviorAsset is DragonAttackBehavior)
        {
            var dragon = mAttackBehavior as DragonAttackBehavior;
            var fireballSeq = new BTSequence(
                new BTCondition(() => !mbIsAttacking && mTarget != null && dragon.CanFireball(transform, mTarget)),
                new BTAction(() =>
                {
                    mbIsAttacking = true;
                    ClearAllBools();
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                        mAgent.isStopped = true;
                    FaceTarget();
                    mAttackBehavior.Attack(transform, mTarget);
                })
            );

            var breathSeq = new BTSequence(
                new BTCondition(() => !mbIsAttacking && mTarget != null && dragon.CanBreath(transform, mTarget)),
                new BTAction(() =>
                {
                    mbIsAttacking = true;
                    ClearAllBools();
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                        mAgent.isStopped = true;
                    FaceTarget();
                    mAttackBehavior.Attack(transform, mTarget);
                })
            );

            var tailSeq = new BTSequence(
                new BTCondition(() => !mbIsAttacking && mTarget != null && dragon.CanTail(transform, mTarget)),
                new BTAction(() =>
                {
                    mbIsAttacking = true;
                    ClearAllBools();
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                        mAgent.isStopped = true;
                    FaceTarget();
                    mAttackBehavior.Attack(transform, mTarget);
                })
            );

            var traceSeq = new BTAction(() =>
            {
                if (mbIsAttacking || mTarget == null) return;
                float d = Vector3.Distance(transform.position, mTarget.position);
                if (d > dragon.FireballRange && d > dragon.BreathRange && d > dragon.TailRange)
                {
                    ClearAllBools();
                    mAnim.SetBool("Trace", true);
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                    {
                        mAgent.isStopped = false;
                        mAgent.SetDestination(mTarget.position);
                    }
                }
            });

            engage = new BTSelector(breathSeq, fireballSeq, tailSeq, traceSeq);
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
            if (mAgent == null || !mAgent.enabled || !mAgent.isOnNavMesh)
                return;

            if (mAgent.pathPending) return;
            if (!mPatrolPointAssigned)
            {
                var rnd = Random.insideUnitSphere * mPatrolRadius + transform.position;
                if (NavMesh.SamplePosition(rnd, out var hit, mPatrolRadius, NavMesh.AllAreas))
                {
                    mAgent.SetDestination(hit.position);
                    mPatrolPointAssigned = true;
                    mPatrolWaitTimer = 0f;
                }
            }
            else
            {
                if (mAgent.remainingDistance <= mAgent.stoppingDistance)
                {
                    mAgent.isStopped = true;
                    mAnim.SetBool("Idle", true);
                    mPatrolWaitTimer += Time.deltaTime;
                    if (mPatrolWaitTimer >= mPatrolWaitTime)
                    {
                        mPatrolPointAssigned = false;
                        mAgent.isStopped = false;
                    }
                }
                else
                {
                    mAgent.isStopped = false;
                    mAnim.SetBool("Patrol", true);
                    mPatrolWaitTimer = 0f;
                }
            }
        });

        var idle = new BTAction(() =>
        {
            if (mbIsAttacking) return;
            ClearAllBools();
            if (mAgent == null || !mAgent.enabled || !mAgent.isOnNavMesh)
                return;
            mAnim.SetBool("Idle", true);
            mAgent.isStopped = true;
        });

        mRoot = new BTSelector(deadSeq,flightSeq, hitSeq, new BTSequence(detectCond, engage), patrol, idle);
    }

    void Update()
    {
        if (mIsFlying)
            return;
        if (mHasTriggeredDead)   
            return;
        mRoot.Tick();
    }

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
        if (mAttackBehaviorAsset is DragonAttackBehavior)
        {
            mAnim.SetBool("FlyTrace", false);
        }
    }


    #region 드래곤 공중

    private IEnumerator FlyAttackRoutine()
    {
        mAnim.SetBool("IsFlying", true);
        yield return new WaitForSeconds(1.5f);
        

        mAgent.enabled = true;
        mAgent.isStopped = false;
        

        float t0 = Time.time;
        while (Time.time - t0 < airDuration)
        {
            if (DetectPlayer() && mTarget != null)
            {
                var dragon = mAttackBehavior as DragonAttackBehavior;

                if (dragon.CanFireball(transform, mTarget))
                {
                    ClearAllBools();
                    mAnim.SetTrigger("FlyFireBall");
                    FaceTarget();
                    yield return new WaitForSeconds(0.8f);
                }
                else
                {
                    ClearAllBools();
                    mAnim.SetBool("FlyTrace", true);
                    FaceTarget();
                    if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
                    {
                        mAgent.SetDestination(mTarget.position);
                    }
                    yield return null;
                    continue;
                }
            }
            yield return null;
        }

        mAgent.isStopped = true;
        mAgent.enabled = false;
        ClearAllBools();

        mAnim.SetTrigger("Land");
        yield return new WaitForSeconds(1.2f);

        mAnim.SetBool("IsFlying", false);
        mIsFlying = false;
        ClearAllBools();
    }


    #endregion
    
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
        if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
        {
            if (DetectPlayer())
            {
                mAnim.SetBool("Trace", true);
                mAgent.isStopped = false;
                mAgent.SetDestination(mTarget.position);
            }
        }
    }

    public void OnDeadAnimationExit()
    {
        itemDropper.DropItemOnDeadth();
        GiveExpReward();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            playerController.OnEnemyKilled();
        }
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

    public void GiveExpReward()
    {
        if (mExpGiven || mExpRewardController == null) return;
        PlayerLevelController playerLevelController = FindPlayerLevelController();

        int expAmount = mExpRewardController.GetExpReward(mEnemyType);
        
        playerLevelController.GainExperience(expAmount);
        mExpGiven = true;
    }

    private PlayerLevelController FindPlayerLevelController()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            return player.GetComponent<PlayerLevelController>();
        }

        return null;
    }

    #endregion

    #region Attack 이벤트
    public void OnAttackAnimationExit()
    {
        mbIsAttacking = false;
        ClearAllBools();
        if (mAgent != null && mAgent.enabled && mAgent.isOnNavMesh)
        {
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
    }

    public void FireProjectile()
    {
        if (mAttackBehaviorAsset is RangedAttackBehavior ranged) ranged.FireLastPosition(transform,
                mTarget != null ? mTarget.position : transform.position + transform.forward * 10f);
        else if (mAttackBehaviorAsset is DragonAttackBehavior dragon) dragon.FireLastPosition(transform);
    }

    public void OnMeleeAttack()
    {
        if (!(mAttackBehaviorAsset is MeleeAttackBehavior melee))
            return;

        // 플레이어 태그만 필터링
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            melee.Range,
            mPlayerMask        
        );
        foreach (var col in hits)
        {
            if (col.CompareTag("Player"))
            {
                col.GetComponent<PlayerController>()
                    .SetHit(melee.Damage, transform, 0);
            }
        }
    }

    #region 드래곤 공격
    public void OnBreathIndicator()
    {
        if (!(mAttackBehaviorAsset is DragonAttackBehavior dragon)) return;
        if (dragon.BreathProjectorPrefab == null) return;

        var go = Instantiate(dragon.BreathProjectorPrefab, mFirePoint);
        go.transform.localPosition = Vector3.up * 0.1f;
        currentProjector = go.GetComponent<Projector>();
        currentProjector.orthographicSize = 0f;

        StartCoroutine(ChargeBreathIndicator(dragon));
    }

    private IEnumerator ChargeBreathIndicator(DragonAttackBehavior dragon)
    {
        float duration = 1.2f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (currentProjector == null) yield break;
            currentProjector.orthographicSize = Mathf.Lerp(0f, dragon.BreathRange, elapsed / duration);
            yield return null;
        }
        if (currentProjector != null) currentProjector.orthographicSize = dragon.BreathRange;
    }

    public void OnTailAttack()
    {
        var dragon = mAttackBehavior as DragonAttackBehavior;
        var hits = Physics.OverlapSphere(transform.position, dragon.TailRange, dragon.HitLayer);
        foreach (var col in hits)
            if (col.CompareTag("Player"))
                col.GetComponent<PlayerController>().SetHit(dragon.TailDamage, transform, 1);
    }
    public void OnBreathLand()
    {
        if (currentProjector) Destroy(currentProjector.gameObject);
        currentProjector = null;

        if (mAttackBehaviorAsset is DragonAttackBehavior dragon)
        {
            if (dragon.BreathVFXPrefab != null) Instantiate(dragon.BreathVFXPrefab, transform.position, Quaternion.identity);
            Collider[] hits = Physics.OverlapSphere(transform.position, dragon.BreathRange, dragon.BreathHitLayer);
            float dotValue = Mathf.Cos(Mathf.Deg2Rad * (dragon.BreathAngle * 0.5f));
            foreach (var col in hits)
            {
                if (!col.CompareTag("Player")) continue;
                Vector3 direction = (col.transform.position - transform.position).normalized;
                if (Vector3.Dot(direction, transform.forward) > dotValue)
                    col.GetComponent<PlayerController>().SetHit(dragon.BreathDamage, transform, 2);
            }
        }
    }
    #endregion

    private IEnumerator ScaleUpProjector()
    {
        if (!(mAttackBehaviorAsset is GolemAttackBehavior golem)) yield break;
        float range = golem.ImpactRange;
        float duration = golem.ImpactChargeTime;
        System.Action onComplete = OnImpactLand;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentProjector.orthographicSize = Mathf.Lerp(0f, range, elapsed / duration);
            yield return null;
        }
        currentProjector.orthographicSize = range;
        onComplete?.Invoke();
        Destroy(currentProjector.gameObject, 0.5f);
    }

    #region 골렘 공격
    public void OnImpactIndicator()
    {
        if (ImpactProjectorPrefab == null || !(mAttackBehaviorAsset is GolemAttackBehavior) || mTarget == null) return;
        var go = Instantiate(ImpactProjectorPrefab);
        currentProjector = go.GetComponent<Projector>();
        go.transform.position = mTarget.position + Vector3.up * 5f;
        go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        StartCoroutine(ScaleUpProjector());
    }

    public void OnImpactLand()
    {
        var golem = mAttackBehaviorAsset as GolemAttackBehavior;
        var hits = Physics.OverlapSphere(transform.position, golem.ImpactRange, ImpactHitLayer);
        foreach (var col in hits)
            if (col.CompareTag("Player"))
                col.GetComponent<PlayerController>().SetHit(golem.ImpactDamage, transform, 1);
    }

    public void OnSwingAttack()
    {
        if (!(mAttackBehaviorAsset is GolemAttackBehavior golem)) return;
        var hits = Physics.OverlapSphere(transform.position, golem.SwingRange, ImpactHitLayer);
        foreach (var col in hits)
            if (col.CompareTag("Player"))
            {
                col.GetComponent<PlayerController>().SetHit(golem.SwingDamage, transform, 1);
                Debug.Log("스윙 데미지 적용");
            }
    }
    #endregion
    
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
