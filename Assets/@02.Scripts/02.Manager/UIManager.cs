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

public class UIManager : Singleton<UIManager>
{
    readonly Stack<BaseUIPanel> mStack = new();
    [SerializeField] Canvas mPanelCanvas;
    [SerializeField] BaseUIPanel mSettingPrefab;
    [SerializeField] BaseUIPanel mCodexPrefab;
    [SerializeField] BaseUIPanel mQuestBoardPrefab;
    [SerializeField] BaseUIPanel mEscGroupPrefab;

    private Dictionary<UIPanelType, BaseUIPanel> mPanels = new();
    protected override void Awake()
    {
        base.Awake();
        mPanels.Add(UIPanelType.Setting, mSettingPrefab);
        mPanels.Add(UIPanelType.Codex, mCodexPrefab);
        mPanels.Add(UIPanelType.QuestBoard, mQuestBoardPrefab);
        mPanels.Add(UIPanelType.EscGroup, mEscGroupPrefab);
    }

    public async UniTask Push(BaseUIPanel prefab, Action onComplete = null)
    {
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

    public async UniTask Pop()
    {
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
        else if (mStack.Count == 0 && GameManager.Instance.CurrentGameState == GameState.UI)
        {
            GameManager.Instance.ChangePreviousGameState();
        }
    }
    void Update()
    {
        ProcessEscInput();
    }

    private void ProcessEscInput()
    {
        if (GameManager.Instance.Input.EscInput)
        {
            Pop().Forget();
        }
        else if(GameManager.Instance.Input.CursorToggleInput)
        {
            Push(UIPanelType.EscGroup).Forget();
        }
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }
}