using System;
using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour, IObserver<GameObject>
{
    [Header("Reference")]
    [SerializeField] private PlayerStats mPlayerStats;
    
    [Space(10)]
    [Header("Control Variable")]
    [SerializeField] private float mSpeed;
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
    [Header("Player Roll Stat")]
    [SerializeField] private float mRollDistance = 5.0f;
    
    [Space(10)]
    [Header("Player Dash Stat")]
    [SerializeField] private float mDashDistance = 10.0f;
    
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
    
    
    // Player Calculation Stat
    [SerializeField]
    private float mVerticalVelocity;
    private float mRotationVelocity;
    private float mTerminalVelocity = 53.0f;
    private float mTargetRotation;
    private float mAnimationBlend;
    
    private bool mbInCombat = false;
    private float mInCombatTimeout = 5.0f;
    
    // Timeout Deltatime
    private float mJumpTimeoutDelta;
    private float mFallTimeoutDelta;
    private float mInCombatTimeoutDelta;
    
    
    // Componenet
    private Animator mPlayerAnimator;
    public Animator PlayerAnimator => mPlayerAnimator;
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
        mPlayerAnimator = GetComponent<Animator>();
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

        mPlayerStats.OnDeath += () =>
        {
            SetPlayerState(PlayerState.Dead);
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
        InCombatCheck();
    }

    public void Init()
    {
        GameManager.Instance.Input.Init(mPlayerInput);
        mJumpTimeoutDelta = mJumpTimeout;
        mFallTimeoutDelta = mFallTimeout;
        
        SetPlayerState(PlayerState.Idle);
        
        /*// 스탯 초기화
        mCurrentAttackPower = mBaseAttackPower;
        mCurrentDefendPower = mBaseDefendPower;
        mCurrentHealth = mMaxHealth;*/
        
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
                if (CurrentPlayerState != PlayerState.Attack && CurrentPlayerState != PlayerState.Dash)
                {
                    SetPlayerState(PlayerState.Fall);
                }
            }
        }
        
        if (mVerticalVelocity < mTerminalVelocity)
        {
            mVerticalVelocity += mGravity * Time.deltaTime;
        }
    }
    
    private void InCombatCheck()
    {
        if (mbInCombat)
        {
            mInCombatTimeout -= Time.deltaTime;
            if (mInCombatTimeout <= 0.0f)
            {
                mbInCombat = false;
            }
        }
    }
    
    #endregion
    
    #region 회전/이동 관련 기능
    
    public Vector3 GetCameraForwardDirection(bool isHorizontalOnly)
    {
        // 카메라 설정
        var cameraForward = mMainCamera.transform.forward;
        
        // Y값을 0으로 설정해서 수평 방향만 고려
        if (isHorizontalOnly)
        {
            cameraForward.y = 0;
        }
        cameraForward.Normalize();
        
        return cameraForward;
    }
    
    public Vector3 SetRollDirection()
    {
        var targetDirection = GetCameraForwardDirection(true);
        
        if (GameManager.Instance.Input.MoveInput != Vector2.zero)
        {
            Vector3 inputDirection = 
                new Vector3(GameManager.Instance.Input.MoveInput.x, 0.0f, GameManager.Instance.Input.MoveInput.y).normalized;
            float targetRotation = 
                Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mMainCamera.transform.eulerAngles.y;

            targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
        }
        
        return targetDirection;
    }

    public Vector3 SetDashDirection()
    {
        var targetDirection = GetCameraForwardDirection(true);
        
        if (!mbIsGrounded)
        {
            targetDirection = GetCameraForwardDirection(false);
        }
        
        return targetDirection;
    }
    
    // 이동 계산
    public void CalculateMovement(bool allowRotation)
    {
        // 속도 계산
        float targetSpeed;
        if (CurrentPlayerState != PlayerState.Defend)
        {
            if (GameManager.Instance.Input.SprintInput)
            {
                targetSpeed = mPlayerStats.GetMoveSpeed() * 1.5f;
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
        
        // 2. 현재 속도 측정 (수평 이동만 고려)
        float currentHorizontalSpeed = new Vector3(mCharacterController.velocity.x, 0, mCharacterController.velocity.z).magnitude;
        
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
        
        // 4. 애니메이션 블렌드 계산
        mAnimationBlend = Mathf.Lerp(mAnimationBlend, targetSpeed, Time.deltaTime * mSpeedChangeRate);
        if (mAnimationBlend < 0.01f)
        {
            mAnimationBlend = 0f;
        }
        
        // 4. 방향 처리 (회전 허용 여부 분기)
        Vector3 inputDirection = 
            new Vector3(GameManager.Instance.Input.MoveInput.x, 0.0f, GameManager.Instance.Input.MoveInput.y).normalized;
        
        mTargetRotation = 
            Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mMainCamera.transform.eulerAngles.y;
        
        if (allowRotation)
        {
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, mTargetRotation, 
                                                                        ref mRotationVelocity, mRotationSmoothTime);
            Vector3 rotationDirection =new Vector3(0.0f, rotation, 0.0f);
            transform.rotation = Quaternion.Euler(rotationDirection);
        }
        
        if (mbInCombat)
        {
            transform.rotation = Quaternion.LookRotation(GetCameraForwardDirection(true));
        }
        
        Vector3 targetDirection = Quaternion.Euler(0.0f, mTargetRotation, 0.0f) * Vector3.forward;
        
        if (CurrentPlayerState == PlayerState.Move)
        {
            mCharacterController.Move(targetDirection.normalized * (mSpeed * Time.deltaTime) + new Vector3(0.0f, mGravity, 0.0f) * Time.deltaTime);
        }
        else
        {
            mCharacterController.Move(targetDirection.normalized * (mSpeed * Time.deltaTime) + new Vector3(0.0f, mVerticalVelocity, 0.0f) * Time.deltaTime);
        }
        
        mPlayerAnimator.SetFloat("Speed", mAnimationBlend);
        mPlayerAnimator.SetFloat("Vertical", GameManager.Instance.Input.MoveInput.y); // 임시
        mPlayerAnimator.SetFloat("Horizontal", GameManager.Instance.Input.MoveInput.x); //임시
        mPlayerAnimator.SetFloat("MotionSpeed", inputMagnitude);
    }
    
    public void Idle()
    {
        mSpeed = 0.0f;
        float inputMagnitude = 1.0f;
        
        if (CurrentPlayerState == PlayerState.Idle)
        {
            mCharacterController.Move(new Vector3(0.0f, mGravity, 0.0f) * Time.deltaTime);
        }
        else
        {
            mCharacterController.Move(new Vector3(0.0f, mVerticalVelocity, 0.0f) * Time.deltaTime);
        }
        
        mPlayerAnimator.SetFloat("Speed", mSpeed);
        mPlayerAnimator.SetFloat("MotionSpeed", inputMagnitude);
    }

    public void Move()
    {
        CalculateMovement(true);
    }

    private void AttackMove()
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

    // 구르기 쿨타임 만들어야 함(연속 사용시 애니메이션 전환 문제, 애니메이션 완전히 종료되기전 사용시 멈춤)
    public void Roll()
    {
        StartCoroutine(RollCoroutine());
    }

    private IEnumerator RollCoroutine()
    {
        // 애니메이션 초기 구간 기다림
        yield return new WaitForSeconds(0.15f); // 선딜(현재 애니메이션에 맞춤)
        // 일정시간 무적?
        RollFuntion(true);
        mPlayerStateRoll.bIsRoll = true;
        
        yield return new WaitForSeconds(0.25f);
        RollFuntion(false);
        
        yield return new WaitForSeconds(0.9f); // 애니메이션 종료 직전
        mPlayerStateRoll.bIsRoll = false;
        
        if (GameManager.Instance.Input.MoveInput == Vector2.zero)
        {
            SetPlayerState(PlayerState.Idle);
        }
        else
        {
            SetPlayerState(PlayerState.Move);
        }
    }
    
    public void Rolling(Vector3 targetDirection)
    {
        if (mPlayerStateRoll.bIsRoll)
        {
            float distanceCovered = 0f;
            float maxDistance = mRollDistance;
            float speed = 5.0f; // 초기 속도 조정
        
            while (distanceCovered < maxDistance)
            {
                float moveAmount = speed * Time.deltaTime;
                mCharacterController.Move(targetDirection * moveAmount);
                distanceCovered += moveAmount;
                return;
            }
        }
    }

    private void RollFuntion(bool isRollFunction)
    {
        if (isRollFunction)
        {
            
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

    public void EnterCombat()
    {
        mbInCombat = true;
        mInCombatTimeout = mInCombatTimeoutDelta;
    }
    
    public void Attack()
    {
        GameManager.Instance.Input.SprintOff();
        
        // 이동 공격시 하반신(Base Layer) 애니메이션
        if (GameManager.Instance.Input.MoveInput == Vector2.zero)
        {
            Idle();
            
            mPlayerAnimator.SetBool("Idle", true);
            mPlayerAnimator.SetBool("Move", false);
            mPlayerAnimator.SetBool("Jump", false);
        }
        else
        {
            AttackMove();
            
            if (mbIsGrounded)
            {
                // 공격 중 이동 상태
                mPlayerAnimator.SetBool("Idle", false);
                mPlayerAnimator.SetBool("Move", true);
                mPlayerAnimator.SetBool("Jump", false);

                // 공격 중 점프 상태
                if (GameManager.Instance.Input.JumpInput)
                {
                    if (mJumpTimeoutDelta < 0.0f)
                    {
                        mVerticalVelocity = 0.0f;
                        mVerticalVelocity = Mathf.Sqrt(mJumpHeight * -2.0f * mGravity);
                        
                        mPlayerAnimator.SetBool("Jump", false);
                    }
                }
            }
            else
            {
                // 공격 중 낙하 상태
                mPlayerAnimator.SetBool("Idle", false);
                mPlayerAnimator.SetBool("Move", false);
                mPlayerAnimator.SetBool("Jump", false);
            }
        }
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

    public void EndCombo()
    {
        mPlayerStateAttack.bIsCombo = false;
    }
    
    public void OnNext(GameObject value)
    {
        var enemyController = value.GetComponent<TestEnemyController>();
        if (enemyController)
        {
            //공격력을 PlayerStats에서 가져와 데미지 계산
            float damage = mPlayerStats.GetAttackDamage();
            enemyController.SetHit(this);
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

    #region 방어 관련 기능

    public void Defend()
    {
        GameManager.Instance.Input.SprintOff();
        
        if (!GameManager.Instance.Input.IsDefending)
        {
            mPlayerAnimator.SetBool("Idle", false);
            mPlayerAnimator.SetBool("Move", false);
            mPlayerAnimator.SetBool("Defend", false);
            SetPlayerState(PlayerState.Idle);
        }
        else
        {
            // 피해감소 or 방어력 증가 기능 메서드
            Defending();
            
            // 이동 방어시 하반신(Base Layer) 애니메이션
            if (GameManager.Instance.Input.MoveInput == Vector2.zero)
            {
                Idle();
                mPlayerAnimator.SetBool("Idle", true);
                mPlayerAnimator.SetBool("Move", false);
            }
            else
            {
                AttackMove();
                mPlayerAnimator.SetBool("Idle", false);
                mPlayerAnimator.SetBool("Move", true);
            }
        }
    }
    
    private void Defending()
    {
        mPlayerStats.OnGuardSuccess();
    }

    /*public void AddItemDefend(int itemDefendPower)
    {
        mCurrentDefendPower += itemDefendPower;
    }*/

    #endregion

    #region 패리 관련 기능

    public void Parry()
    {
        GameManager.Instance.Input.SprintOff();

        // 패리 성공 시 행동 메서드
        ParrySuccess();
        
        // 이동 방어시 하반신(Base Layer) 애니메이션
        if (GameManager.Instance.Input.MoveInput == Vector2.zero)
        {
            Idle();
            mPlayerAnimator.SetBool("Idle", true);
            mPlayerAnimator.SetBool("Move", false);
        }
        else
        {
            AttackMove();
            mPlayerAnimator.SetBool("Idle", false);
            mPlayerAnimator.SetBool("Move", true);
        }
    }

    private void ParrySuccess()
    {
        
    }
    
    #endregion

    #region 대시 관련 기능

    public void Dash()
    {
        StartCoroutine(DashCoroutine());
    }
    
    private IEnumerator DashCoroutine()
    {
        // 애니메이션 초기 구간 기다림
        yield return new WaitForSeconds(0.0f); // 선딜
        // 충돌 무시 및 피해감소? 무적?
        DashFunction(true);
        mPlayerStateDash.bIsDashing = true;
        
        yield return new WaitForSeconds(0.3f);
        DashFunction(false);
        
        yield return new WaitForSeconds(0.7f); // 애니메이션 종료 직전
        mPlayerStateDash.bIsDashing = false;

        if (IsGrounded)
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

    public void Dashing(Vector3 cameraCenterDirection)
    {
        if (mPlayerStateDash.bIsDashing)
        {
            float distanceCovered = 0f;
            float maxDistance = mDashDistance;
            float speed = 10f; // 초기 속도 조정

            while (distanceCovered < maxDistance)
            {
                float moveAmount = speed * Time.deltaTime;
                mCharacterController.Move(cameraCenterDirection * moveAmount);
                distanceCovered += moveAmount;
                return;
            }
        }
    }

    private void DashFunction(bool isDashFunction)
    {
        if (isDashFunction)
        {
            
        }
    }

    #endregion

    #region 피격/사망 관련 기능

    public void SetHit(TestEnemyController enemyController, Vector3 direction)
    {
        if (CurrentPlayerState != PlayerState.Hit)
        {
            //PlayerState의 TakeDamage 메서드 사용
            var enemyPower = enemyController.EnemyAttackPower;
            mPlayerStats.TakeDamage(enemyPower);
        }
        
        // 체력 UI 업데이트
        // GameManager.Instance.SetHP((float)mPlayerStats.GetCurrentHP() / mPlayerStats.GetMaxHP());
        
        if (mPlayerStats.GetCurrentHP() <= 0)
        {
            SetPlayerState(PlayerState.Dead);
        }
        else
        {
            SetPlayerState(PlayerState.Hit);
            // 방향에 따라 맞는 애니메이션이 없으므로 현재는 효과가 없는 것이나 마찬가지인 상태, 일단 대기
            // 플레이어 캐릭터의 방향을 회전시켜주는 수동적인 방식을 사용하거나?
            mPlayerAnimator.SetFloat("HitPosX", -direction.x);
            mPlayerAnimator.SetFloat("HitPosY", -direction.z);
        }
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

        if (mbIsGrounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;
        
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - mGroundedOffset, transform.position.z),
            mGroundedRadius);
    }

    #endregion
}