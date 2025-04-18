using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBTController : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private int maxHealth = 100;
    private int _currentHealth;
    private bool _isDead = false;
    
    [Header("감지 설정")]
    public float      detectRadius        = 10f;  
    public float      detectAngle         = 360f;  
    public LayerMask  playerMask;                 

    [Header("순찰 설정")]
    public float      patrolRadius        = 5f;    

    [Header("공격 전략")]
    public ScriptableObject attackBehaviorAsset;   
    private IAttackBehavior  attackBehavior;

    private NavMeshAgent _agent;
    private Animator     _anim;
    private Transform    _target;
    private BTNode       _root;

    private bool _isAttacking = false;
    private bool _isHit       = false;
    [Header("렌더러 설정")]
    [SerializeField] private Renderer[] mRenderer;
    
    [Header("원거리 발사 위치")]
    [SerializeField] private Transform firePoint;
    public Transform FirePoint => firePoint;

    void Awake()
    {
        _agent         = GetComponent<NavMeshAgent>();
        _anim          = GetComponent<Animator>();
        attackBehavior = attackBehaviorAsset as IAttackBehavior;
        mRenderer = GetComponentsInChildren<Renderer>();
    }

    void Start()
    {
        // 체력 초기화
        _currentHealth = maxHealth;
        
        // 1) 사망
        var deadSeq = new BTSequence(
            new BTCondition(() => _isDead),
            new BTAction(() =>
            {
                ClearAllBools();
                _anim.SetTrigger("Dead");

                if (_agent.enabled && _agent.isOnNavMesh)
                {
                    _agent.isStopped = true;
                    _agent.enabled = false;
                }
                _isDead = false;
            })
        );

        // 2) 피격
        var hitSeq = new BTSequence(
            new BTCondition(() => _isHit),
            new BTAction(() => {
                _anim.SetTrigger("Hit");
                StartCoroutine(HitColorChange());
                if (_agent.enabled && _agent.isOnNavMesh)
                {
                    _agent.isStopped = true;
                    _agent.ResetPath();
                }
            })
        );

        // 3) 플레이어 감지
        var detectCond = new BTCondition(DetectPlayer);

        // 4) 공격 시퀀스
        var attackSeq = new BTSequence(
            new BTCondition(() =>
                !_isAttacking
                && attackBehavior != null
                && _target != null
                && attackBehavior.IsInRange(transform, _target)
            ),
            // 4-2) 이동 멈추고, 파라미터 초기화
            new BTAction(() =>
            {
                if (_agent.enabled && _agent.isOnNavMesh)
                {
                    _agent.isStopped = true;
                    _agent.velocity  = Vector3.zero;
                    _agent.ResetPath();
                }
                ClearAllBools();
            }),
            // 4-3) 실제 공격 트리거 + 플래그 세팅
            new BTAction(() =>
            {
                _isAttacking = true;
                attackBehavior.Attack(transform, _target);
            })
        );

        // 5) 추적 (공격 중이 아니고, 범위 밖일 때만)
        var traceAction = new BTAction(() =>
        {
            if (_isAttacking) return;
            if (_target == null) return;
            if (!attackBehavior.IsInRange(transform, _target))
            {
                ClearAllBools();
                _anim.SetBool("Trace", true);
                _agent.isStopped = false;
                _agent.SetDestination(_target.position);
            }
        });

        var engage = new BTSelector(attackSeq, traceAction);

        // 6) 순찰
        var patrolAction = new BTAction(() =>
        {
            if (_isAttacking) return;      
            ClearAllBools();
            _anim.SetBool("Patrol", true);
            _agent.isStopped = false;
            if (!_agent.hasPath || _agent.remainingDistance <= _agent.stoppingDistance)
            {
                var rnd = Random.insideUnitSphere * patrolRadius + transform.position;
                if (NavMesh.SamplePosition(rnd, out var hit, patrolRadius, NavMesh.AllAreas))
                    _agent.SetDestination(hit.position);
            }
        });

        // 7) 대기
        var idle = new BTAction(() =>
        {
            if (_isAttacking) return;
            ClearAllBools();
            _anim.SetBool("Idle", true);
            _agent.isStopped = true;
        });

        // 루트 구성
        _root = new BTSelector(
            deadSeq,
            hitSeq,
            new BTSequence(detectCond, engage),
            patrolAction,
            idle
        );
    }

    void Update()
    {
        _root.Tick();
    }

   

    private void ClearAllBools()
    {
        _anim.SetBool("Patrol", false);
        _anim.SetBool("Trace",  false);
        _anim.SetBool("Idle",   false);
    }

    #region 몬스터 공격 관련

    // Attack 애니메이션 스크립트에서 호출되는 함수
    public void OnAttackAnimationExit()
    {
        _isAttacking = false;
    }
    

    #endregion

    #region 플레이어 감지 관련

    private bool DetectPlayer()
    {
        var hits = Physics.OverlapSphere(transform.position, detectRadius, playerMask);
        if (hits.Length > 0)
        {
            _target = hits[0].transform;
            return true;
        }
        else
        {
            _target = null;
            return false;
        }
    }

    #endregion

    #region Dead 시 실행

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
            foreach (var rend in mRenderer)
            {
                rend.GetPropertyBlock(block);
                Color c = block.GetColor("_Color");
                c.a = clamped;
                block.SetColor("_Color", c);
                rend.SetPropertyBlock(block);
            }
            yield return null;
        }
        Destroy(gameObject);
    }

    #endregion

    #region Hit 시 실행

    public void SetHit(int damage)
    {
        if (_isDead) return;       
        _currentHealth -= damage;  
        Debug.Log("현재 체력: "+_currentHealth);

        if (_currentHealth <= 0)
        {
            _isDead = true;
        }
        else
        {
            _isHit = true;
        }
    }
    // Hit 애니메이션 스크립트에서 호출하는 함수
    public void OnHitAnimationExit() {
        _isHit = false;

        // 바로 플레이어 추격으로 복귀
        if (_agent.enabled && _agent.isOnNavMesh && _target != null) {
            _anim.SetBool("Trace", true);
            _agent.isStopped = false;
            _agent.SetDestination(_target.position);
        }
    }
    // Hit 상태에서 호출할 코루틴
    private IEnumerator HitColorChange()
    {
        var block = new MaterialPropertyBlock();
        float duration = 0.5f;           
        float elapsed  = 0f;

        ChangeColorRenderer(Color.red, block);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Color c = Color.Lerp(Color.red, Color.white, t);
            ChangeColorRenderer(c, block);
            yield return null;
        }

        ChangeColorRenderer(Color.white, block);
    }

    private void ChangeColorRenderer(Color color, MaterialPropertyBlock block)
    {
        foreach (var rend in mRenderer)
        {
            rend.GetPropertyBlock(block);
            block.SetColor("_Color", color);
            rend.SetPropertyBlock(block);
        }
    }

    #endregion

    #region 디버깅

    // 감지범위
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        if (attackBehaviorAsset is MeleeAttackBehavior melee)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, melee.range);
        }
        else if (attackBehaviorAsset is RangedAttackBehavior ranged)
        {
            Gizmos.color = Color.blue;  // 색깔은 마음대로
            Gizmos.DrawWireSphere(transform.position, ranged.range);
        }
        
    }

    #endregion

}
