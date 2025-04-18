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
    private bool isHolding;

    public BufferedInput(float bufferTime = 0.5f)
    {
        this.bufferTime = bufferTime;
    }

    public void SetBuffer()
    {
        this.isBuffered = true;
        timer = bufferTime;
    }

    public void SetHold(bool isPressed)
    {
        isHolding = isPressed;
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
    public bool IsHolding => isHolding;
}

public class InputManager
{
    public Vector2 LookInput { get; private set; }
    public Vector2 MoveInput { get; private set; }
    public bool JumpInput => mJumpBuffer.ConsumeInput();
    public bool RollInput => mRollBuffer.ConsumeInput();
    public bool AttackInput => mAttackBuffer.ConsumeInput();
    public bool DefendInput => mDefendBuffer.ConsumeInput(); // 단발성
    public bool IsDefending => mDefendBuffer.IsHolding; // 지속 입력
    public bool ParryInput => mParryBuffer.ConsumeInput();
    public bool DashInput => mDashBuffer.ConsumeInput();
    public bool Skill_1Input => mSkill_1Buffer.ConsumeInput();
    public bool Skill_2Input => mSkill_2Buffer.ConsumeInput();
    public bool Skill_3Input => mSkill_3Buffer.ConsumeInput();
    public bool Skill_4Input => mSkill_4Buffer.ConsumeInput();
    public bool InteractionInput => mInteractionBuffer.ConsumeInput();

    private BufferedInput mJumpBuffer = new BufferedInput();
    private BufferedInput mRollBuffer = new BufferedInput();
    private BufferedInput mAttackBuffer = new BufferedInput();
    private BufferedInput mDefendBuffer = new BufferedInput();
    private BufferedInput mParryBuffer = new BufferedInput();
    private BufferedInput mDashBuffer = new BufferedInput();
    private BufferedInput mSkill_1Buffer = new BufferedInput();
    private BufferedInput mSkill_2Buffer = new BufferedInput();
    private BufferedInput mSkill_3Buffer = new BufferedInput();
    private BufferedInput mSkill_4Buffer = new BufferedInput();
    private BufferedInput mInteractionBuffer = new BufferedInput();
    
    private InputAction mLookAction;
    private InputAction mMoveAction;
    private InputAction mJumpAction;
    private InputAction mRollAction;
    private InputAction mAttackAction;
    private InputAction mDefendAction;
    private InputAction mParryAction;
    private InputAction mDashAction;
    private InputAction mSkill_1Action;
    private InputAction mSkill_2Action;
    private InputAction mSkill_3Action;
    private InputAction mSkill_4Action;
    private InputAction mInteractionAction;
    
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
        mDashAction = playerInput.actions["Dash"];
        mSkill_1Action = playerInput.actions["Skill_1"];
        mSkill_2Action = playerInput.actions["Skill_2"];
        mSkill_3Action = playerInput.actions["Skill_3"];
        mSkill_4Action = playerInput.actions["Skill_4"];
        mInteractionAction = playerInput.actions["Interaction"];
    }

    public void OnInputUpdate()
    {
        if (mPlayerInput == null)
        {
            return;
        }
        
        LookInput = mLookAction.ReadValue<Vector2>();
        MoveInput = mMoveAction.ReadValue<Vector2>();

        SetInputBuffer(mJumpAction, mJumpBuffer);
        SetInputBuffer(mRollAction, mRollBuffer);
        SetInputBuffer(mAttackAction, mAttackBuffer);
        SetInputBuffer(mDefendAction, mDefendBuffer);
        mDefendBuffer.SetHold(mDefendAction.IsPressed());
        SetInputBuffer(mParryAction, mParryBuffer);
        SetInputBuffer(mDashAction, mDashBuffer);
        SetInputBuffer(mSkill_1Action, mSkill_1Buffer);
        SetInputBuffer(mSkill_2Action, mSkill_2Buffer);
        SetInputBuffer(mSkill_3Action, mSkill_3Buffer);
        SetInputBuffer(mSkill_4Action, mSkill_4Buffer);
        SetInputBuffer(mInteractionAction, mInteractionBuffer);
    }

    private void SetInputBuffer(InputAction inputAction, BufferedInput bufferedInput)
    {
        if (inputAction.WasPressedThisFrame())
        {
            bufferedInput.SetBuffer();
        }
        bufferedInput.OnBufferUpdate(Time.deltaTime);
    }
}