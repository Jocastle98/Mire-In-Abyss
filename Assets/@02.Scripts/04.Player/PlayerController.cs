using System;
using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour, IObserver<GameObject>
{
    [Header("Stat")]
    [SerializeField] private int mMaxHealth = 100;
    public int AttackPower => mAttackPower;
    [SerializeField] private int mAttackPower = 10;
    public int DefensePower => mDefensePower;
    [SerializeField] private int mDefensePower = 5;

    [Header("Action")] 
    [SerializeField] private LayerMask mGroundLayer;
    public float MoveSpeed => mMoveSpeed;
    [SerializeField] private float mMoveSpeed = 2.0f;
    public float TurnSpeed => mTurnSpeed;
    [SerializeField] private float mTurnSpeed = 10.0f;
    [SerializeField] private float mJumpForce = 7.5f;
    [SerializeField] private float mRollForce = 10.0f;
    public float DashForce => mDashForce;
    [SerializeField] private float mDashForce = 100.0f;

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
    private PlayerStateDash mPlayerStateDash;
    private PlayerStateSkill_1 mPlayerStateSkill_1;
    private PlayerStateSkill_2 mPlayerStateSkill_2;
    private PlayerStateSkill_3 mPlayerStateSkill_3;
    private PlayerStateSkill_4 mPlayerStateSkill_4;
    private PlayerStateInteraction mPlayerStateInteraction;
    private PlayerStateHit mPlayerStateHit;
    private PlayerStateDead mPlayerStateDead;
    
    public PlayerState CurrentPlayerState { get; private set; }
    private Dictionary<PlayerState, IPlayerState> mPlayerStates;
    
    // 외부에서 접근 가능한 변수
    public Animator PlayerAnimator { get; private set; }
    public PlayerGroundChecker mPlayerGroundChecker;
    public float walkAndRunSpeed { get; private set; } = 0.0f;
    
    // 내부에서만 사용되는 변수
    public Rigidbody Rigidbody => mRigidbody;
    private Rigidbody mRigidbody;
    public CapsuleCollider CapsuleCollider => mCapsuleCollider;
    private CapsuleCollider mCapsuleCollider;
    private PlayerInput mPlayerInput;
    private CameraController mCameraController;
    private WeaponController mWeaponController;
    private int mCurrentHealth = 0;

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
        mPlayerStateDash = new PlayerStateDash();
        mPlayerStateSkill_1 = new PlayerStateSkill_1();
        mPlayerStateSkill_2 = new PlayerStateSkill_2();
        mPlayerStateSkill_3 = new PlayerStateSkill_3();
        mPlayerStateSkill_4 = new PlayerStateSkill_4();
        mPlayerStateInteraction = new PlayerStateInteraction();
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
            { PlayerState.Dash, mPlayerStateDash },
            { PlayerState.Skill_1, mPlayerStateSkill_1 },
            { PlayerState.Skill_2, mPlayerStateSkill_2 },
            { PlayerState.Skill_3, mPlayerStateSkill_3 },
            { PlayerState.Skill_4, mPlayerStateSkill_4 },
            { PlayerState.Interaction, mPlayerStateInteraction },
            { PlayerState.Hit, mPlayerStateHit },
            { PlayerState.Dead, mPlayerStateDead },
        };
        
        // 상태 초기화
        Init();
        
        // 체력 초기화
        mCurrentHealth = mMaxHealth;
        
        // 무기 할당
        SetPlayerWeapon(mRightHandTransform, "Longsword", 
            mLeftHandTransform, "Shield");
    }

    private void Update()
    {
        if (CurrentPlayerState != PlayerState.None)
        {
            mPlayerStates[CurrentPlayerState].OnUpdate();
        }

        CheckGrounded();
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
        bool isGrounded = mPlayerGroundChecker.bIsGrounded;
        bool isJump = mPlayerStateJump.bIsJumping;
        bool isRoll = mPlayerStateRoll.bIsRolling;
        bool isAttack = mPlayerStateAttack.bIsAttacking;
        bool isDefend = mPlayerStateDefend.bIsDefending;
        bool isParry = mPlayerStateParry.bIsParrying;

        // 땅의 바로 위에 있으면서 5가지 중 아무 행동도 하지 않을 경우 다른 행동 가능
        if (isGrounded && !isJump && !isRoll && !isAttack && !isDefend && !isParry)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SetHit(TestEnemyController enemyController/*, Vector3 direction*/)
    {
        if (CurrentPlayerState != PlayerState.Hit)
        {
            //var enemyPower = enemyController.AttackPower;
            //mCurrentHealth -= enemyPower;
            
            // 체력바 감소
            //GameManager.Instance.SetHP((float)mCurrentHealth / mMaxHealth);

            if (mCurrentHealth <= 0)
            {
                SetPlayerState(PlayerState.Dead);
            }
            else
            {
                SetPlayerState(PlayerState.Hit);
                //PlayerAnimator.SetFloat("HitPosX", -direction.x);
                //PlayerAnimator.SetFloat("HitPosY", -direction.z);
            }
        }
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
            
            //GetGroundAngle();
        }
    }
    
    public Vector3 GetCameraForwardDirection()
    {
        // 카메라 설정
        var cameraTransform = Camera.main.transform;
        var cameraForward = cameraTransform.forward;
        
        // Y값을 0으로 설정해서 수평 방향만 고려
        cameraForward.y = 0;
        cameraForward.Normalize();
        
        return cameraForward;
    }
    
    public void SetCameraForwardRotate(Vector3 cameraForwardDirection, float angle)
    {
        Vector3 correctionRotation = Quaternion.Euler(0.0f, angle, 0.0f) * cameraForwardDirection;
        Quaternion targetRotation = Quaternion.LookRotation(correctionRotation);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, TurnSpeed * Time.deltaTime);
    }

    // 땅의 경사로에 따라 캐릭터의 수직 기울기 설정, 사용할지 몰라 나둠-> 미사용시 삭제
    public void GetGroundAngle()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 2.0f))
        {
            Vector3 surfaceNormal = hit.normal;
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, surfaceNormal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime);
        }
    }

    #endregion

    #region 점프 관련

    public void CheckGrounded()
    {
        PlayerAnimator.SetBool("IsGrounded", mPlayerGroundChecker.bIsGrounded);
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
        if (ActionCheck())
        {
            SetPlayerState(PlayerState.Idle);
        }
    }

    public void FallCheck()
    {
        if (!mPlayerGroundChecker.bIsGrounded && mRigidbody.velocity.y < -0.1f)
        {
            SetPlayerState(PlayerState.Fall);
        }
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

    // 장비 세팅 메서드
    private void SetPlayerWeapon(Transform rightHandTransform, string rightWeaponName,
        Transform leftHandTransform = null, string leftWeaponName = null)
    {
        var rightWeaponObject = Resources.Load<GameObject>($"Player/Weapons/{rightWeaponName}");
        var rightWeapon = Instantiate(rightWeaponObject, rightHandTransform).GetComponent<WeaponController>();
        rightWeapon.Subscribe(this);

        mWeaponController = rightWeapon;

        if (leftHandTransform != null)
        {
            var leftWeaponObject = Resources.Load<GameObject>($"Player/Weapons/{leftWeaponName}");
            var leftWeapon = Instantiate(leftWeaponObject, leftHandTransform);
        }
    }
    
    // 공격 애니메이션의 공격 모션 시작 시 호출 메서드
    public void MeleeAttackStart()
    {
        if (CurrentPlayerState == PlayerState.Attack)
        {
            mPlayerStateAttack.bIsAttacking = true;
            mPlayerStateAttack.bIsComboEnable = true;
            
            mWeaponController.AttackStart();
        }
    }

    // 공격 애니메이션의 공격 모션 종료 시 호출되는 메서드
    public void MeleeAttackEnd()
    {
        if (CurrentPlayerState == PlayerState.Attack)
        {
            mPlayerStateAttack.bIsAttacking = false;
            
            mWeaponController.AttackEnd();
        }
    }
    
    // 공격 애니메이션의 모션이 끝나기 직전 호출되는 메서드
    public void ComboEnd()
    {
        mPlayerStateAttack.bIsComboEnable = false;
    }

    public bool ComboAttackCheck()
    {
        return mPlayerStateAttack.bIsComboInputCheck;
    }

    public void SetComboInputFalse()
    {
        mPlayerStateAttack.bIsComboInputCheck = false;
    }

    public void DefendEnd()
    {
        mPlayerStateDefend.bIsDefending = false;
    }
    
    public void ParryStart()
    {
        mPlayerStateParry.bIsParrying = true;
    }

    public void ParryEnd()
    {
        mPlayerStateParry.bIsParrying = false;
    }

    public void OnNext(GameObject value)
    {
        var enemyController = value.GetComponent<TestEnemyController>();
        if (enemyController)
        {
            enemyController.SetHit(this);
        }
    }

    public void OnError(Exception error)
    {
        
    }

    public void OnCompleted()
    {
        mWeaponController.Unsubscribe(this);
    }

    #endregion

    #region 대시 관련

    public void Dash()
    {
        StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        var cameraForwardDirection = GetCameraForwardDirection();
        var dashDirection = (cameraForwardDirection + Vector3.up * 0.3f).normalized;

        Vector3 dashVelocity = dashDirection * DashForce;
        dashVelocity.y = mRigidbody.velocity.y; // 중력 유지
        mRigidbody.velocity = dashVelocity;

        yield return new WaitForSeconds(0.2f); // 대시 유지 시간

        // 감속 혹은 정지
        mRigidbody.velocity = new Vector3(0, mRigidbody.velocity.y, 0);
    }

    #endregion
}