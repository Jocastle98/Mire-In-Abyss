using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UIPanelEnums;
using TMPro;

public sealed class MainMenuPresenter : MonoBehaviour
{
    [SerializeField] Button mBtnPlay, mBtnCodex, mBtnSettings, mBtnCredit, mBtnQuit;
    [SerializeField] TMP_Text mVersionText;

    void Start()
    {
        //Temp
        mBtnPlay    .onClick.AddListener(() => SceneLoader.LoadAsync(Constants.TownScene).Forget());
        mBtnCodex   .onClick.AddListener(() => UIManager.Instance.Push(UIPanelType.Codex).Forget());
        mBtnSettings.onClick.AddListener(() => UIManager.Instance.Push(UIPanelType.Setting).Forget());
        //mBtnCredit  .onClick.AddListener(() => UIManager.Instance.Push(UIPanelType.Credit).Forget());
        mBtnQuit    .onClick.AddListener(QuitGame);

        mVersionText.text = $"v{Application.version}";
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
