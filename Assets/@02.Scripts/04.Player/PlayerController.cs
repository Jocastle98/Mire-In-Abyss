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
public class PlayerController : MonoBehaviour
{
    [Header("Stat")]
    [SerializeField] private int mMaxHealth = 100;
    [SerializeField] private int mAttackPower = 10;
    public int AttackPower => mAttackPower;
    [SerializeField] private int mDefensePower = 5;
    public int DefensePower => mDefensePower;

    [Header("Action")] 
    [SerializeField] private float mMoveSpeed = 5.0f;
    public float MoveSpeed => mMoveSpeed;
    [SerializeField] private float mTurnSpeed = 100.0f;
    [SerializeField] private float mJumpForce = 5.0f;
    [SerializeField] private LayerMask mGroundLayer;
    [SerializeField] private float mMaxGroundCheckDistance = 10.0f;

    [Header("Attach Point")] 
    [SerializeField] private Transform mHeadTransform;
    [SerializeField] private Transform mRightHandTransform;
    [SerializeField] private Transform mLeftHandTransform;
    
    // 상태 관련
    private PlayerStateIdle mPlayerStateIdle;
    private PlayerStateMove mPlayerStateMove;
    private PlayerStateJump mPlayerStateJump;
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
    public bool IsGrounded { get { return GetDistanceToGround() < 0.1f; } }
    
    // 내부에서만 사용되는 변수
    private Rigidbody mRigidbody;
    private CapsuleCollider mCapsuleCollider;
    private PlayerInput mPlayerInput;
    private CameraController mCameraController;
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
        Debug.Log(mMoveSpeed);
        if (CurrentPlayerState != PlayerState.None)
        {
            mPlayerStates[CurrentPlayerState]?.OnUpdate();
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

    public void SetPlayerMoveSpeed(float moveSpeed)
    {
        mMoveSpeed = moveSpeed;
    }

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
            if (vertical > 0)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }
            else if (vertical < 0)
            {
                transform.rotation = Quaternion.LookRotation(-moveDirection);
            }
            
            transform.position += moveDirection * (mMoveSpeed * Time.deltaTime);
        }
    }

    public void Jump()
    {
        Vector2 moveInput = GameManager.Instance.Input.MoveInput;
        
        mRigidbody.AddForce(Vector3.up * mJumpForce, ForceMode.Impulse);
        if (moveInput.y >= 0.0f)
        {
            mRigidbody.AddForce(transform.forward * mJumpForce, ForceMode.Impulse);
        }
        else if (moveInput.y < 0.0f)
        {
            mRigidbody.AddForce(-transform.forward * mJumpForce, ForceMode.Impulse);
        }
    }
}
