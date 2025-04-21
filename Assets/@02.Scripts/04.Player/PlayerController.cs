using System;
using System.Collections.Generic;
using Cinemachine;
using PlayerEnums;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour, IObserver<GameObject>
{
    [Header("Player Basic Stat")]
    [SerializeField] private int mMaxHealth = 100;
    [SerializeField] private int mAttackPower = 10;
    public int AttackPower => mAttackPower;
    [SerializeField] private int mDefendPower = 5;
    
    [Space(10)]
    [Header("Player Move Stat")]
    [SerializeField] private float mMoveSpeed = 2.0f;
    [SerializeField] private float mSprintSpeed = 6.0f;
    [SerializeField] private float mRotationSmoothTime = 0.12f;
    [SerializeField] private float mSpeedChangeRate = 10.0f;
    
    [Space(10)]
    [Header("Player Jump Stat")]
    [SerializeField] private float mGravity = - 9.81f;
    [SerializeField] private float mJumpHeight = 5.0f;
    [SerializeField] private float mJumpTimeout = 0.5f;
    public float JumpTimeout => mJumpTimeout;
    [SerializeField] private float mFallTimeout = 0.15f;
    public float FallTimeout => mFallTimeout;
    
    [Space(10)]
    [Header("Player Grouned Check")]
    [SerializeField] private bool mbIsGrounded = true;
    public bool IsGrounded => mbIsGrounded;
    [SerializeField] private float mGroundedOffset = -0.15f;
    [SerializeField] private float mGroundedRadius = 0.3f;
    [SerializeField] private LayerMask mGroundLayers;
    
    [Space(10)]
    [Header("Player Attach Point")]
    [SerializeField] private Transform mRightHandTransform;
    [SerializeField] private Transform mLeftHandTransform;

    // Player Stat
    private float mCurrentHealth;
    private float mSpeed;
    private float mAnimationBlend;
    private float mTerminalVelocity = 53.0f;
    private float mTargetRotation;
    private float mRotationVelocity;
    private float mVerticalVelocity;
    
    // Timeout Deltatime
    private float mJumpTimeoutDelta;
    private float mFallTimeoutDelta;
    
    // Componenet
    public Animator PlayerAnimator;
    private CharacterController mCharacterController;
    private PlayerInput mPlayerInput;
    private GameObject mMainCamera;
    private WeaponController mWeaponController;
    private const float mThreshold = 0.01f;
    public float Threshold => mThreshold;
    private bool mHasAnimator;
    
    // State
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
    
    private void Awake()
    {
        PlayerAnimator = GetComponent<Animator>();
        mHasAnimator = TryGetComponent(out PlayerAnimator);
        mCharacterController = GetComponent<CharacterController>();
        mPlayerInput = GetComponent<PlayerInput>();
        if (Camera.main != null)
        {
            mMainCamera = Camera.main.gameObject;
        }
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
        mPlayerStateDash = new PlayerStateDash();
        mPlayerStateSkill_1 = new PlayerStateSkill_1();
        mPlayerStateSkill_2 = new PlayerStateSkill_2();
        mPlayerStateSkill_3 = new PlayerStateSkill_3();
        mPlayerStateSkill_4 = new PlayerStateSkill_4();
        mPlayerStateInteraction = new PlayerStateInteraction();

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
        
        Init();
    }

    private void Update()
    {
        if (CurrentPlayerState != PlayerState.None)
        {
            mPlayerStates[CurrentPlayerState].OnUpdate();
        }
        
        GroundedCheck();
        ApplyGravity();
    }

    public void Init()
    {
        GameManager.Instance.Input.Init(mPlayerInput);
        mJumpTimeoutDelta = mJumpTimeout;
        mFallTimeoutDelta = mFallTimeout;
        
        SetPlayerState(PlayerState.Idle);
        
        // 체력 초기화
        mCurrentHealth = mMaxHealth;
        
        // 무기 할당
        SetPlayerWeapon(mRightHandTransform, "Longsword", mLeftHandTransform, "Shield");
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

    #region 물리 계산 관련

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - mGroundedOffset,
            transform.position.z);
        mbIsGrounded = Physics.CheckSphere(spherePosition, mGroundedRadius, mGroundLayers,
            QueryTriggerInteraction.Ignore);

        PlayerAnimator.SetBool("IsGrounded", mbIsGrounded);
    }
    
    private void ApplyGravity()
    {
        if (mbIsGrounded)
        {
            mFallTimeoutDelta = FallTimeout;
             
            if (mJumpTimeoutDelta >= 0.0f)
            {
                 mJumpTimeoutDelta -= Time.deltaTime;
            }
            
            if (mVerticalVelocity < 0.0f)
            {
                mVerticalVelocity = -2.0f;
            }
        }
        else
        {
            mJumpTimeoutDelta = JumpTimeout;
            
            if (mFallTimeoutDelta >= 0.0f)
            {
                mFallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                SetPlayerState(PlayerState.Fall);
            }
        }
        
        if (mVerticalVelocity < mTerminalVelocity)
        {
            mVerticalVelocity += mGravity * Time.deltaTime;
        }
    }
    
    #endregion
    
    #region 회전/이동 관련 기능
    
    public void Idle()
    {
        float inputMagnitude = 1.0f;
        mSpeed = 0.0f;
        
        mAnimationBlend = Mathf.Lerp(mAnimationBlend, mMoveSpeed, Time.deltaTime * mSpeedChangeRate);
        if (mAnimationBlend < 0.01f)
        {
            mAnimationBlend = 0f;
        }
        
        mCharacterController.Move(new Vector3(0.0f, mGravity, 0.0f) * Time.deltaTime);
        
        if (mHasAnimator)
        {
            PlayerAnimator.SetFloat("Speed", mAnimationBlend);
            PlayerAnimator.SetFloat("MotionSpeed", inputMagnitude);
        }
    }

    public void Move()
    {
        float targetSpeed = GameManager.Instance.Input.SprintInput ? mSprintSpeed : mMoveSpeed;
        float currentHorizontalSpeed = new Vector3(mCharacterController.velocity.x, 0.0f, mCharacterController.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = 1.0f;
        
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            mSpeed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * mSpeedChangeRate);
            mSpeed = Mathf.Round(mSpeed * 1000f) / 1000f;
        }
        else
        {
            mSpeed = targetSpeed;
        }

        mAnimationBlend = Mathf.Lerp(mAnimationBlend, targetSpeed, Time.deltaTime * mSpeedChangeRate);
        if (mAnimationBlend < 0.01f)
        {
            mAnimationBlend = 0f;
        }
        
        Vector3 inputDirection = new Vector3(GameManager.Instance.Input.MoveInput.x, 0.0f, GameManager.Instance.Input.MoveInput.y).normalized;
        
        mTargetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mMainCamera.transform.eulerAngles.y;
        float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, mTargetRotation, ref mRotationVelocity, mRotationSmoothTime);
        transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        
        Vector3 targetDirection = Quaternion.Euler(0.0f, mTargetRotation, 0.0f) * Vector3.forward;
        mCharacterController.Move(targetDirection.normalized * (mSpeed * Time.deltaTime) + new Vector3(0.0f, mGravity, 0.0f) * Time.deltaTime);
        
        if (mHasAnimator)
        {
            PlayerAnimator.SetFloat("Speed", mAnimationBlend);
            PlayerAnimator.SetFloat("MotionSpeed", inputMagnitude);
        }
    }

    #endregion

    #region 점프 관련 기능
    
    public void Jump()
    {
        if (mJumpTimeoutDelta < 0.0f)
        {
            mVerticalVelocity = -2.0f;
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
    
    public void EndLand()
    {
        //mPlayerStateLand.bIsLand = false;
    }
    
    #endregion

    #region 공격 관련 기능

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
    
    public void Attack()
    {
        
    }

    // 공격 애니메이션의 공격 모션 시작 시 호출 메서드
    public void MeleeAttackStart()
    {
        if (CurrentPlayerState == PlayerState.Attack)
        {
            mWeaponController.AttackStart();
        }
    }

    // 공격 애니메이션의 공격 모션 종료 시 호출되는 메서드
    public void MeleeAttackEnd()
    {
        if (CurrentPlayerState == PlayerState.Attack)
        {
            mWeaponController.AttackEnd();
        }
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
    
    #region 디버깅 관련

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (mbIsGrounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;
        
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - mGroundedOffset, transform.position.z),
            mGroundedRadius);
    }

    #endregion
}