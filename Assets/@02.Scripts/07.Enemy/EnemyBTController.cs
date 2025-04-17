using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBTController : MonoBehaviour
{
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

    void Awake()
    {
        _agent         = GetComponent<NavMeshAgent>();
        _anim          = GetComponent<Animator>();
        attackBehavior = attackBehaviorAsset as IAttackBehavior;
    }

    void Start()
    {
        // 1) 사망
        var deadSeq = new BTSequence(
            new BTCondition(() => false),                    
            new BTAction(() => _anim.SetTrigger("Dead"))
        );

        // 2) 피격
        var hitSeq = new BTSequence(
            new BTCondition(() => _isHit),
            new BTAction(() => {
                _anim.SetTrigger("Hit");
                _agent.isStopped = true;
                _agent.ResetPath();
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
                _agent.isStopped = true;
                _agent.velocity  = Vector3.zero;
                _agent.ResetPath();
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

    private void ClearAllBools()
    {
        _anim.SetBool("Patrol", false);
        _anim.SetBool("Trace",  false);
        _anim.SetBool("Idle",   false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        if (attackBehaviorAsset is MeleeAttackBehavior melee)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, melee.range);
        }
    }

    public void OnAttackAnimationExit()
    {
        _isAttacking = false;
    }
    public void OnHitAnimationExit() {
        _isHit = false;

        // 바로 플레이어 추격으로 복귀
        if (_target != null)
        {
            ClearAllBools();
            _anim.SetBool("Trace", true);
            _agent.isStopped = false;
            _agent.SetDestination(_target.position);
        }
    }
    
    public void SetHit()
    {
        _isHit = true;
    }
}
