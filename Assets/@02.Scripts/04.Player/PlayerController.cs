using System;
using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour, IObserver<GameObject>
{
    [Header("Reference")]
    [SerializeField] private PlayerStats mPlayerStats;

    [Space(10)]
    [Header("Player Movement Stat")]
    [SerializeField] private float mSpeed;
    [SerializeField] private float mSpeedChangeRate = 10.0f;
    [SerializeField] private float mRotationSmoothTime = 0.12f;
    
    [Space(10)]
    [Header("Player Jump Stat")]
    [SerializeField] private float mGravity = - 9.81f;
    [SerializeField] private float mJumpHeight = 5.0f;
    [SerializeField] private float mJumpTimeout = 0.5f;
    [SerializeField] private float mJumpTimeoutDelta;
    [SerializeField] private float mFallTimeout = 0.15f;
    [SerializeField] private float mFallTimeoutDelta;
    
    [Space(10)]
    [Header("Player Roll Stat")]
    [SerializeField] private float mRollDistance = 8.0f;
    [SerializeField] private float mRollFunctionDuration = 0.3f;
    [SerializeField] private float mRollTimeout = 3.0f;
    [SerializeField] private float mRollTimeoutDelta;
    public float RollTimeoutDelta => mRollTimeoutDelta;
    private Coroutine mRollCoroutine;
    
    [Space(10)]
    [Header("Player Dash Stat")]
    [SerializeField] private float mDashDistance = 10.0f;
    [SerializeField] private float mDashFunctionDuration = 0.3f;
    [SerializeField] private float mDashTimeout = 5.0f;
    [SerializeField] private float mDashTimeoutDelta;
    public float DashTimeoutDelta => mDashTimeoutDelta;
    private Coroutine mDashCoroutine;

    [Space(10)] 
    [Header("Player Attack Stat")]
    [SerializeField] private float mAttackSpeed = 1.0f;
    
    [Space(10)]
    [Header("Player Grouned Check")]
    [SerializeField] private LayerMask mGroundLayers;
    [SerializeField] private float mGroundedOffset = -0.15f;
    [SerializeField] private float mGroundedRadius = 0.3f;
    public bool bIsGrounded { get; private set; }

    [Space(10)]
    [Header("Player Combat Check")]
    [SerializeField] private float mInCombatTimeout = 10.0f;
    [SerializeField] private float mInCombatTimeoutDelta;
    [SerializeField] private bool mbInCombat = false;
    
    [Space(10)]
    [Header("Player Interactables Check")]
    [SerializeField] private LayerMask mInteractableLayers;
    [SerializeField] private float mInteractableRadius = 3.0f;
    [SerializeField] private Collider[] mDetectedInteractables = new Collider[5];
    [SerializeField] private List<Collider> mActiveInteractables = new List<Collider>();
    public InteractableObject NearestInteractableObject { get; private set; }
    
    [Space(10)]
    [Header("Player Attach Point")]
    [SerializeField] private Transform mRightHandTransform;
    [SerializeField] private Transform mLeftHandTransform;
    
    // Player Internal Calculation Stat
    [SerializeField]
    private float mVerticalVelocity;
    private float mRotationVelocity;
    private float mTerminalVelocity = 53.0f;
    private float mTargetRotation;
    private float mAnimationBlend;
    
    // Componenet
    public Animator PlayerAnimator { get; private set; }
    private CharacterController mCharacterController;
    private PlayerInput mPlayerInput;
    private GameObject mMainCamera;
    private WeaponController mWeaponController;
    
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
    private PlayerStateHit mPlayerStateHit;
    private PlayerStateDefendHit mPlayerStateDefend_Hit;
    private PlayerStateStun mPlayerStateStun;
    private PlayerStateFreeze mPlayerStateFreeze;
    private PlayerStateDead mPlayerStateDead;
    public PlayerState CurrentPlayerState { get; private set; }
    private Dictionary<PlayerState, IPlayerState> mPlayerStates;
    
    private void Awake()
    {
        PlayerAnimator = GetComponent<Animator>();
        mCharacterController = GetComponent<CharacterController>();
        mPlayerInput = GetComponent<PlayerInput>();
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
        NearbyInteractablesCheck();
        TimeoutCheck();
    }

    private void FixedUpdate()
    {
        CalculateGravity();
    }

    /// <summary>
    /// 플레이어 캐릭터 초기화 메서드
    /// </summary>
    public void Init()
    {
        StateInit();
        TimeoutInit();
        
        GameManager.Instance.Input.Init(mPlayerInput);
        
        // 공격 속도 설정
        PlayerAnimator.SetFloat("AttackSpeed", mAttackSpeed);
        
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
        mPlayerStateHit = new PlayerStateHit();
        mPlayerStateDefend_Hit = new PlayerStateDefendHit();
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
            { PlayerState.Hit, mPlayerStateHit },
            { PlayerState.Defend_Hit, mPlayerStateDefend_Hit },
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
        bIsGrounded = Physics.CheckSphere(spherePosition, mGroundedRadius, mGroundLayers, QueryTriggerInteraction.Ignore);

        PlayerAnimator.SetBool("IsGrounded", bIsGrounded);

        mMainCamera.GetComponent<CameraController>().SendPlayerGrounded(bIsGrounded);
    }

    /// <summary>
    /// 인접한 상호작용 개체 중 가장 근접한 상호작용 개체의 <InteractableObject>에 접근
    /// </summary>
    private void NearbyInteractablesCheck()
    {
        // 감지된 Interactable 콜라이더의 수
        int InteractableCount = Physics.OverlapSphereNonAlloc(transform.position, mInteractableRadius, mDetectedInteractables, mInteractableLayers, QueryTriggerInteraction.Ignore);
        
        // 현재 감지된 콜라이더들을 해시셋으로 변환
        HashSet<Collider> currentDetected  = new HashSet<Collider>();
        for (int i = 0; i < InteractableCount; i++)
        {
            currentDetected.Add(mDetectedInteractables[i]);
        }

        // 이전 리스트 중, 더 이상 감지되지 않는 것은 제거
        mActiveInteractables.RemoveAll(InteractableCollider => !currentDetected.Contains(InteractableCollider));
        
        // 현재 감지된 것 중, 새로 들어온 것은 추가
        foreach (Collider detectCollider in currentDetected)
        {
            if (!mActiveInteractables.Contains(detectCollider))
            {
                mActiveInteractables.Add(detectCollider);
            }
        }

        // 가장 가까운 상호작용 오브젝트 계산
        if (mActiveInteractables.Count > 0)
        {
            float shortestDistance = float.MaxValue;
            Collider nearest = null;

            foreach (var currentInteractable in mActiveInteractables)
            {
                float distance = Vector3.Distance(transform.position, currentInteractable.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearest = currentInteractable;
                }
            }
            
            // 상호작용 가능한 오브젝트 캐싱
            NearestInteractableObject = nearest?.GetComponent<InteractableObject>();
        }
        else
        {
            NearestInteractableObject = null;
        }
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
                else if (CurrentPlayerState == PlayerState.Dash)
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
    /// 구르기 기능의 방향을 설정해주는 메서드
    /// </summary>
    /// <returns> 이동 입력이 없으면 카메라 전방, 이동 입력이 있으면 입력 방향 </returns>
    public Vector3 SetRollDirection()
    {
        Vector3 targetDirection = Vector3.zero;
        Vector2 moveInput = GameManager.Instance.Input.MoveInput;
        
        if (moveInput == Vector2.zero)
        {
            targetDirection = GetCameraForwardDirection(true);
        }
        else
        {
            Vector3 inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;
            float targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mMainCamera.transform.eulerAngles.y;

            targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
        }
        
        return targetDirection;
    }

    /// <summary>
    /// 돌진 기능의 방향을 설정해주는 메서드
    /// </summary>
    /// <returns> 지상에서는 카메라 수평 전방, 공중에서는 카메라 중앙전방(수직+수평) </returns>
    public Vector3 SetDashDirection()
    {
        var targetDirection = Vector3.zero;
        
        if (bIsGrounded)
        {
            targetDirection = GetCameraForwardDirection(true);
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
        mSpeed = HandleSpeed(targetSpeed);
        
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
            mCharacterController.Move(targetDirection.normalized * (mSpeed * Time.deltaTime) + new Vector3(0.0f, mGravity, 0.0f) * Time.deltaTime);
        }
        else
        {
            mCharacterController.Move(targetDirection.normalized * (mSpeed * Time.deltaTime) + new Vector3(0.0f, mVerticalVelocity, 0.0f) * Time.deltaTime);
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
        float smoothedSpeed = Mathf.Lerp(currentSpeed, mSpeed, Time.deltaTime * mSpeedChangeRate);
        if (smoothedSpeed < 0.01f)
        {
            smoothedSpeed = 0f;
        }
        PlayerAnimator.SetFloat("Speed", smoothedSpeed);
    }
    
    #endregion
    
    public void Idle()
    {
        mSpeed = 0.0f;
        
        if (CurrentPlayerState == PlayerState.Idle)
        {
            mCharacterController.Move(new Vector3(0.0f, mGravity, 0.0f) * Time.deltaTime);
        }
        else
        {
            mCharacterController.Move(new Vector3(0.0f, mVerticalVelocity, 0.0f) * Time.deltaTime);
        }
        
        PlayerAnimator.SetFloat("Speed", mSpeed);
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
        if (mJumpTimeoutDelta < 0.0f)
        {
            mVerticalVelocity = 0.0f;
            mVerticalVelocity = Mathf.Sqrt(mJumpHeight * -2.0f * mGravity);

            Vector3 targetDirection = Quaternion.Euler(0.0f, mTargetRotation, 0.0f) * Vector3.forward;
            mCharacterController.Move(targetDirection.normalized * (mSpeed * Time.deltaTime) 
                                      + new Vector3(0.0f, mVerticalVelocity, 0.0f) * Time.deltaTime);
        }
    }
    
    public void Fall()
    {
        Vector3 targetDirection = Quaternion.Euler(0.0f, mTargetRotation, 0.0f) * Vector3.forward;
        mCharacterController.Move(targetDirection.normalized * (mSpeed * Time.deltaTime) 
                                  + new Vector3(0.0f, mVerticalVelocity, 0.0f) * Time.deltaTime);
    }
    
    #endregion

    #region 구르기 관련 기능
    
    // todo: 현재 재사용대기시간 적용 -> 스테미너 소모 형식으로 바꿀지 고민 중 / 재사용대기시간을 적용 한다면 돌진기와 공통기능 통합 가능
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
    }

    private IEnumerator RollCoroutine(Vector3 targetDirection)
    {
        float rollEndOffset = 0.5f;
        float firstDelay = 0.2f;
        
        mRollTimeoutDelta = mRollTimeout;
        mPlayerStateRoll.bIsRoll = true;

        yield return new WaitForSeconds(firstDelay); // 선딜(현재 애니메이션에 맞춤)
        RollFunction(true);
     
        StartCoroutine(RollingCoroutine(targetDirection));
        
        yield return new WaitForSeconds(mRollFunctionDuration); // 무적시간?
        RollFunction(false);

        yield return null;
        var rollAnimationInfo = PlayerAnimator.GetCurrentAnimatorStateInfo(0);
        if (rollAnimationInfo.IsName("Roll"))
        {
            float rollAnimationLength = rollAnimationInfo.length;
            float rollEndTime = Mathf.Max(0.0f, rollAnimationLength - rollEndOffset - firstDelay - mRollFunctionDuration);
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

    // 무적?
    private void RollFunction(bool isRollFunction)
    {
        if (isRollFunction)
        {
            
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
            SetCombatState(true);
        }
    }

    public void StopDash()
    {
        if (mDashCoroutine != null)
        {
            StopCoroutine(mDashCoroutine);
            mDashCoroutine = null;
        }
    }
    
    private IEnumerator DashCoroutine(Vector3 cameraCenterDirection)
    {
        float dashEndOffset = 0.5f;
        float firstDelay = 0.0f;
        
        mDashTimeoutDelta = mDashTimeout;
        mPlayerStateDash.bIsDashing = true;
        
        // 애니메이션 초기 구간 기다림
        yield return new WaitForSeconds(firstDelay); // 선딜
        DashFunction(true);
        
        StartCoroutine(DashingCoroutine(cameraCenterDirection));
        
        yield return new WaitForSeconds(mDashFunctionDuration);
        DashFunction(false);
        
        yield return null;
        var dashAnimationInfo = PlayerAnimator.GetCurrentAnimatorStateInfo(0);
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
        }
    }

    // 충돌 무시 및 피해감소? 무적?
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
    }

    // 플레이어 캐릭터의 공격속도 설정 및 애니메이션에 반영
    public void SetAttackSpeed(float speed)
    {
        mAttackSpeed = speed;
        PlayerAnimator.SetFloat("AttackSpeed", mAttackSpeed);
    }
    
    // 공격 상태 중 캐릭터의 움직임을 설정하는 메서드
    public void Attack()
    {
        GameManager.Instance.Input.SprintOff();
        SetCombatState(true);
        
        PlayerAnimator.SetBool("Idle", false);
        PlayerAnimator.SetBool("Move", false);
        PlayerAnimator.SetBool("Jump", false);
        PlayerAnimator.SetBool("Fall", false);
        
        if (bIsGrounded)
        {
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
            
            // 공격 중 점프 상태
            if (GameManager.Instance.Input.JumpInput)
            {
                Jump();
                
                PlayerAnimator.SetBool("Jump", true);
            }
        }
        else
        {
            // 공격 중 낙하 상태
            PlayerAnimator.SetBool("Fall", true);
        }
    }
    
    // 공격 애니메이션의 공격 모션 시작 시 호출 메서드
    public void MeleeAttackStart()
    {
        mWeaponController.AttackStart();
    }

    // 공격 애니메이션의 공격 모션 종료 시 호출되는 메서드
    public void MeleeAttackEnd()
    {
        mWeaponController.AttackEnd();
    }

    public void EndCombo()
    {
        mPlayerStateAttack.bIsComboActive = false;
    }

    #region 옵저버 패턴 관련 기능

        public void OnNext(GameObject value)
        {
            var enemyController = value.GetComponent<EnemyBTController>();
            if (enemyController)
            {
                //공격력을 PlayerStats에서 가져와 데미지 계산
                float damage = mPlayerStats.GetAttackDamage();
                enemyController.SetHit((int)damage);
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
    
    #endregion

    #region 방어 관련 기능

    public void Defend()
    {
        GameManager.Instance.Input.SprintOff();
        
        if (!GameManager.Instance.Input.IsDefending)
        {
            Defending(false);
            
            PlayerAnimator.SetBool("Idle", false);
            PlayerAnimator.SetBool("Move", false);
            PlayerAnimator.SetBool("Defend", false);
            SetPlayerState(PlayerState.Idle);
        }
        else
        {
            // 피해감소 or 방어력 증가 기능 메서드
            Defending(true);
            
            // 이동 방어시 하반신(Base Layer) 애니메이션
            if (GameManager.Instance.Input.MoveInput == Vector2.zero)
            {
                Idle();
                PlayerAnimator.SetBool("Idle", true);
                PlayerAnimator.SetBool("Move", false);
            }
            else
            {
                BattleMove();
                PlayerAnimator.SetBool("Idle", false);
                PlayerAnimator.SetBool("Move", true);
            }
        }
    }
    
    private void Defending(bool isDefending)
    {
        if (isDefending)
        {
            // todo: 임시[테스트용]
            mPlayerStats.EnableDefenceBuff(90.0f);
            mPlayerStats.OnGuardSuccess();
        }
        else
        {
            mPlayerStats.EnableDefenceBuff(0.0f);
        }
    }

    #endregion

    #region 패리 관련 기능

    public void Parry()
    {
        GameManager.Instance.Input.SprintOff();
        SetCombatState(true);

        // 패리 성공 시 행동 메서드
        ParrySuccess();
        
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

    private void ParrySuccess()
    {
        
    }
    
    #endregion
    
    #region 피격/사망 관련 기능

    public void SetHit(EnemyBTController enemyController, Vector3 direction)
    {
        // 플레이어가 죽은 상태일 때는 공격을 받지 않음
        if (CurrentPlayerState == PlayerState.Dead)
        {
            return; // 사망 상태에서는 더 이상 피해를 받지 않음
        }
        
        if (CurrentPlayerState != PlayerState.Hit)
        {
            //PlayerState의 TakeDamage 메서드 사용
            var enemyPower = enemyController.AttackBehaviorAsset as MeleeAttackBehavior;
            if (enemyPower != null)
            {
                mPlayerStats.TakeDamage(enemyPower.Damage);
            }
        }
        
        // 체력 UI 업데이트
        // GameManager.Instance.SetHP((float)mPlayerStats.GetCurrentHP() / mPlayerStats.GetMaxHP());
        
        if (mPlayerStats.GetCurrentHP() <= 0)
        {
            SetPlayerState(PlayerState.Dead);
        }
        else
        {
            SetCombatState(true);

            if (CurrentPlayerState == PlayerState.Defend)
            {
                SetPlayerState(PlayerState.Defend_Hit);
            }
            else
            {
                SetPlayerState(PlayerState.Hit);
                
                // 방향에 따라 맞는 애니메이션이 없으므로 현재는 효과가 없는 것이나 마찬가지인 상태, 일단 대기
                // 플레이어 캐릭터의 방향을 회전시켜주는 수동적인 방식을 사용하거나?
                PlayerAnimator.SetFloat("HitPosX", -direction.x);
                PlayerAnimator.SetFloat("HitPosY", -direction.z);
            }
        }
    }

    // 플레이어의 상태이상 효과
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

    public void OnEnemyKilled()
    {
        mPlayerStats.OnEnemyKilled();
    }

    public bool CheckSkillReset()
    {
        return mPlayerStats.OnSkillUse();
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