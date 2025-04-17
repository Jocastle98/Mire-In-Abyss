using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBTController : MonoBehaviour
{
    [Header("감지 설정")]
    public float      detectRadius        = 10f;   // 감지 반경
    public float      detectAngle         = 360f;  // 감지 시야각
    public LayerMask  playerMask;                  // 플레이어 레이어 마스크

    [Header("순찰 설정")]
    public float      patrolRadius        = 5f;    // 순찰 범위

    [Header("공격 전략")]
    public ScriptableObject attackBehaviorAsset;   // 공격 전략 ScriptableObject
    private IAttackBehavior  attackBehavior;

    private NavMeshAgent _agent;
    private Animator     _anim;
    private Transform    _target;
    private BTNode       _root;

    // --- 새로 추가된 플래그 ---
    private bool _isAttacking = false;

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
            new BTCondition(() => false),                     // 체력<=0 체크 로직으로 대체
            new BTAction(() => _anim.SetTrigger("Dead"))
        );

        // 2) 피격
        var hitSeq = new BTSequence(
            new BTCondition(() => false),                     // 히트 플래그 체크 로직으로 대체
            new BTAction(() => _anim.SetTrigger("Hit"))
        );

        // 3) 플레이어 감지
        var detectCond = new BTCondition(DetectPlayer);

        // 4) 공격 시퀀스
        var attackSeq = new BTSequence(
            // 4-1) 범위 내에 있고, 아직 공격 중이 아닐 때만 실행
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
            if (_isAttacking) return;       // 공격 중엔 순찰하지 않는다
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

        // 7) 대기(Idle)
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

    // 플레이어 감지: 단순 OverlapSphere
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

    // 애니메이터 Bool 초기화
    private void ClearAllBools()
    {
        _anim.SetBool("Patrol", false);
        _anim.SetBool("Trace",  false);
        _anim.SetBool("Idle",   false);
    }

    // 기즈모: 감지/공격 범위 시각화
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

    // --- 공격 애니메이션 종료 콜백 ---
    // Attack 애니메이션 스테이트 MachineBehaviour 에서 호출하거나,
    // Animator 이벤트로 이 함수를 부르면 공격 플래그를 꺼줍니다.
    public void OnAttackAnimationExit()
    {
        _isAttacking = false;
    }
}
