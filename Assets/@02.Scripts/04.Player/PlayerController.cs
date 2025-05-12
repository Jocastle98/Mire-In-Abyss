using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Events.Player;
using PlayerEnums;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour, IObserver<GameObject>
{
    [Header("Reference")]
    [SerializeField] private PlayerStats mPlayerStats;
    
    [Space(10)]
    [Header("Player Movement Stat")]
    [SerializeField] private float mCurrentSpeed;
    [SerializeField] private float mSpeedChangeRate = 10.0f;
    [SerializeField] private float mRotationSmoothTime = 0.12f;
    
    [Space(10)]
    [Header("Player Jump Stat")]
    [SerializeField] private float mGravity = - 9.81f;
    [SerializeField] private float mJumpHeight = 5.0f;
    [SerializeField] private float mJumpTimeout = 0.5f;
    [SerializeField] private float mJumpTimeoutDelta;
    public float JumpTimeoutDelta => mJumpTimeoutDelta;
    [SerializeField] private float mFallTimeout = 0.15f;
    [SerializeField] private float mFallTimeoutDelta;
    
    [Space(10)]
    [Header("Player Roll Stat")]
    [SerializeField] private float mRollDistance = 10.0f;
    [SerializeField] private float mRollFunctionDuration = 0.3f;
    [SerializeField] private float mRollTimeout = 3.0f;
    [SerializeField] private float mRollTimeoutDelta;
    public float RollTimeoutDelta => mRollTimeoutDelta;
    private Coroutine mRollCoroutine;
    
    [Space(10)]
    [Header("Player Dash Stat")]
    [SerializeField] private float mDashDistance = 15.0f;
    [SerializeField] private float mDashFunctionDuration = 0.3f;
    [SerializeField] private float mDashTimeout = 5.0f;
    [SerializeField] private float mDashTimeoutDelta;
    public float DashTimeoutDelta => mDashTimeoutDelta;
    private Coroutine mDashCoroutine;
    
    [Space(10)] 
    [Header("Player Attack Stat")]
    [SerializeField] private float mAttackSpeedMultiplier = 1.0f;

    [Space(10)] 
    [Header("Player Parry Stat")]
    [SerializeField] private float mParryDamageMultiplier = 3.0f;
    [SerializeField] private float mParryFunctionDuration = 0.2f;
    [SerializeField] private float mParrySuccessFunctionDuration = 0.5f;
    [SerializeField] private float mParryTimeout = 6.0f;
    [SerializeField] private float mParryTimeoutDelta;
    public float ParryTimeoutDelta => mParryTimeoutDelta;
    
    [Space(10)] 
    [Header("Player Skill_1 Stat")]
    [SerializeField] private float mSkill_1_DamageMultiplier = 1.5f;
    [SerializeField] private float mSkill_1_Distance = 20.0f;
    [SerializeField] private float mSkill_1_Timeout = 8.0f;
    [SerializeField] private float mSkill_1_TimeoutDelta;
    public float Skill_1_TimeoutDelta => mSkill_1_TimeoutDelta;
    
    [Space(10)] 
    [Header("Player Skill_2 Stat")]
    [SerializeField] private float mSkill_2_DamageMultiplier = 0.5f;
    [SerializeField] private float mSkill_2_Radius = 10.0f;
    [SerializeField] private float mSkill_2_Timeout = 10.0f;
    [SerializeField] private float mSkill_2_TimeoutDelta;
    public float Skill_2_TimeoutDelta => mSkill_2_TimeoutDelta;
    
    [Space(10)] 
    [Header("Player Skill_3 Stat")]
    [SerializeField] private float mSkill_3_DamageMultiplier = 2.0f;
    [SerializeField] private float mSkill_3_Radius = 5.0f;
    [SerializeField] private float mSkill_3_Timeout = 12.0f;
    [SerializeField] private float mSkill_3_TimeoutDelta;
    public float Skill_3_TimeoutDelta => mSkill_3_TimeoutDelta;
    
    [Space(10)] 
    [Header("Player Skill_4 Stat")]
    [SerializeField] private float mSkill_4_DamageMultiplier = 2.5f;
    [SerializeField] private float mSkill_4_Radius = 5.0f;
    [SerializeField] private float mSkill_4_Timeout = 25.0f;
    [SerializeField] private float mSkill_4_TimeoutDelta;
    public float Skill_4_TimeoutDelta => mSkill_4_TimeoutDelta;
    
    [Space(10)]
    [Header("Player Surroundings Check")]
    [SerializeField] private LayerMask mGroundLayers;
    [SerializeField] private float mGroundedOffset = -0.15f;
    [SerializeField] private float mGroundedRadius = 0.3f;
    public bool bIsGrounded { get; private set; }
    public InteractableObject NearestInteractableObject { get; private set; }

    [Space(10)]
    [Header("Player Combat Check")]
    [SerializeField] private float mInCombatTimeout = 10.0f;
    [SerializeField] private float mInCombatTimeoutDelta;
    [SerializeField] private bool mbInCombat = false;
    
    [Space(10)]
    [Header("Player Attach Point")]
    [SerializeField] private Transform mRightHandTransform;
    [SerializeField] private Transform mLeftHandTransform;
    
    [Space(10)]
    [Header("Player AudioClips")]
    public AudioClip[] footstepAudioClips;
    public AudioClip landingAudioClip;
    
    // Player Internal Calculation Stat
    private float mVerticalVelocity;
    private float mRotationVelocity;
    private float mTerminalVelocity = 53.0f;
    private float mTargetRotation;
    private float mAnimationBlend;
    private bool mbIsDamageReduced;
    
    // Componenet
    public Animator PlayerAnimator { get; private set; }
    private CharacterController mCharacterController;
    private GameObject mMainCamera;
    private WeaponController mWeaponController;
    
    // Effect
    private GameObject mSlash_Effect_Prefab;
    private GameObject mSkill_1_Effect_Prefab;
    private GameObject mSkill_2_Effect_Prefab;
    private GameObject mSkill_3_Effect_Prefab;
    private GameObject mSkill_4_RangeIndicator_Prefab;
    private GameObject mSkill_4_Effect_Prefab;
    
    // State
    private PlayerStateIdle mPlayerStateIdle;
    private PlayerStateMove mPlayerStateMove;
    private PlayerStateJump mPlayerStateJump;
    private PlayerStateFall mPlayerStateFall;
    private PlayerStateLand mPlayerStateLand;
    private PlayerStateRoll mPlayerStateRoll;
    private PlayerStateDash mPlayerStateDash;
    private PlayerStateAttack mPlayerStateAttack;
    private PlayerStateDefend mPlayerStateDefend;
    private PlayerStateParry mPlayerStateParry;
    private PlayerStateSkill_1 mPlayerStateSkill_1;
    private PlayerStateSkill_2 mPlayerStateSkill_2;
    private PlayerStateSkill_3 mPlayerStateSkill_3;
    private PlayerStateSkill_4 mPlayerStateSkill_4;
    private PlayerStateInteraction mPlayerStateInteraction;
    private PlayerStateStun mPlayerStateStun;
    private PlayerStateFreeze mPlayerStateFreeze;
    private PlayerStateDead mPlayerStateDead;
    public PlayerState CurrentPlayerState { get; private set; }
    private Dictionary<PlayerState, IPlayerState> mPlayerStates;
    
    private void Awake()
    {
        PlayerAnimator = GetComponent<Animator>();
        mCharacterController = GetComponent<CharacterController>();
        if (Camera.main != null)
        {
            mMainCamera = Camera.main.gameObject;
        }
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        if (CurrentPlayerState != PlayerState.None)
        {
            mPlayerStates[CurrentPlayerState].OnUpdate();
        }
        
        GroundedCheck();
        TimeoutCheck();
    }

    private void FixedUpdate()
    {
        if (CurrentPlayerState != PlayerState.Skill_4)
        {
            CalculateGravity();
        }
    }

    /// <summary>
    /// 플레이어 캐릭터 초기화 메서드
    /// </summary>
    public void Init()
    {
        StateInit();
        TimeoutInit();

        // 공격 속도 설정
        PlayerAnimator.SetFloat("AttackSpeed", mAttackSpeedMultiplier);
        
        // 무기 할당
        SetPlayerWeapon(mRightHandTransform, "Longsword", mLeftHandTransform, "Shield");
    }

    private void StateInit()
    {
        mPlayerStateIdle = new PlayerStateIdle();
        mPlayerStateMove = new PlayerStateMove();
        mPlayerStateJump = new PlayerStateJump();
        mPlayerStateFall = new PlayerStateFall();
        mPlayerStateLand = new PlayerStateLand();
        mPlayerStateRoll = new PlayerStateRoll();
        mPlayerStateDash = new PlayerStateDash();
        mPlayerStateAttack = new PlayerStateAttack();
        mPlayerStateDefend = new PlayerStateDefend();
        mPlayerStateParry = new PlayerStateParry();
        mPlayerStateSkill_1 = new PlayerStateSkill_1();
        mPlayerStateSkill_2 = new PlayerStateSkill_2();
        mPlayerStateSkill_3 = new PlayerStateSkill_3();
        mPlayerStateSkill_4 = new PlayerStateSkill_4();
        mPlayerStateInteraction = new PlayerStateInteraction();
        mPlayerStateStun = new PlayerStateStun();
        mPlayerStateFreeze = new PlayerStateFreeze();
        mPlayerStateDead = new PlayerStateDead();

        mPlayerStates = new Dictionary<PlayerState, IPlayerState>
        {
            { PlayerState.Idle, mPlayerStateIdle },
            { PlayerState.Move, mPlayerStateMove },
            { PlayerState.Jump, mPlayerStateJump },
            { PlayerState.Fall, mPlayerStateFall },
            { PlayerState.Land, mPlayerStateLand },
            { PlayerState.Roll, mPlayerStateRoll },
            { PlayerState.Dash, mPlayerStateDash },
            { PlayerState.Attack, mPlayerStateAttack },
            { PlayerState.Defend, mPlayerStateDefend },
            { PlayerState.Parry, mPlayerStateParry },
            { PlayerState.Skill_1, mPlayerStateSkill_1 },
            { PlayerState.Skill_2, mPlayerStateSkill_2 },
            { PlayerState.Skill_3, mPlayerStateSkill_3 },
            { PlayerState.Skill_4, mPlayerStateSkill_4 },
            { PlayerState.Interaction, mPlayerStateInteraction },
            { PlayerState.Stun, mPlayerStateStun },
            { PlayerState.Freeze, mPlayerStateFreeze },
            { PlayerState.Dead, mPlayerStateDead },
        };

        mPlayerStats.OnDeath += () =>
        {
            SetPlayerState(PlayerState.Dead);
        };
        
        SetPlayerState(PlayerState.Idle);
    }

    private void TimeoutInit()
    {
        mJumpTimeoutDelta = mJumpTimeout;
        mFallTimeoutDelta = mFallTimeout;
        mRollTimeoutDelta = 0.0f;
        mDashTimeoutDelta = 0.0f;
        mSkill_1_TimeoutDelta = 0.0f;
        mSkill_2_TimeoutDelta = 0.0f;
        mSkill_3_TimeoutDelta = 0.0f;
        mSkill_4_TimeoutDelta = 0.0f;
    }
    
    /// <summary>
    /// 플레이어 캐릭터 상태전환 메서드
    /// </summary>
    /// <param name="newPlayerState"> 전환하려고 하는 상태머신 </param>
    public void SetPlayerState(PlayerState newPlayerState)
    {
        if (CurrentPlayerState != PlayerState.None)
        {
            mPlayerStates[CurrentPlayerState]?.OnExit();
        }
        CurrentPlayerState = newPlayerState;
        mPlayerStates[CurrentPlayerState].OnEnter(this);
    }

    #region 감지 관련 기능

    /// <summary>
    /// 플레이어 캐릭터의 발 아래에 땅 레이어 감지 메서드
    /// </summary>
    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - mGroundedOffset, transform.position.z);
        bool isGrounded = Physics.CheckSphere(spherePosition, mGroundedRadius, mGroundLayers, QueryTriggerInteraction.Ignore);

        if (bIsGrounded != isGrounded)
        {
            bIsGrounded = isGrounded;
            PlayerAnimator.SetBool("IsGrounded", bIsGrounded);
            R3EventBus.Instance.Publish(new PlayerGrounded(bIsGrounded));
        }
    }
   
    public void SetNearestInteractable(InteractableObject obj)
    {
        NearestInteractableObject = obj;
    }
    
    #endregion
    
    #region 계산 관련 기능

    private void TimeoutCheck()
    {
        // Jump 관련 Timeout
        if (bIsGrounded)
        {
            mFallTimeoutDelta = mFallTimeout;
             
            if (mJumpTimeoutDelta >= 0.0f)
            {
                mJumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            mJumpTimeoutDelta = mJumpTimeout;
            
            if (mFallTimeoutDelta >= 0.0f)
            {
                mFallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                if (CurrentPlayerState == PlayerState.Attack)
                {
                    Fall();
                }
                else if (CurrentPlayerState == PlayerState.Dash || 
                         CurrentPlayerState == PlayerState.Skill_1 || CurrentPlayerState == PlayerState.Skill_2 ||
                         CurrentPlayerState == PlayerState.Skill_3 || CurrentPlayerState == PlayerState.Skill_4)
                {
                    mVerticalVelocity = 0.0f;
                }
                else
                {
                    SetPlayerState(PlayerState.Fall);
                }
            }
        }
        
        // Combat 관련 Timeout
        if (mbInCombat)
        {
            mInCombatTimeoutDelta -= Time.deltaTime;
            if (mInCombatTimeoutDelta <= 0.0f)
            {
                mbInCombat = false;
            }
        }
        
        // Roll 관련 Timeout
        if (mRollTimeoutDelta >= 0.0f)
        {
            mRollTimeoutDelta -= Time.deltaTime;
        }
        
        // Dash 관련 Timeout
        if (mDashTimeoutDelta >= 0.0f)
        {
            mDashTimeoutDelta -= Time.deltaTime;
        }
        
        // Parry 관련 Timeout
        if (mParryTimeoutDelta >= 0.0f)
        {
            mParryTimeoutDelta -= Time.deltaTime;
        }
        
        // Skill_1 관련 Timeout
        if (mSkill_1_TimeoutDelta >= 0.0f)
        {
            mSkill_1_TimeoutDelta -= Time.deltaTime;
        }
        
        // Skill_2 관련 Timeout
        if (mSkill_2_TimeoutDelta >= 0.0f)
        {
            mSkill_2_TimeoutDelta -= Time.deltaTime;
        }
        
        // Skill_3 관련 Timeout
        if (mSkill_3_TimeoutDelta >= 0.0f)
        {
            mSkill_3_TimeoutDelta -= Time.deltaTime;
        }
        
        // Skill_4 관련 Timeout
        if (mSkill_4_TimeoutDelta >= 0.0f)
        {
            mSkill_4_TimeoutDelta -= Time.deltaTime;
        }
    }
    
    private void CalculateGravity()
    {
        if (bIsGrounded)
        {
            if (mVerticalVelocity < 0.0f)
            {
                mVerticalVelocity = -2.0f;
            }
        }
        
        if (mVerticalVelocity < mTerminalVelocity)
        {
            mVerticalVelocity += mGravity * Time.deltaTime;
        }
    }
    
    #endregion
    
    #region 회전/이동 관련 기능
    
    /// <summary>
    /// 카메라 기준 정면 방향을 계산 하는 메서드
    /// </summary>
    /// <param name="isHorizontalOnly"> y값을 0으로 만들어 수평 방향만을 계산할지 여부 </param>
    /// <returns></returns>
    public Vector3 GetCameraForwardDirection(bool isHorizontalOnly)
    {
        Vector3 cameraForward = mMainCamera.transform.forward;
        
        // Y값을 0으로 설정해서 수평 방향만 고려
        if (isHorizontalOnly)
        {
            cameraForward.y = 0;
        }
        
        cameraForward.Normalize();
        
        return cameraForward;
    }

    /// <summary>
    /// 구르기나 돌진기 등의 방향을 설정해 주는 메서드
    /// </summary>
    /// <param name="useMoveInput"> 이동 입력을 사용할지 여부 (구르기=O, 돌진=X) </param>
    /// <param name="isGroundOnly"> 지상 전용 동작인지 여부 (구르기=O, 돌진=X) </param>
    /// <returns> 방향 벡터 반환 </returns>
    public Vector3 GetActionDirection(bool useMoveInput, bool isGroundOnly)
    {
        Vector3 targetDirection = Vector3.zero;
        Vector2 moveInput = GameManager.Instance.Input.MoveInput;
        
        if (isGroundOnly || bIsGrounded)
        {
            if (useMoveInput && moveInput != Vector2.zero)
            {
                Vector3 inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;
                float targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mMainCamera.transform.eulerAngles.y;

                targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
            }
            else
            {
                targetDirection = GetCameraForwardDirection(true);
            }
        }
        else
        {
            targetDirection = GetCameraForwardDirection(false);
        }

        return targetDirection;
    }
    
    /// <summary>
    /// 플레이어 캐릭터를 이동시켜주는 메서드
    /// </summary>
    /// <param name="allowRotation"> moveInput 방향으로 회전을 시킬지 여부 </param>
    private void CalculateMovement(bool allowRotation)
    {
        Vector2 moveInput = GameManager.Instance.Input.MoveInput;
        
        // 1. 목표 속도 설정
        float targetSpeed = SetSpeed();
        
        // 2. 현재 속도 측정 (수평 이동만 고려) 및 목표 속도로 증감
        mCurrentSpeed = HandleSpeed(targetSpeed);
        
        // 3. 플레이어 캐릭터 회전
        HandleRotation(allowRotation);
    
        // 4. 플레이어 캐릭터 목표 방향으로 이동
        HandleMovement(moveInput);
        
        // 5. 애니메이션 블렌드 계산
        HandleAnimatorBlend(moveInput);
    }

    #region CalculateMovement 하위 기능

    private float SetSpeed()
    {
        float targetSpeed;
        
        if (CurrentPlayerState != PlayerState.Defend)
        {
            if (GameManager.Instance.Input.SprintInput)
            {
                 targetSpeed = mPlayerStats.GetMoveSpeed() * 1.5f;
                 SetCombatState(false);
            }
            else
            {
                targetSpeed = mPlayerStats.GetMoveSpeed();
            }
        }
        else
        {
            targetSpeed = mPlayerStats.GetMoveSpeed() * 0.5f;
        }

        return targetSpeed;
    }

    private float HandleSpeed(float targetSpeed)
    {
        float currentHorizontalSpeed = new Vector3(mCharacterController.velocity.x, 0, mCharacterController.velocity.z).magnitude;
        float speedOffset = 0.1f;
        float speed;
        
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * mSpeedChangeRate);
        }
        else
        {
            speed = targetSpeed;
        }
        
        return speed;
    }
    
    private void HandleRotation(bool allowRotation)
    {
        if (mbInCombat)
        {
            transform.rotation = Quaternion.LookRotation(GetCameraForwardDirection(true));
        }
        else if (allowRotation)
        {
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, mTargetRotation, ref mRotationVelocity, mRotationSmoothTime);
            Vector3 rotationDirection =new Vector3(0.0f, rotation, 0.0f);
            transform.rotation = Quaternion.Euler(rotationDirection);
        }
    }

    private void HandleMovement(Vector2 moveInput)
    {
        Vector3 inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;
        mTargetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mMainCamera.transform.eulerAngles.y;
        Vector3 targetDirection = Quaternion.Euler(0.0f, mTargetRotation, 0.0f) * Vector3.forward;
        
        if (CurrentPlayerState == PlayerState.Move)
        {
            mCharacterController.Move(targetDirection.normalized * (mCurrentSpeed * Time.deltaTime) + new Vector3(0.0f, mGravity, 0.0f) * Time.deltaTime);
        }
        else
        {
            mCharacterController.Move(targetDirection.normalized * (mCurrentSpeed * Time.deltaTime) + new Vector3(0.0f, mVerticalVelocity, 0.0f) * Time.deltaTime);
        }
    }
    
    private void HandleAnimatorBlend(Vector2 moveInput)
    {
        // 애니메이션의 수평/수직 이동 블렌드
        if (mbInCombat)
        {
            float currentVertical = PlayerAnimator.GetFloat("Vertical");
            float currentHorizontal = PlayerAnimator.GetFloat("Horizontal");
            
            float smoothedVertical = Mathf.Lerp(currentVertical, moveInput.y, Time.deltaTime * mSpeedChangeRate);
            float smoothedHorizontal = Mathf.Lerp(currentHorizontal, moveInput.x, Time.deltaTime * mSpeedChangeRate);
            
            PlayerAnimator.SetFloat("Vertical", smoothedVertical);
            PlayerAnimator.SetFloat("Horizontal", smoothedHorizontal);
        }
        else
        {
            PlayerAnimator.SetFloat("Vertical", 1.0f);
            PlayerAnimator.SetFloat("Horizontal", 0.0f);
        }
        
        // 애니메이션의 이동 속도 블랜드
        float currentSpeed = PlayerAnimator.GetFloat("Speed");
        float smoothedSpeed = Mathf.Lerp(currentSpeed, mCurrentSpeed, Time.deltaTime * mSpeedChangeRate);
        if (smoothedSpeed < 0.01f)
        {
            smoothedSpeed = 0f;
        }
        PlayerAnimator.SetFloat("Speed", smoothedSpeed);
    }
    
    #endregion
    
    public void Idle()
    {
        mCurrentSpeed = 0.0f;
        
        if (CurrentPlayerState == PlayerState.Idle)
        {
            mCharacterController.Move(new Vector3(0.0f, mGravity, 0.0f) * Time.deltaTime);
        }
        else
        {
            mCharacterController.Move(new Vector3(0.0f, mVerticalVelocity, 0.0f) * Time.deltaTime);
        }
        
        PlayerAnimator.SetFloat("Vertical", 0.0f);
        PlayerAnimator.SetFloat("Horizontal", 0.0f);
        PlayerAnimator.SetFloat("Speed", mCurrentSpeed);
    }

    public void Move()
    {
        CalculateMovement(true);
    }

    private void BattleMove()
    {
        CalculateMovement(false);
    }
    
    #endregion

    #region 점프 관련 기능
    
    public void Jump()
    {
        mVerticalVelocity = 0.0f;
        mVerticalVelocity = Mathf.Sqrt(mJumpHeight * -2.0f * mGravity);

        Vector3 targetDirection = Quaternion.Euler(0.0f, mTargetRotation, 0.0f) * Vector3.forward;
        mCharacterController.Move(targetDirection.normalized * (mCurrentSpeed * Time.deltaTime) 
                                  + new Vector3(0.0f, mVerticalVelocity, 0.0f) * Time.deltaTime);
    }
    
    private bool mbInDirection = false;
    public void Fall()
    {
        Vector3 moveDirection = Vector3.zero;
        
        if (!mbInDirection)
        {
            Vector3 targetDirection = Quaternion.Euler(0.0f, mTargetRotation, 0.0f) * Vector3.forward;
            moveDirection = targetDirection.normalized * mCurrentSpeed;
        }
        mCharacterController.Move(moveDirection * Time.deltaTime 
                                  + new Vector3(0.0f, mVerticalVelocity, 0.0f) * Time.deltaTime);
    }

    public void Land()
    {
        // 착지 후 딜레이
        Invoke("Landing", 0.3f);
    }

    private void Landing()
    {
        mbInDirection = false;
        SetPlayerState(PlayerState.Idle);
    }
    
    #endregion

    #region 구르기 관련 기능
    
    public void StartRoll(Vector3 targetDirection)
    {
        if (mRollCoroutine == null || !mPlayerStateRoll.bIsRoll)
        {
            mRollCoroutine = StartCoroutine(RollCoroutine(targetDirection));
        }
    }
    
    public void StopRoll()
    {
        if (mRollCoroutine != null)
        {
            StopCoroutine(mRollCoroutine);
            mRollCoroutine = null;
        }
        
        mRollTimeoutDelta = mRollTimeout;
    }

    private IEnumerator RollCoroutine(Vector3 targetDirection)
    {
        float startupTime = 0.0f;
        float recoveryTime = 0.0f;
        
        mPlayerStateRoll.bIsRoll = true;

        yield return new WaitForSeconds(startupTime); // 선딜(현재 애니메이션 선딜 없음)
        RollFunction(true);
     
        StartCoroutine(RollingCoroutine(targetDirection));
        
        yield return new WaitForSeconds(mRollFunctionDuration); // 무적시간?
        RollFunction(false);

        yield return null;
        int mobilityLayer = PlayerAnimator.GetLayerIndex("Mobility Layer");
        var rollAnimationInfo = PlayerAnimator.GetCurrentAnimatorStateInfo(mobilityLayer);
        if (rollAnimationInfo.IsName("Roll"))
        {
            float rollEndTime = Mathf.Max(0.0f, rollAnimationInfo.length - startupTime - recoveryTime - mRollFunctionDuration);
            yield return new WaitForSeconds(rollEndTime);
            
            mPlayerStateRoll.bIsRoll = false;

            if (bIsGrounded)
            {
                if (GameManager.Instance.Input.MoveInput == Vector2.zero)
                {
                    SetPlayerState(PlayerState.Idle);
                }
                else
                {
                    SetPlayerState(PlayerState.Move);
                }
            }
            else
            {
                SetPlayerState(PlayerState.Fall);
            }
        }
        else
        {
            // 만약 아직 롤 애니메이션이 아니면, 약간 대기 후 다시 체크
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    // mRollDistance 만큼 구르기 동작을 수행하는 코루틴 메서드
    private IEnumerator RollingCoroutine(Vector3 targetDirection)
    {
        float distanceCovered = 0.0f;
        float maxDistance = mRollDistance;
        float speed = 10.0f; // 초기 속도 조정
        
        while (distanceCovered < maxDistance)
        {
            float moveAmount = speed * Time.deltaTime;

            if (distanceCovered + moveAmount > maxDistance)
            {
                moveAmount = maxDistance - distanceCovered;
            }
            
            mCharacterController.Move(targetDirection * moveAmount);
            distanceCovered += moveAmount;
            yield return null;
        }
    }

    // 무적
    private void RollFunction(bool isRollFunction)
    {
        if (isRollFunction)
        {
            // 무적 시작
            mbIsDamageReduced = true;
            OverrideDamageReduction = 1.0f;
            Debug.Log("무적 시작");
        }
        else
        {
            // 무적 끝
            mbIsDamageReduced = false;
            OverrideDamageReduction = 0.0f;
            Debug.Log("무적 끝");
        }
    }
    
    #endregion
    
    #region 돌진기 관련 기능
    
    public void StartDash(Vector3 cameraCenterDirection)
    {
        // 돌진 이후의 낙하 방향 설정
        Vector2 moveInput = GameManager.Instance.Input.MoveInput;
        Vector3 inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;
        mTargetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mMainCamera.transform.eulerAngles.y;
        
        if (mDashCoroutine == null || !mPlayerStateDash.bIsDashing)
        {
            mDashCoroutine = StartCoroutine(DashCoroutine(cameraCenterDirection));
        }
    }

    public void StopDash()
    {
        if (mDashCoroutine != null)
        {
            StopCoroutine(mDashCoroutine);
            mDashCoroutine = null;
        }
        
        mDashTimeoutDelta = mDashTimeout;
    }
    
    private IEnumerator DashCoroutine(Vector3 cameraCenterDirection)
    {
        float dashEndOffset = 0.5f;
        float firstDelay = 0.0f;
        
        mPlayerStateDash.bIsDashing = true;
        
        // 애니메이션 선딜
        yield return new WaitForSeconds(firstDelay);
        DashFunction(true);
        
        StartCoroutine(DashingCoroutine(cameraCenterDirection));
        
        yield return new WaitForSeconds(mDashFunctionDuration);
        DashFunction(false);
        
        yield return null;
        int mobilityLayer = PlayerAnimator.GetLayerIndex("Mobility Layer");
        var dashAnimationInfo = PlayerAnimator.GetCurrentAnimatorStateInfo(mobilityLayer);
        if (dashAnimationInfo.IsName("Dash"))
        {
            float dashAnimationLength = dashAnimationInfo.length;
            float dashEndTime = Mathf.Max(0.0f, dashAnimationLength - dashEndOffset - firstDelay - mDashFunctionDuration);
            yield return new WaitForSeconds(dashEndTime);
            
            mPlayerStateDash.bIsDashing = false;
        
            if (bIsGrounded)
            {
                if (GameManager.Instance.Input.MoveInput == Vector2.zero)
                {
                    SetPlayerState(PlayerState.Idle);
                }
                else if (GameManager.Instance.Input.MoveInput != Vector2.zero)
                {
                    SetPlayerState(PlayerState.Move);
                }
            }
            else
            {
                mbInDirection = true;
                
                SetPlayerState(PlayerState.Fall);
            }
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
        }
    }

    public IEnumerator DashingCoroutine(Vector3 cameraCenterDirection)
    {
        if (mPlayerStateDash.bIsDashing)
        {
            float distanceCovered = 0.0f;
            float maxDistance = mDashDistance;
            float speed = 50.0f; // 초기 속도 조정

            while (distanceCovered < maxDistance)
            {
                float moveAmount = speed * Time.deltaTime;

                if (distanceCovered + moveAmount > maxDistance)
                {
                    moveAmount = maxDistance - distanceCovered;
                }
                
                mCharacterController.Move(cameraCenterDirection * moveAmount);
                distanceCovered += moveAmount;
                yield return null;
            }

            GameObject slashEffectObject = SlashEffect(SlashEffectType.LeftToRight);
            Destroy(slashEffectObject, 0.2f);
        }
    }

    // 적 레이어와 충돌 무시
    private void DashFunction(bool isDashFunction)
    {
        if (isDashFunction)
        {
            mCharacterController.excludeLayers = LayerMask.GetMask("Enemy");
        }
        else
        {
            mCharacterController.excludeLayers = LayerMask.GetMask("Nothing");
        }
    }
    
    #endregion
    
    #region 공격 관련 기능

    // 검방 캐릭에 대한 무기 세팅 메서드(오른쪽 한손 검만 무기라는 전제의 메서드)
    private void SetPlayerWeapon(Transform rightHandTransform, string rightWeaponName,
        Transform leftHandTransform = null, string leftWeaponName = null)
    {
        var rightWeaponObject = Resources.Load<GameObject>($"Player/Weapons/{rightWeaponName}");
        var rightWeapon = Instantiate(rightWeaponObject, rightHandTransform).GetComponent<WeaponController>();
        rightWeapon.Subscribe(this);

        mWeaponController = rightWeapon;
        mWeaponController.SetPlayer(this);
        int weaponPower = mWeaponController.GetWeaponPower();
        mPlayerStats.ModifyAttackPower(weaponPower, "flat");

        if (leftHandTransform != null)
        {
            var leftWeaponObject = Resources.Load<GameObject>($"Player/Weapons/{leftWeaponName}");
            var leftWeapon = Instantiate(leftWeaponObject, leftHandTransform);
        }
    }

    // 전투돌입 상태를 설정해주는 메서드
    private void SetCombatState(bool inCombat)
    {
        mbInCombat = inCombat;
        mInCombatTimeoutDelta = inCombat ? mInCombatTimeout : 0.0f;

        if (mbInCombat)
        {
            GameManager.Instance.Input.SprintOff();
        }
    }

    // 플레이어 캐릭터의 공격속도 설정 및 애니메이션에 반영
    public void SetAttackSpeed(float speed)
    {
        mAttackSpeedMultiplier = speed;
        PlayerAnimator.SetFloat("AttackSpeed", mAttackSpeedMultiplier);
    }
    
    // 공격 상태 중 캐릭터의 움직임을 설정하는 메서드
    public void Attack()
    {
        SetCombatState(true);
        
        PlayerAnimator.SetBool("Idle", false);
        PlayerAnimator.SetBool("Move", false);
        PlayerAnimator.SetBool("Jump", false);
        PlayerAnimator.SetBool("Fall", false);
        
        if (GameManager.Instance.Input.DashInput && mDashTimeoutDelta < 0.0f)
        {
            SetPlayerState(PlayerState.Dash);
            return;
        }
            
        if (GameManager.Instance.Input.Skill_1Input && mSkill_1_TimeoutDelta < 0.0f)
        {
            SetPlayerState(PlayerState.Skill_1);
            return;
        }
        else if (GameManager.Instance.Input.Skill_2Input)
        {
            SetPlayerState(PlayerState.Skill_2);
            return;
        }
        else if (GameManager.Instance.Input.Skill_3Input && mSkill_3_TimeoutDelta < 0.0f)
        {
            SetPlayerState(PlayerState.Skill_3);
            return;
        }
        else if (GameManager.Instance.Input.Skill_4Input && mSkill_4_TimeoutDelta < 0.0f)
        {
            SetPlayerState(PlayerState.Skill_4);
            return;
        }
        
        if (bIsGrounded)
        {
            if (GameManager.Instance.Input.RollInput && mRollTimeoutDelta < 0.0f)
            {
                SetPlayerState(PlayerState.Roll);
                return;
            }
            
            // 공격 중 점프 상태
            if (GameManager.Instance.Input.JumpInput && mJumpTimeoutDelta < 0.0f)
            {
                Jump();
                
                PlayerAnimator.SetBool("Jump", true);
            }
            
            if (GameManager.Instance.Input.MoveInput == Vector2.zero)
            {
                Idle();
            
                // 제자리 공격 상태
                PlayerAnimator.SetBool("Idle", true);
            }
            else
            {
                BattleMove();
                
                // 공격 중 이동 상태
                PlayerAnimator.SetBool("Move", true);
            }
        }
        else
        {
            // 공격 중 낙하 상태
            PlayerAnimator.SetBool("Fall", true);
        }
    }
    
    // 공격 애니메이션 중 공격 행동 시작 시점에 호출되는 메서드
    public void MeleeAttackStart()
    {
        mWeaponController.AttackStart();
        mPlayerStateAttack.HasReceivedNextAttackInput = false;
        mPlayerStateAttack.bIsComboActive = true;

        if (CurrentPlayerState == PlayerState.Attack)
        {
            mPlayerStateAttack.AttackEffect();
        }
    }

    // 공격 애니메이션 중 공격 행동 종료 시점에 호출되는 메서드
    public void MeleeAttackEnd()
    {
        mWeaponController.AttackEnd();
    }

    #region 옵저버 패턴 관련 기능

        public void OnNext(GameObject value)
        {
            var enemyController = value.GetComponent<EnemyBTController>();
            if (enemyController)
            {
                //공격력을 PlayerStats에서 가져와 데미지 계산
                float damage = mPlayerStats.GetAttackDamage();
                enemyController.SetHit((int)damage, -1);
                //피해적용 후 흡혈효과 처리
                mPlayerStats.OnDamageDealt(damage);
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
    
    #region 일반 공격 이펙트 관련 기능

        private Coroutine mSlashCoroutine = null;
        public GameObject SlashEffect(SlashEffectType type)
        {
            var slashEffect = GameManager.Instance.Resource.Instantiate("Slash_Effect");
            GameObject slashEffectObject = slashEffect.gameObject;
            
            if (slashEffect != null)
            {
                Vector3 firePosition = transform.position + transform.forward + Vector3.up;
                Quaternion rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                
                switch (type)
                {
                    case SlashEffectType.RightToLeft:
                        rotation = transform.rotation;
                        break;
                    case SlashEffectType.LeftToRight:
                        rotation = transform.rotation * Quaternion.Euler(0.0f, 0.0f, 180.0f);
                        break;
                    case SlashEffectType.TopToBottom:
                        rotation = transform.rotation * Quaternion.Euler(0.0f, 0.0f, 90.0f);
                        break;
                }
                
                slashEffectObject.transform.position = firePosition;
                slashEffectObject.transform.rotation = rotation;
                
                mSlashCoroutine = StartCoroutine(DisableEffectAfterDelay(slashEffectObject, 0.2f));
            }
            
            return slashEffectObject;
        }

        private IEnumerator DisableEffectAfterDelay(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            GameManager.Instance.Resource.Destroy(obj); // 풀링 객체면 풀로, 아니면 파괴
        }

        public void StopSlashCoroutine()
        {
            StopCoroutine(mSlashCoroutine);
        }
    
    #endregion
    
    #endregion

    #region 방어 관련 기능

    public void Defend()
    {
        SetCombatState(true);
        
        if (!GameManager.Instance.Input.IsDefending)
        {
            Defending(false);

            if (GameManager.Instance.Input.MoveInput == Vector2.zero)
            {
                SetPlayerState(PlayerState.Idle);
            }
            else
            {
                SetPlayerState(PlayerState.Move);
            }
        }
        else
        {
            Defending(true);
            
            // 이동 방어시 하반신(Base Layer) 애니메이션
            if (GameManager.Instance.Input.MoveInput == Vector2.zero)
            {
                Idle();
            }
            else
            {
                BattleMove();
            }
        }
    }
    
    private void Defending(bool isDefending)
    {
        if (isDefending)
        {
            //피해감소
            mbIsDamageReduced = true;
            OverrideDamageReduction = 0.9f;
        }
        else
        {
            mbIsDamageReduced = false;
            OverrideDamageReduction = 0.0f;
            //피해감소 끝
        }
    }

    #endregion

    #region 패리 관련 기능
            
    private Coroutine mParryCoroutine;
    private Coroutine mParrySuccessInvincibleCoroutine;
    private bool mbIsParryActive = false;
    private bool mbIsParrySuccess = false;

    public void StartParry()
    {
        if (mParryCoroutine == null)
        {
            mParryCoroutine = StartCoroutine(ParryCoroutine());
        }
    }

    public void StopParry()
    {
        if (mParryCoroutine != null)
        {
            StopCoroutine(mParryCoroutine);
            mParryCoroutine = null;
        }
        
        mbIsParryActive = false;
        mbIsDamageReduced = false;
        ParryCooldownTime();
    }
    
    public void Parry()
    {
        SetCombatState(true);
        
        PlayerAnimator.SetBool("Idle", false);
        PlayerAnimator.SetBool("Move", false);

        // 이동 방어시 하반신(Base Layer) 애니메이션
        if (GameManager.Instance.Input.MoveInput == Vector2.zero)
        {
            Idle();
            PlayerAnimator.SetBool("Idle", true);
        }
        else
        {
            BattleMove();
            PlayerAnimator.SetBool("Move", true);
        }
    }

    private IEnumerator ParryCoroutine()
    {
        mbIsParryActive = true;
        mbIsParrySuccess = false;
        
        yield return new WaitForSeconds(mParryFunctionDuration);
        
        mbIsParryActive = false;
    }

    private IEnumerator ParrySuccessInvincibleCoroutine()
    {
        // 무적 시작
        mbIsDamageReduced = true;
        OverrideDamageReduction = 1.0f;
        mbIsParrySuccess = true;
        
        yield return new WaitForSeconds(mParrySuccessFunctionDuration);
        
        //무적 끝
        mbIsDamageReduced = false;
        OverrideDamageReduction = 0.0f;
    }

    private void ParryCooldownTime()
    {
        if (mbIsParrySuccess)
        {
            // 패리 성공 시 재사용대기시간 초기화
            mParryTimeoutDelta = 0.0f;
        }
        else
        {
            // 패리 실패 시 재사용대기시간 적용
            mParryTimeoutDelta = mParryTimeout;
        }
    }
    
    #endregion
    
    #region 피격/사망 관련 기능

    /// <summary>
    /// 플레이어 캐릭터에 피격 적용하는 메서드
    /// </summary>
    /// <param name="enemyAttackPower"> 적의 공격력 </param>
    /// <param name="enemyTransform"> 적의 위치(현재 사용안해서 없어도 됨) </param>
    /// <param name="hitPower"> 피격 단계 0 = 살짝 휘청, 1 = 크게 휘청 </param>
    public void SetHit(int enemyAttackPower, Transform enemyTransform = null, int hitPower = 0)
    {
        // 플레이어가 죽은 상태일 때는 공격을 받지 않음
        if (CurrentPlayerState == PlayerState.Dead)
        {
            return; // 사망 상태에서는 더 이상 피해를 받지 않음
        }

        // 패리 성공 시
        if (mbIsParryActive)
        {
            mbIsParryActive = false;

            // 적에게 패리 데미지 적용
            if (enemyTransform != null)
            {
                EnemyBTController enemy = enemyTransform.GetComponent<EnemyBTController>();
                if (enemy != null)
                {
                    enemy.SetHit((int)(mPlayerStats.GetAttackDamage() * mParryDamageMultiplier),0);
                }
                else
                {
                    // 공격 오브젝트가 발사체라면
                    Projectile enemyProjectile = enemyTransform.GetComponent<Projectile>();
                    if (enemyProjectile != null)
                    {
                        // 화살 프리팹 인스턴스 생성 (반사용)
                        GameObject reflectedObject = Instantiate(enemyProjectile.gameObject, transform.position + transform.forward + Vector3.up, Quaternion.identity);
                       
                        Projectile reflectedProjectile = reflectedObject.GetComponent<Projectile>();
                        if (reflectedProjectile != null)
                        {
                            // 방향: 적 방향으로
                            Vector3 reflectDir = (enemyProjectile.ShooterTransform.transform.position - transform.position).normalized;

                            // 반사 레이어: 적만 맞도록 설정
                            LayerMask enemyLayer = LayerMask.GetMask("Enemy"); // 적 레이어 이름에 따라 수정

                            // 반사 화살 발사
                            reflectedProjectile.Initialize(transform.forward, 15.0f, enemyLayer, (int)(mPlayerStats.GetAttackDamage() * mParryDamageMultiplier));
                            reflectedProjectile.transform.rotation = Quaternion.LookRotation(reflectDir);
                        }
                    }
                }
            }

            // 잠시 무적효과
            if (mParrySuccessInvincibleCoroutine != null)
            {
                StopCoroutine(mParrySuccessInvincibleCoroutine);
            }
            mParrySuccessInvincibleCoroutine = StartCoroutine(ParrySuccessInvincibleCoroutine());
            
            return;
        }

        if (CurrentPlayerState == PlayerState.Defend && GameManager.Instance.Input.IsDefending)
        {
            mPlayerStats.OnGuardSuccess();
        }
        
        //PlayerState의 TakeDamage 메서드 사용
        mPlayerStats.TakeDamage(enemyAttackPower, OverrideDamageReduction);
        
        // 체력 UI 업데이트
        // GameManager.Instance.SetHP((float)mPlayerStats.GetCurrentHP() / mPlayerStats.GetMaxHP());
        
        if (mPlayerStats.GetCurrentHP() <= 0)
        {
            SetPlayerState(PlayerState.Dead);
        }
        else
        {
            SetCombatState(true);
            
            int upperBodyLayer = PlayerAnimator.GetLayerIndex("UpperBody Layer");
            PlayerAnimator.SetLayerWeight(upperBodyLayer, 1.0f);
            
            if (CurrentPlayerState == PlayerState.Defend)
            {
                PlayerAnimator.SetTrigger("DefendHit");
            }
            // 공격 관련 동작들이 끊기지 않도록
            else if(CurrentPlayerState == PlayerState.Idle || CurrentPlayerState == PlayerState.Move)
            {
                PlayerAnimator.SetFloat("HitPower", hitPower);
                PlayerAnimator.SetTrigger("Hit");
                
                // 방향에 따라 맞는 애니메이션이 없으므로 현재는 효과가 없는 것이나 마찬가지인 상태, 일단 대기
                // 플레이어 캐릭터의 방향을 회전시켜주는 수동적인 방식을 사용하거나?
                if (enemyTransform != null)
                {
                    Vector3 direction = enemyTransform.transform.position - transform.position;
                    
                    PlayerAnimator.SetFloat("HitPosX", -direction.x);
                    PlayerAnimator.SetFloat("HitPosY", -direction.z);
                }
            }
        }
    }

    // 플레이어의 상태이상 효과를 부여하는 메서드
    public void SetStatusEffect(StatusEffect statusEffectType, float duration)
    {
        switch (statusEffectType)
        {
            // 상태이상 없음 or 해제된 상태
            case StatusEffect.None:
                break;
            case StatusEffect.Stun:
                if (CurrentPlayerState != PlayerState.Stun && CurrentPlayerState != PlayerState.Dead)
                {
                    SetPlayerState(PlayerState.Stun);
                    StartCoroutine(StatusEffectDuration(
                        onStart: () => { },
                        duration,
                        onEnd: () => { SetPlayerState(PlayerState.Idle); }));
                }
                break;
            case StatusEffect.Freeze:
                if (CurrentPlayerState != PlayerState.Freeze && CurrentPlayerState != PlayerState.Dead)
                {
                    SetPlayerState(PlayerState.Freeze);
                    StartCoroutine(StatusEffectDuration(
                        onStart: () => { PlayerAnimator.speed = 0.0f; },
                        duration,
                        onEnd: () => { PlayerAnimator.speed = 1.0f; SetPlayerState(PlayerState.Idle); }));
                }
                break;
            case StatusEffect.Burn: // 캐릭터 움직임에 제한을 주지 않는 상태이상이므로 별도의 상태머신은 없음
                /*StartCoroutine(StatusEffectDuration(
                        onStart: () => { /* 화상 이펙트, 토트 데지미 적용 코루틴 #1# },
                        duration,
                        onEnd: () => { /* 화상 이펙트 종료, 토트 데지미 코루틴 종료 #1# }));*/
                break;
            case StatusEffect.Poison:
                // 화상 상태이상과 동일
                break;
            case StatusEffect.Bleed:
                // 화상 상태이상과 동일
                break;
        }
    }

    private IEnumerator StatusEffectDuration(Action onStart, float duration, Action onEnd)
    {
        onStart.Invoke();
        
        yield return new WaitForSeconds(duration);
        
        onEnd?.Invoke();
    }

    private float mOverrideDamageReduction = 0.0f;
    private float OverrideDamageReduction
    {
        get
        {
            if (mbIsDamageReduced)
            {
                return mOverrideDamageReduction;
            }
            else
            {
                return 0.0f;
            }
        } 
        set { mOverrideDamageReduction = value; }
    }

    public void OnEnemyKilled()
    {
        mPlayerStats.OnEnemyKilled();
    }
    
    #endregion

    #region 스킬 관련 기능

    public bool CheckSkillReset()
    {
        return mPlayerStats.OnSkillUse();
    }
    
    #region 1번 스킬

    private Coroutine mSkill1Coroutine;
    public void Start_Skill_1()
    {
        mSkill1Coroutine = StartCoroutine(Skill_1());
    }

    public void Stop_Skill_1()
    {
        if (mSkill1Coroutine != null)
        {
            StopCoroutine(mSkill1Coroutine);
            mSkill1Coroutine = null;
        }
    }

    private IEnumerator Skill_1()
    {
        float attackSpeed = PlayerAnimator.GetFloat("AttackSpeed");
        float invAttackSpeed = 1.0f / attackSpeed;
        float startupTime = 0.15f * invAttackSpeed;
        float recoveryTime = 1.0f * invAttackSpeed;
        
        yield return new WaitForSeconds(startupTime); // 애니메이션 선딜

        SlashEffect(SlashEffectType.LeftToRight);
        
        var skill_1_Effect_Prefab = GameManager.Instance.Resource.Instantiate("Skill_1_Projectile");
        if (skill_1_Effect_Prefab == null)
        {
            yield break;
        }
        
        Vector3 firePosition = transform.position + transform.forward * 2.0f + Vector3.up;
        Vector3 direction = GetActionDirection(false, false);
        
        skill_1_Effect_Prefab.transform.position = firePosition;
        skill_1_Effect_Prefab.transform.rotation = Quaternion.LookRotation(direction);
        
        // 검기 스크립트에 방향 및 속도 설정
        Skill_1 skill_1 = skill_1_Effect_Prefab.GetComponent<Skill_1>();
        skill_1.Init((int)mPlayerStats.GetAttackDamage(), mSkill_1_DamageMultiplier, mSkill_1_Distance, direction);

        yield return new WaitForSeconds(recoveryTime);
        
        mbInDirection = true;
        
        SetCombatState(true);
        
        // 스킬 쿨타임 초기화 확인
        if (!CheckSkillReset())
        {
            // 초기화 실패 - 일반적인 쿨타임 적용
            mSkill_1_TimeoutDelta = mSkill_1_Timeout;
        }
        else
        {
            // 초기화 성공 - 쿨타임 즉시 완료
            mSkill_1_TimeoutDelta = 0.0f;
            Debug.Log("스킬 1 쿨타임 초기화!");
        }
    }

    #endregion

    #region 2번 스킬

    private Coroutine mSkill2CCoroutine;
    public void Start_Skill_2()
    {
        mSkill2CCoroutine = StartCoroutine(Skill_2());
    }

    public void Stop_Skill_2()
    {
        if (mSkill2CCoroutine != null)
        {
            StopCoroutine(mSkill2CCoroutine);
            mSkill2CCoroutine = null;
        }
    }
    
    private IEnumerator Skill_2()
    {
        float attackSpeed = PlayerAnimator.GetFloat("AttackSpeed");
        float invAttackSpeed = 1.0f / attackSpeed;
        float startupTime = 0.2f * invAttackSpeed;
        float recoveryTime = 1.0f * invAttackSpeed;
        
        yield return new WaitForSeconds(startupTime); // 애니메이션 선딜
        
        // 주변 적에게 데미지 주는 로직
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, mSkill_2_Radius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                EnemyBTController enemy = hitCollider.GetComponent<EnemyBTController>();
                if (enemy != null)
                {
                    // 공격력 감소 부여
                    enemy.SetHit((int)(mPlayerStats.GetAttackDamage() * mSkill_2_DamageMultiplier),1);
                }
            }
        }
        
        GameObject skill_2_Effect_Prefab = GameManager.Instance.Resource.Instantiate("Skill_2_Effect");
        if (skill_2_Effect_Prefab == null)
        {
            yield break;
        }
        skill_2_Effect_Prefab.transform.position = transform.position;
        skill_2_Effect_Prefab.transform.rotation = Quaternion.identity;
        
        yield return new WaitForSeconds(recoveryTime);
        GameManager.Instance.Resource.Destroy(skill_2_Effect_Prefab);
        
        SetCombatState(true);
        
        // 스킬 쿨타임 초기화 확인
        if (!CheckSkillReset())
        {
            // 초기화 실패 - 일반적인 쿨타임 적용
            mSkill_2_TimeoutDelta = mSkill_2_Timeout;
        }
        else
        {
            // 초기화 성공 - 쿨타임 즉시 완료
            mSkill_2_TimeoutDelta = 0.0f;
            Debug.Log("스킬 2 쿨타임 초기화!");
        }
    }

    #endregion

    #region 3번 스킬

    private Coroutine mFallToGroundCoroutine;
    private Coroutine mSkill3Coroutine;
    private bool mbIsLandSucess = false;

    public void Start_Skill_3()
    {
        mSkill3Coroutine = StartCoroutine(Skill_3());
    }

    public void Stop_Skill_3()
    {
        if (mFallToGroundCoroutine != null)
        {
            StopCoroutine(mFallToGroundCoroutine);
            mFallToGroundCoroutine = null;
        }

        if (mSkill3Coroutine != null)
        {
            StopCoroutine(mSkill3Coroutine);
            mSkill3Coroutine = null;
        }
    }
    
    private IEnumerator Skill_3()
    {
        mbIsLandSucess = false;
        PlayerAnimator.SetTrigger("Skill");
        PlayerAnimator.SetInteger("Skill_Index", 3);

        if (bIsGrounded)
        {
            mbIsLandSucess = true;
            yield return new WaitForSeconds(0.3f);
        }
        else
        {
            mFallToGroundCoroutine = StartCoroutine(Skill_3_Stance());
            yield return mFallToGroundCoroutine;
        }

        yield return null;

        if (mbIsLandSucess)
        {
            mSkill3Coroutine = StartCoroutine(Skill_3_Fire());
            yield return mSkill3Coroutine;
        }
    }

    private IEnumerator Skill_3_Stance()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 2.0f, Vector3.down, out RaycastHit hit, 100f, mGroundLayers))
        {
            mbIsLandSucess = true;
            
            float duration = 0.3f; // 떨어지는 시간
            float timer = 0.0f;
            Vector3 startPosition = transform.position;

            while (timer < duration)
            {
                transform.position = Vector3.Lerp(startPosition, hit.point, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }

            transform.position = hit.point;
        }
        else
        {
            // 레이 실패 시 그냥 낙하
            mbIsLandSucess = false;
            SetPlayerState(PlayerState.Fall);
        }
    }
    
    private IEnumerator Skill_3_Fire()
    {
        float attackSpeed = PlayerAnimator.GetFloat("AttackSpeed");
        float invAttackSpeed = 1.0f / attackSpeed;
        float startupTime = 0.0f * invAttackSpeed;
        float recoveryTime = 0.5f * invAttackSpeed;
        
        yield return new WaitForSeconds(startupTime); // 애니메이션 선딜
        
        GameObject skill_3_Effect_Prefab = GameManager.Instance.Resource.Instantiate("Skill_3_Effect");
        if (skill_3_Effect_Prefab == null)
        {
            yield break;
        }
        skill_3_Effect_Prefab.transform.position = transform.position;
        skill_3_Effect_Prefab.transform.rotation = Quaternion.identity;
        
        // 주변 적에게 데미지 주는 로직
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, mSkill_3_Radius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                EnemyBTController enemy = hitCollider.GetComponent<EnemyBTController>();
                if (enemy != null)
                {
                    enemy.SetHit((int)(mPlayerStats.GetAttackDamage() * mSkill_3_DamageMultiplier),1);
                }
            }
        }
        
        yield return new WaitForSeconds(recoveryTime);
        GameManager.Instance.Resource.Destroy(skill_3_Effect_Prefab);
        
        SetCombatState(true);
        
        // 스킬 쿨타임 초기화 확인
        if (!CheckSkillReset())
        {
            // 초기화 실패 - 일반적인 쿨타임 적용
            mSkill_3_TimeoutDelta = mSkill_3_Timeout;
        }
        else
        {
            // 초기화 성공 - 쿨타임 즉시 완료
            mSkill_3_TimeoutDelta = 0f;
            Debug.Log("스킬 3 쿨타임 초기화!");
        }    
    }
    
    #endregion

    #region 4번 스킬

    private Coroutine mSkill4Coroutine;
    private Coroutine mCameraCoroutine;
    private Coroutine mAimAndFireCoroutine;
    private Coroutine mProjectileCoroutine;
    
    private float originCameraDistance = 0.0f;
    private bool mbIsCameraResetting = false;
    
    public void Start_Skill_4()
    {
        StartCoroutine(Skill_4());
    }

    public void Stop_Skill_4()
    {
        if (mSkill4Coroutine != null)
        {
            StopCoroutine(mSkill4Coroutine);
            mSkill4Coroutine = null;
        }

        if (mCameraCoroutine != null)
        {
            StopCoroutine(mCameraCoroutine);
            mCameraCoroutine = null;
        }

        if (mAimAndFireCoroutine != null)
        {
            StopCoroutine(mAimAndFireCoroutine);
            mAimAndFireCoroutine = null;
        }

        if (mProjectileCoroutine != null)
        {
            StopCoroutine(mProjectileCoroutine);
            mProjectileCoroutine = null;
        }
    }
    
    private IEnumerator Skill_4()
    {
        mSkill4Coroutine = StartCoroutine(Skill_4_Stance());
        yield return mSkill4Coroutine;

        mCameraCoroutine = StartCoroutine(Skill_4_Camera(true));
        mAimAndFireCoroutine = StartCoroutine(Skill_4_AimAndFire());
        
        yield return mCameraCoroutine;
        yield return mAimAndFireCoroutine;
    }
    
    private IEnumerator Skill_4_Stance()
    {
        Vector3 groundPosition = transform.position;
        if (Physics.Raycast(transform.position + Vector3.up * 2.0f, Vector3.down, out RaycastHit hit, 100.0f, mGroundLayers))
        {
            groundPosition = hit.point;
        }
        
        float jumpSpeed = 25.0f;
        float jumpHeight = 10.0f;
        float targetHeight = groundPosition.y + jumpHeight;
        
        PlayerAnimator.SetBool("Jump", true);
        
        while (transform.position.y < targetHeight)
        {
            float jumpAmount = jumpSpeed * Time.deltaTime;

            if (transform.position.y + jumpAmount > targetHeight)
            {
                jumpAmount = targetHeight - transform.position.y;
            }
            
            
            mCharacterController.Move(Vector3.up * jumpAmount);
            yield return null;
        }
        
        PlayerAnimator.SetBool("Jump", false);
    }

    private IEnumerator Skill_4_Camera(bool isStance)
    {
        var camera = mMainCamera.GetComponent<CameraController>();
        var virtualCamera = camera.GetComponent<CinemachineVirtualCamera>();
        var threePersonFollow = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        
        if (isStance)
        {
            // 카메라 복원 중이면 대기
            while (mbIsCameraResetting)
            {
                yield return null;
            }
            
            originCameraDistance = threePersonFollow.CameraDistance;
            mbIsCameraResetting = false;
            
            float timeElapsed = 0.0f;
            float duration = 0.5f;
            float targetDistance = 20.0f;
            
            while (timeElapsed < duration)
            {
                threePersonFollow.CameraDistance = Mathf.Lerp(originCameraDistance, targetDistance, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            threePersonFollow.CameraDistance = targetDistance;
        }
        else
        {
            // 이미 복원 중이면 종료
            if (mbIsCameraResetting)
            {
                yield break;
            }
            
            mbIsCameraResetting = true;
            
            float timeElapsed = 0.0f;
            float duration = 0.5f;
            float startDistance = threePersonFollow.CameraDistance;
            
            while (timeElapsed < duration)
            {
                threePersonFollow.CameraDistance = Mathf.Lerp(startDistance, originCameraDistance, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            threePersonFollow.CameraDistance = originCameraDistance;
            mbIsCameraResetting = false;
        }
    }
    
    private IEnumerator Skill_4_AimAndFire()
    {
        GameObject rangeIndicatorObject = GameManager.Instance.Resource.Instantiate("Skill_4_RangeIndicator");

        float timer = 5.0f;
        bool isAttackCompleted = false;
        Vector3 finalTargetPoint = Vector3.zero;
        
        while (timer > 0.0f)
        {
            timer -= Time.deltaTime;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100.0f, mGroundLayers))
            {
                rangeIndicatorObject.transform.position = hit.point;
                finalTargetPoint = hit.point;
            }

            Vector3 cameraForward = GetCameraForwardDirection(true);
            Quaternion cameraRotation = Quaternion.LookRotation(cameraForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, cameraRotation, 10.0f * Time.deltaTime);

            if (GameManager.Instance.Input.AttackInput)
            {
                FireProjectile(finalTargetPoint);
                isAttackCompleted = true;
                break;
            }

            // 취소 입력 시
            if (GameManager.Instance.Input.DefendInput)
            {
                CancelSkill();
                isAttackCompleted = true;
                break;
            }

            yield return null;
        }
        
        // 자동 발사
        if (!isAttackCompleted)
        {
            FireProjectile(finalTargetPoint);
            yield return null;
        }
        
        GameManager.Instance.Resource.Destroy(rangeIndicatorObject);
        
        SetCombatState(true);
    }
    
    private void FireProjectile(Vector3 targetPoint)
    {
        mbInDirection = true;
        
        mProjectileCoroutine = StartCoroutine(FireProjectileCoroutine(targetPoint));
        
        mSkill_4_TimeoutDelta = mSkill_4_Timeout;
    }

    private IEnumerator FireProjectileCoroutine(Vector3 targetPoint)
    {
        float attackSpeed = PlayerAnimator.GetFloat("AttackSpeed");
        float invAttackSpeed = 1.0f / attackSpeed;
        float startupTime = 0.3f * invAttackSpeed;
        float recoveryTime = 0.1f * invAttackSpeed;
        
        PlayerAnimator.SetTrigger("Skill");
        PlayerAnimator.SetInteger("Skill_Index", 4);
        
        yield return new WaitForSeconds(startupTime); // 애니메이션 선딜

        SlashEffect(SlashEffectType.TopToBottom);

        GameObject projectilePrefab = GameManager.Instance.Resource.Instantiate("Skill_4_Projectile");
        if (projectilePrefab == null)
        {
            yield break;
        }
        
        Vector3 firePosition = targetPoint + Vector3.up * 10.0f;
        Quaternion rotation = Quaternion.LookRotation(- mMainCamera.transform.right);
        
        projectilePrefab.transform.position = firePosition;
        projectilePrefab.transform.rotation = rotation;
        
        Skill_4 skill_4 = projectilePrefab.GetComponent<Skill_4>();
        skill_4.Init((int)mPlayerStats.GetAttackDamage(), mSkill_4_DamageMultiplier, mSkill_4_Radius, targetPoint);
        
        yield return new WaitForSeconds(recoveryTime);
        
        // 스킬 쿨타임 초기화 확인
        if (!CheckSkillReset())
        {
            // 초기화 실패 - 일반적인 쿨타임 적용
            mSkill_4_TimeoutDelta = mSkill_4_Timeout;
        }
        else
        {
            // 초기화 성공 - 쿨타임 즉시 완료
            mSkill_4_TimeoutDelta = 0f;
            Debug.Log("스킬 4 쿨타임 초기화!");
        }
        
        yield return StartCoroutine(Skill_4_Camera(false));
        yield return null;
    }

    private void CancelSkill()
    {
        StartCoroutine(Skill_4_Camera(false));

        mbInDirection = true;
        
        SetPlayerState(PlayerState.Fall);
    }
    
    #endregion
    
    #endregion

    #region 사운드 관련 기능

    // 발소리, 나중에 사운드매니저로 관리해야 함
    private void OnFootstepSound(AnimationEvent animationEvent)
    {
        var mobilityLayer = PlayerAnimator.GetLayerIndex("Mobility Layer");
        var skillLayer = PlayerAnimator.GetLayerIndex("Skill Layer");
        
        if (animationEvent.animatorClipInfo.weight > 0.5f && 
            (PlayerAnimator.GetLayerWeight(mobilityLayer) < 1.0f && PlayerAnimator.GetLayerWeight(skillLayer) < 1.0f))
        {
            if (footstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, footstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(footstepAudioClips[index], transform.position /*, 볼륨 */);
            }
        }
    }

    private void OnLandSound(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(landingAudioClip, transform.position /*, 볼륨 */);
        }
    }

    #endregion
    
    #region 디버깅 관련

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (bIsGrounded)
        {
            Gizmos.color = transparentGreen;
        }
        else
        {
            Gizmos.color = transparentRed;
        }
        
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - mGroundedOffset, transform.position.z), mGroundedRadius);
    }

    #endregion
}