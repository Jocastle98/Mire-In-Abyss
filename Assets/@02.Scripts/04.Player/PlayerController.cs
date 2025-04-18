using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Stat")]
    [SerializeField] private int mMaxHealth = 100;
    public int AttackPower => mAttackPower;
    [SerializeField] private int mAttackPower = 10;
    public int DefensePower => mDefensePower;
    [SerializeField] private int mDefensePower = 5;

    [Header("Action")] 
    [SerializeField] private LayerMask mGroundLayer;
    [SerializeField] private float mMaxGroundCheckDistance = 10.0f;
    public float MoveSpeed => mMoveSpeed;
    [SerializeField] private float mMoveSpeed = 2.0f;
    public float TurnSpeed => mTurnSpeed;
    [SerializeField] private float mTurnSpeed = 10.0f;
    [SerializeField] private float mJumpForce = 10.0f;
    [SerializeField] private float mRollForce = 10.0f;

    [Header("Attach Point")] 
    [SerializeField] private Transform mHeadTransform;
    [SerializeField] private Transform mRightHandTransform;
    [SerializeField] private Transform mLeftHandTransform;
    
    // 상태 관련
    private PlayerStateIdle mPlayerStateIdle;
    private PlayerStateMove mPlayerStateMove;
    private PlayerStateJump mPlayerStateJump;
    private PlayerStateFall mPlayerStateFall;
    private PlayerStateLand mPlayerStateLand;
    private PlayerStateRoll mPlayerStateRoll;
    private PlayerStateAttack mPlayerStateAttack;
    private PlayerStateDefend mPlayerStateDefend;
    private PlayerStateParry mPlayerStateParry;
    private PlayerStateHit mPlayerStateHit;
    private PlayerStateDead mPlayerStateDead;
    public PlayerState CurrentPlayerState { get; private set; }
    private Dictionary<PlayerState, IPlayerState> mPlayerStates;
    
    // 외부에서 접근 가능한 변수
    public Animator PlayerAnimator { get; private set; }
    public bool bIsGrounded { get { return GetDistanceToGround() < 0.1f; } }
    public float walkAndRunSpeed { get; private set; } = 0.0f;
    
    // 내부에서만 사용되는 변수
    private Rigidbody mRigidbody;
    private CapsuleCollider mCapsuleCollider;
    private PlayerInput mPlayerInput;
    private CameraController mCameraController;
    private int mCurrentHealth = 0;

    
    // 몬스터 타격 실험 지워도됨    
    [SerializeField] private WeaponHitboxController weaponHitbox;

    private void Awake()
    {
        PlayerAnimator = GetComponent<Animator>();
        mRigidbody = GetComponent<Rigidbody>();
        mCapsuleCollider = GetComponent<CapsuleCollider>();
        mPlayerInput = GetComponent<PlayerInput>();
    }
    
    private void Start()
    {
        mPlayerStateIdle = new PlayerStateIdle();
        mPlayerStateMove = new PlayerStateMove();
        mPlayerStateJump = new PlayerStateJump();
        mPlayerStateFall = new PlayerStateFall();
        mPlayerStateLand = new PlayerStateLand();
        mPlayerStateRoll = new PlayerStateRoll();
        mPlayerStateAttack = new PlayerStateAttack();
        mPlayerStateDefend = new PlayerStateDefend();
        mPlayerStateParry = new PlayerStateParry();
        mPlayerStateHit = new PlayerStateHit();
        mPlayerStateDead = new PlayerStateDead();

        mPlayerStates = new Dictionary<PlayerState, IPlayerState>
        {
            { PlayerState.Idle, mPlayerStateIdle },
            { PlayerState.Move, mPlayerStateMove },
            { PlayerState.Jump, mPlayerStateJump },
            { PlayerState.Fall, mPlayerStateFall },
            { PlayerState.Land, mPlayerStateLand },
            { PlayerState.Roll, mPlayerStateRoll },
            { PlayerState.Attack, mPlayerStateAttack },
            { PlayerState.Defend, mPlayerStateDefend },
            { PlayerState.Parry, mPlayerStateParry },
            { PlayerState.Hit, mPlayerStateHit },
            { PlayerState.Dead, mPlayerStateDead },
        };
        
        // 상태 초기화
        Init();
        
        // 체력 초기화
        mCurrentHealth = mMaxHealth;
        
        // 무기 할당
        
    }

    private void Update()
    {
        if (CurrentPlayerState != PlayerState.None)
        {
            mPlayerStates[CurrentPlayerState].OnUpdate();
            
            // 짦은 거리의 확인은 성능 문제가 크지 않다고 해서 Jump 상태에서 옮겨옴
            PlayerAnimator.SetFloat("GroundDistance", GetDistanceToGround());
        }
    }
    
    public void Init()
    {
        // InputSystem 초기화
        GameManager.Instance.Input.Init(mPlayerInput);
        
        SetPlayerState(PlayerState.Idle);
        mRigidbody.velocity = Vector3.zero;
        
        // Camera 설정
        mCameraController = Camera.main.GetComponent<CameraController>();
        mCameraController.SetTarget(mHeadTransform);
        
        // Player 체력 표시
    }

    public void SetPlayerState(PlayerState newPlayerState)
    {
        if (CurrentPlayerState != PlayerState.None)
        {
            mPlayerStates[CurrentPlayerState]?.OnExit();
        }
        CurrentPlayerState = newPlayerState;
        mPlayerStates[CurrentPlayerState].OnEnter(this);
    }

    public bool ActionCheck()
    {
        bool isJump = mPlayerStateJump.bIsJumping;
        bool isRoll = mPlayerStateRoll.bIsRolling;
        bool isAttack = mPlayerStateAttack.bIsAttacking;
        bool isDefend = mPlayerStateDefend.bIsDefending;
        bool isParry = mPlayerStateParry.bIsParrying;

        // 땅의 바로 위에 있으면서 5가지 중 아무 행동도 하지 않을 경우 다른 행동 가능
        if (bIsGrounded && !isJump && !isRoll && !isAttack && !isDefend && !isParry)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool ComboAttackCheck()
    {
        bool isComboEnable = mPlayerStateAttack.bIsComboEnable;
        bool isJump = mPlayerStateJump.bIsJumping;
        bool isRoll = mPlayerStateRoll.bIsRolling;
        bool isDefend = mPlayerStateDefend.bIsDefending;
        bool isParry = mPlayerStateParry.bIsParrying;
        
        if (bIsGrounded && isComboEnable && !isJump && !isRoll && !isDefend && !isParry)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // todo: 혹시 몰라 만들어 둠, 나중에도 사용 안하면 삭제 예정
    public void SetPlayerStateDelayed(PlayerState newPlayerState, float delay)
    {
        StartCoroutine(DelayedStateCoroutine(newPlayerState, delay));
    }

    private IEnumerator DelayedStateCoroutine(PlayerState newPlayerState, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetPlayerState(newPlayerState);
    }

    public void SetHit()
    {
        
    }

    #region 회전/이동 관련

    public void SetWalkAndRunSpeed(float newWalkAndRunSpeed)
    {
        walkAndRunSpeed = Mathf.Clamp01(newWalkAndRunSpeed);
    }

    public void SetPlayerMoveSpeed(float newPlayerMoveSpeed)
    {
        mMoveSpeed = newPlayerMoveSpeed;
    }

    public float AddMoveSpeed(float originMoveSpeed, float mSpeed)
    {
        return originMoveSpeed + (originMoveSpeed * 2.0f * (mSpeed / 1.0f));
    }
    
    public void Movement(float vertical, float horizontal)
    {
        // 카메라 설정
        var cameraTransform = Camera.main.transform;
        var cameraForward = cameraTransform.forward;
        var cameraRight = cameraTransform.right;
        
        // Y값을 0으로 설정해서 수평 방향만 고려
        cameraForward.y = 0;
        cameraRight.y = 0;
        
        // 입력 방향에 따라 카메라 기준으로 이동 방향 계산
        var moveDirection = ((cameraForward * vertical) + (cameraRight * horizontal)).normalized;
        
        // 이동 방향이 있을 경우에만 회전
        if (moveDirection != Vector3.zero)
        {
            // 현재 방향
            Quaternion currentRotation = transform.rotation;
            Quaternion targetRotation = default;
            
            if (vertical >= 0)
            {
                // 목표 방향
                targetRotation = Quaternion.LookRotation(moveDirection);
            }
            else if (vertical < 0)
            {
                // 목표 방향
                targetRotation = Quaternion.LookRotation(-moveDirection);
            }

            // 부드럽게 회전
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, mTurnSpeed * Time.deltaTime);

            if (ActionCheck())
            {
                transform.position += moveDirection * (mMoveSpeed * Time.deltaTime);
            }
        }
    }

    #endregion

    #region 점프 관련

    public float GetDistanceToGround()
    {
        // 너무 바닥에 딱 붙으면 오히려 감지 못하는 경우가 있음
        Vector3 rayOrigin = transform.position + Vector3.up * 0.001f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, mMaxGroundCheckDistance, mGroundLayer))
        {
            return hit.distance;
        }
        else
        {
            return mMaxGroundCheckDistance;
        }
    }
    
    public void Jump()
    {
        Vector2 moveInput = GameManager.Instance.Input.MoveInput;
        
        // 카메라 설정
        var cameraTransform = Camera.main.transform;
        var cameraForward = cameraTransform.forward;
        var cameraRight = cameraTransform.right;
        
        // Y값을 0으로 설정해서 수평 방향만 고려
        cameraForward.y = 0;
        cameraRight.y = 0;
        
        // 입력 방향에 따라 카메라 기준으로 이동 방향 계산
        var moveDirection = ((cameraForward * moveInput.y) + (cameraRight * moveInput.x)).normalized;
        
        mRigidbody.AddForce(Vector3.up * mJumpForce, ForceMode.Impulse);
        
        if (moveInput != Vector2.zero)
        {
            mRigidbody.AddForce(moveDirection * mJumpForce, ForceMode.Impulse);
            
            if (moveInput.y >= 0)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(-moveDirection);
            }
        }
        else
        {
            // 제자리 점프는 카메라 방향의 정면으로 점프
            transform.rotation = Quaternion.LookRotation(cameraForward);
        }
    }

    public void JumpStart()
    {
        mPlayerStateJump.bIsJumping = true;
    }

    public void JumpEnd()
    {
        mPlayerStateJump.bIsJumping = false;
    }

    #endregion

    #region 구르기 관련

    public void Roll()
    {
        StartCoroutine(RollCoroutine());
    }

    private IEnumerator RollCoroutine()
    {
        Vector2 moveInput = GameManager.Instance.Input.MoveInput;

        // 카메라 설정
        var cameraTransform = Camera.main.transform;
        var cameraForward = cameraTransform.forward;
        var cameraRight = cameraTransform.right;
        
        // Y값을 0으로 설정해서 수평 방향만 고려
        cameraForward.y = 0;
        cameraRight.y = 0;
        
        // 입력 방향에 따라 카메라 기준으로 이동 방향 계산
        var moveDirection = ((cameraForward * moveInput.y) + (cameraRight * moveInput.x)).normalized;
        
        // 애니메이션의 선 딜레이
        yield return new WaitForSeconds(0.2f);
        
        if (moveInput != Vector2.zero)
        {
            mRigidbody.AddForce(moveDirection * mRollForce, ForceMode.Impulse);
           
            if (moveInput.y >= 0)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(-moveDirection);
            }
        }
        else
        {
            // 방향 입력이 없을 때는 카메라 방향의 정면으로 구르기
            mRigidbody.AddForce(cameraForward * mRollForce, ForceMode.Impulse);
            transform.rotation = Quaternion.LookRotation(cameraForward);
        }
        
        yield return new WaitForSeconds(1.3f);
        mRigidbody.velocity = Vector3.zero;
    }
    
    public void RollStart()
    {
        if (CurrentPlayerState == PlayerState.Roll)
        {
            mPlayerStateRoll.bIsRolling = true;
        }
    }

    public void RollEnd()
    {
        if (CurrentPlayerState == PlayerState.Roll)
        {
            mPlayerStateRoll.bIsRolling = false;
        }
    }

    #endregion

    #region 공격 관련

    public void MeleeAttackStart()
    {
        if (CurrentPlayerState == PlayerState.Attack)
        {
            mPlayerStateAttack.bIsAttacking = true;
            mPlayerStateAttack.bIsComboEnable = true;
            // mWeaponController.AttackStart();
            
            // 몬스터 타격 실험 공격 애니메이션인데 이부분 이름 통일시켜도 될듯
            weaponHitbox.EnableHitbox(); 
        }
    }

    public void MeleeAttackEnd()
    {
        if (CurrentPlayerState == PlayerState.Attack)
        {
            mPlayerStateAttack.bIsAttacking = false;
            // mWeaponController.AttackEnd();
            
            //몬스터 타격 실험
            weaponHitbox.DisableHitbox();
        }
    }
    
    public void ComboEnd()
    {
        mPlayerStateAttack.bIsComboEnable = false;
    }

    public void ParryStart()
    {
        mPlayerStateParry.bIsParrying = true;
    }

    public void ParryEnd()
    {
        mPlayerStateParry.bIsParrying = false;
    }

    #endregion
}