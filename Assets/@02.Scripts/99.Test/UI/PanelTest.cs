using Cysharp.Threading.Tasks;
using UIPanelEnums;
using UnityEngine;

public class PanelTest : MonoBehaviour
{
    [SerializeField] private CanvasGroup mTestButtonGroup;

    void Start()
    {
        mTestButtonGroup.alpha = 0;
        mTestButtonGroup.interactable = false;
        mTestButtonGroup.blocksRaycasts = false;
    }

    public void OnPanelTestButtonToggle()
    {
        mTestButtonGroup.alpha = mTestButtonGroup.alpha == 1 ? 0 : 1;
        mTestButtonGroup.interactable = !mTestButtonGroup.interactable;
        mTestButtonGroup.blocksRaycasts = !mTestButtonGroup.blocksRaycasts;
    }

    public void OnPopPanelTest()
    {
        UIManager.Instance.Pop().Forget();
    }

    public void OnSettingPanelTest()
    {
        UIManager.Instance.Push(UIPanelType.Setting).Forget();
    }

    public void OnCodexPanelTest()
    {
        UIManager.Instance.Push(UIPanelType.Codex).Forget();
    }

    public void OnQuestBoardPanelTest()
    {
        UIManager.Instance.Push(UIPanelType.QuestBoard).Forget();
    }
}
