using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager
{
    public bool CursorToggleInput => mCursorToggleBuffer.ConsumeInputBuffer();
    public Vector2 LookInput { get; private set; }
    public Vector2 MoveInput { get; private set; }
    public bool SprintInput => mSprintBuffer.bIsHolding;
    private bool mSprintToggled = false;
    public bool JumpInput => mJumpBuffer.ConsumeInputBuffer();
    public bool RollInput => mRollBuffer.ConsumeInputBuffer();
    public bool AttackInput => mAttackBuffer.ConsumeInputBuffer();
    // public bool IsAttacking => mAttackBuffer.bIsHolding;
    public bool DefendInput => mDefendBuffer.ConsumeInputBuffer();  // 단발성
    public bool IsDefending => mDefendBuffer.bIsHolding;             // 지속 입력
    public bool ParryInput => mParryBuffer.ConsumeInputBuffer();
    public bool DashInput => mDashBuffer.ConsumeInputBuffer();
    public bool Skill_1Input => mSkill_1Buffer.ConsumeInputBuffer();
    public bool Skill_2Input => mSkill_2Buffer.ConsumeInputBuffer();
    public bool Skill_3Input => mSkill_3Buffer.ConsumeInputBuffer();
    public bool Skill_4Input => mSkill_4Buffer.ConsumeInputBuffer();
    public bool InteractionInput => mInteractionBuffer.ConsumeInputBuffer();
    
    private InputBuffer mCursorToggleBuffer = new InputBuffer();
    private InputBuffer mSprintBuffer = new InputBuffer();
    private InputBuffer mJumpBuffer = new InputBuffer();
    private InputBuffer mRollBuffer = new InputBuffer();
    private InputBuffer mAttackBuffer = new InputBuffer();
    private InputBuffer mDefendBuffer = new InputBuffer();
    private InputBuffer mParryBuffer = new InputBuffer();
    private InputBuffer mDashBuffer = new InputBuffer();
    private InputBuffer mSkill_1Buffer = new InputBuffer();
    private InputBuffer mSkill_2Buffer = new InputBuffer();
    private InputBuffer mSkill_3Buffer = new InputBuffer();
    private InputBuffer mSkill_4Buffer = new InputBuffer();
    private InputBuffer mInteractionBuffer = new InputBuffer();

    private InputAction mCursorToggleAction;
    private InputAction mLookAction;
    private InputAction mMoveAction;
    private InputAction mSprintAction;
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

        mCursorToggleAction = playerInput.actions["CursorToggleAction"];
        mLookAction = playerInput.actions["Look"];
        mMoveAction = playerInput.actions["Move"];
        mSprintAction = playerInput.actions["Sprint"];
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
        
        SetInputBuffer(mCursorToggleAction, mCursorToggleBuffer);
        LookInput = mLookAction.ReadValue<Vector2>();
        MoveInput = mMoveAction.ReadValue<Vector2>();

        if (mSprintAction.WasPressedThisFrame())
        {
            mSprintToggled = !mSprintToggled;
        }
        mSprintBuffer.SetHold(mSprintToggled);
        
        SetInputBuffer(mJumpAction, mJumpBuffer);
        SetInputBuffer(mRollAction, mRollBuffer);
        SetInputBuffer(mAttackAction, mAttackBuffer);
        //mAttackBuffer.SetHold(mAttackAction.IsPressed());
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

    private void SetInputBuffer(InputAction inputAction, InputBuffer inputBuffer)
    {
        if (inputAction.WasPressedThisFrame())
        {
            inputBuffer.SetBuffer();
        }
        inputBuffer.OnBufferUpdate(Time.deltaTime);
    }

    // 상태전환 후 스플린터 해제를 위한 메서드
    public void SprintOff()
    {
        mSprintToggled = false;
        mSprintBuffer.SetHold(false);
    }
}