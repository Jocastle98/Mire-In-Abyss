using UnityEngine;
using UnityEngine.InputSystem;
using R3;
using GameEnums;

public sealed class InputRouter : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;    // GameManager에 붙어 있는 컴포넌트
    InputActionMap mGameplay, mUI;

    void Awake()
    {
        mGameplay = playerInput.actions.FindActionMap("Player");
        mUI       = playerInput.actions.FindActionMap("UI");

        // 상태 변화 → Map On/Off
        GameManager.Instance.ObserveState
            .Subscribe(UpdateMaps)
            .AddTo(this);

        UpdateMaps(GameManager.Instance.CurrentGameState);   // 부팅 직후 동기화
    }

    void UpdateMaps(GameState s)
    {
        mGameplay.Disable();
        mUI.Disable();
        switch (s)
        {
            case GameState.Gameplay:
                mGameplay.Enable(); break;
            case GameState.UI:
                mUI.Enable();       break;
            case GameState.GameplayPause:
                break;
        }
    }
}
