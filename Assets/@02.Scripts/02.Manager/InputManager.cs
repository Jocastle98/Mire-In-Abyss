using System;
using PlayerEnums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BufferedInput
{
    private float bufferTime;
    private float timer;
    private bool isBuffered;

    public BufferedInput(float bufferTime = 0.2f)
    {
        this.bufferTime = bufferTime;
    }

    public void SetBuffer()
    {
        this.isBuffered = true;
        timer = bufferTime;
    }

    public void OnBufferUpdate(float deltaTime)
    {
        if (isBuffered)
        {
            timer -= deltaTime;
            if (timer <= 0.0f)
            {
                isBuffered = false;
            }
        }
    }

    public bool ConsumeInput()
    {
        if (isBuffered)
        {
            isBuffered = false;
            return true;
        }
        
        return false;
    }
    
    public bool IsBuffered => isBuffered;
}

public class InputManager
{
    public Vector2 LookInput { get; private set; }
    public Vector2 MoveInput { get; private set; }
    public bool JumpInput => mJumpBuffer.ConsumeInput();
    public bool RollInput => mRollBuffer.ConsumeInput();
    public bool AttackInput => mAttackBuffer.ConsumeInput();
    public bool DefendInput => mDefendBuffer.ConsumeInput();
    public bool ParryInput => mParryBuffer.ConsumeInput();

    private BufferedInput mJumpBuffer = new BufferedInput();
    private BufferedInput mRollBuffer = new BufferedInput();
    private BufferedInput mAttackBuffer = new BufferedInput();
    private BufferedInput mDefendBuffer = new BufferedInput();
    private BufferedInput mParryBuffer = new BufferedInput();
    
    private InputAction mLookAction;
    private InputAction mMoveAction;
    private InputAction mJumpAction;
    private InputAction mRollAction;
    private InputAction mAttackAction;
    private InputAction mDefendAction;
    private InputAction mParryAction;
    
    private PlayerInput mPlayerInput;
    
    public void Init(PlayerInput playerInput)
    {
        mPlayerInput = playerInput;
        
        mLookAction = playerInput.actions["Look"];
        mMoveAction = playerInput.actions["Move"];
        mJumpAction = playerInput.actions["Jump"];
        mRollAction = playerInput.actions["Roll"];
        mAttackAction = playerInput.actions["Attack"];
        mDefendAction = playerInput.actions["Defend"];
        mParryAction = playerInput.actions["Parry"];
    }

    public void OnInputUpdate()
    {
        if (mPlayerInput == null)
        {
            return;
        }
        
        LookInput = mLookAction.ReadValue<Vector2>();
        MoveInput = mMoveAction.ReadValue<Vector2>();

        float deltaTime = Time.deltaTime;
        if (mJumpAction.WasPressedThisFrame())
        {
            mJumpBuffer.SetBuffer();
        }
        mJumpBuffer.OnBufferUpdate(deltaTime);

        if (mRollAction.WasPressedThisFrame())
        {
            mRollBuffer.SetBuffer();
        }
        mRollBuffer.OnBufferUpdate(deltaTime);

        if (mAttackAction.WasPressedThisFrame())
        {
            mAttackBuffer.SetBuffer();
        }
        mAttackBuffer.OnBufferUpdate(deltaTime);

        if (mDefendAction.WasPressedThisFrame())
        {
            mDefendBuffer.SetBuffer();
        }
        mDefendBuffer.OnBufferUpdate(deltaTime);

        if (mParryAction.WasPressedThisFrame())
        {
            mParryBuffer.SetBuffer();
        }
        mParryBuffer.OnBufferUpdate(deltaTime);
    }
}