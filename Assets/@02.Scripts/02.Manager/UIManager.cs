using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public class UIManager : MonoBehaviour
{
    readonly Stack<BaseUIPanel> mStack = new();
    [SerializeField] Canvas mPanelCanvas;
    [SerializeField] BaseUIPanel mPausePrefab;

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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            newMethod();
        }
    }

    private void newMethod()
    {
        if (mStack.Count == 0)
        {
            Push(mPausePrefab).Forget();
        }
        else
        {
            Pop().Forget();
        }
    }
}