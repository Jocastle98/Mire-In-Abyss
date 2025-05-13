using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Events.UI;
using UIPanelEnums;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using R3;
using GameEnums;
using PlayerEnums;

public class UIManager : Singleton<UIManager>
{
    readonly Stack<BaseUIPanel> mStack = new();
    [SerializeField] Canvas mPanelCanvas;
    [SerializeField] BaseUIPanel mSettingPrefab;
    [SerializeField] BaseUIPanel mCodexPrefab;
    [SerializeField] BaseUIPanel mQuestBoardPrefab;
    [SerializeField] BaseUIPanel mEscGroupPrefab;
    [SerializeField] BaseUIPanel mSoulStoneShopPanelPrefab;
    [SerializeField] BaseUIPanel mPortalPanelPrefab;

    private Dictionary<UIPanelType, BaseUIPanel> mPanels = new();
    protected override void Awake()
    {
        base.Awake();
        mPanels.Add(UIPanelType.Setting, mSettingPrefab);
        mPanels.Add(UIPanelType.Codex, mCodexPrefab);
        mPanels.Add(UIPanelType.QuestBoard, mQuestBoardPrefab);
        mPanels.Add(UIPanelType.EscGroup, mEscGroupPrefab);
        mPanels.Add(UIPanelType.SoulStoneShop, mSoulStoneShopPanelPrefab);
        mPanels.Add(UIPanelType.EnterPortal, mPortalPanelPrefab);
    }

    void Update()
    {
        ProcessEscInput();
    }
    
    public async UniTask Push(BaseUIPanel prefab, Action onComplete = null)
    {
        AudioManager.Instance.PlayUi(AudioEnums.EUiType.Open);
        if (mStack.TryPeek(out var top))
        {
            top.CG.interactable = false;
        }

        var inst = Instantiate(prefab, mPanelCanvas.transform);
        mStack.Push(inst);

        if (mStack.Count == 1)
        {
            GameManager.Instance.SetGameState(GameState.UI);
        }

        await inst.Show(onComplete);
    }

    public async UniTask Push(UIPanelType type, Action onComplete = null)
    {
        if (mPanels.TryGetValue(type, out var prefab))
        {
            await Push(prefab, onComplete);
        }
    }
    #region playercontroller도 전달해야할때 ex 퀘스트, 영혼석 상점, 포탈 패널 취소시 player setstate idle로 변경

    // PlayerController를 전달하는 Push 메서드 추가
    public async UniTask Push(BaseUIPanel prefab, PlayerController player, Action onComplete = null, Action onClose = null)
    {
        AudioManager.Instance.PlayUi(AudioEnums.EUiType.Open);

        if (mStack.TryPeek(out var top))
        {
            top.CG.interactable = false;
        }

        var inst = Instantiate(prefab, mPanelCanvas.transform);

        // PlayerController 설정
        if (player != null)
        {
            inst.SetPlayer(player);
            inst.SetOnCloseCallback(onClose);
        }

        mStack.Push(inst);

        if (mStack.Count == 1)
        {
            GameManager.Instance.SetGameState(GameState.UI);
        }

        await inst.Show(onComplete);
    }

    // PlayerController를 전달하는 UIPanelType 기반 Push 메서드 추가
    public async UniTask Push(UIPanelType type, PlayerController player, Action onComplete = null)
    {
        if (mPanels.TryGetValue(type, out var prefab))
        {
            await Push(prefab, player, onComplete, () =>
            {
                player.SetPlayerState(PlayerState.Idle);
            });
        }
    }

    #endregion

    public async UniTask Pop()
    {
        AudioManager.Instance.PlayUi(AudioEnums.EUiType.Close);

        if (mStack.Count == 0)
        {
            return;
        }
        var top = mStack.Pop();
        await top.Hide();
        Destroy(top.gameObject);

        if (mStack.TryPeek(out var next))
        {
            next.CG.interactable = true;
        }
        else if (mStack.Count == 0)
        {
            // 팝 했을 때 스택이 비어있으면 이전 게임 상태로 돌아감
            GameManager.Instance.ChangePreviousGameState();
        }
    }

    /// <summary>
    /// 씬 변경 시 모든 패널 제거 할 때만 호출
    /// </summary>
    public void PopAll()
    {
        while (mStack.Count > 0)
        {
            Destroy(mStack.Pop().gameObject);
        }
    }

    private void ProcessEscInput()
    {
        if (GameManager.Instance.Input.EscInput)
        {
            if (GameManager.Instance.CurrentGameState == GameState.UI)
            {
                Pop().Forget();
            }
            else if (GameManager.Instance.CurrentGameState == GameState.Gameplay)
            {
                Push(UIPanelType.EscGroup).Forget();
            }
        }
        else if (GameManager.Instance.Input.TabInput)
        {
            //TODO: EscGroup패널을 Inventory 탭으로 시작하게 호출
            if (GameManager.Instance.CurrentGameState == GameState.UI)
            {
                Pop().Forget();
            }
            else if (GameManager.Instance.CurrentGameState == GameState.Gameplay)
            {
                Push(UIPanelType.EscGroup).Forget();
            }
        }
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }
}