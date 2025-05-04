using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UIPanelEnums;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class UIManager : Singleton<UIManager>
{
    readonly Stack<BaseUIPanel> mStack = new();
    [SerializeField] Canvas mPanelCanvas;
    [SerializeField] BaseUIPanel mPausePrefab;
    [SerializeField] BaseUIPanel mSettingPrefab;
    //[SerializeField] BaseUIPanel mCodexPrefab;

    private Dictionary<UIPanelType, BaseUIPanel> mPanels = new();
    void Awake()
    {
        mPanels.Add(UIPanelType.Setting, mSettingPrefab);
        //mPanels.Add(UIPanelType.Codex, mCodexPrefab);
    }

    public async UniTask Push(BaseUIPanel prefab)
    {
        if (mStack.TryPeek(out var top))
        {
            top.CG.interactable = false;
        }

        var inst = Instantiate(prefab, mPanelCanvas.transform);
        mStack.Push(inst);
        await inst.Show();
    }

    public async UniTask Push(UIPanelType type)
    {
        if (mPanels.TryGetValue(type, out var prefab))
        {
            await Push(prefab);
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
    }
    void Update()
    {
        //Temp
        if (Input.GetKeyDown(KeyCode.F1))
        {
            newMethod();
        }
    }

    private void newMethod()
    {
        if (mStack.Count == 0)
        {
            //Push(mPausePrefab).Forget();
        }
        else
        {
            Pop().Forget();
        }
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }
}