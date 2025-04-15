using System;
using PlayerEnums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager
{
    public Vector2 LookInput { get; private set; }
    public Vector2 MoveInput { get; private set; }
    public bool JumpInput { get; private set; }
    public bool RollInput { get; private set; }
    public bool AttackInput { get; private set; }
    public bool DefendInput { get; private set; }
    public bool ParryInput { get; private set; }

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
        JumpInput = mJumpAction.WasPressedThisFrame();
        RollInput = mRollAction.WasPressedThisFrame();
        AttackInput = mAttackAction.WasPressedThisFrame();
        DefendInput = mDefendAction.WasPressedThisFrame();
        ParryInput = mParryAction.WasPressedThisFrame();
    }
}