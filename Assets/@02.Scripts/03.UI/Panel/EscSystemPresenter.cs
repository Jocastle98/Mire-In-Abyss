using Cysharp.Threading.Tasks;
using SceneEnums;
using UIPanelEnums;
using UnityEngine;
using UnityEngine.UI;

public sealed class EscSystemPresenter : TabPresenterBase
{
    [SerializeField] Button mBtnTown, mBtnSettings, mBtnMain, mBtnQuit;
    private bool mbIsInit = false;

    public override void Initialize()
    {
        if (!mbIsInit)
        {
            mBtnSettings.onClick.AddListener(() => UIManager.Instance.Push(UIPanelType.Setting).Forget());
            if (SceneLoader.CurrentSceneType == GameScene.Abyss)
            {
                mBtnTown.interactable = true;
                mBtnTown.onClick.AddListener(() => AbyssManager.LetsGoHome());
            }
            else
            {
                mBtnTown.interactable = false;
            }
            mBtnMain.onClick.AddListener(() => SceneLoader.LoadSceneAsync(Constants.MainMenuScene).Forget());
            mBtnQuit.onClick.AddListener(QuitGame);

            //Abyss에 있는 경우에만 활성화
            // mBtnTown.interactable = GameState.Instance.IsInAbyss;

            mbIsInit = true;
        }
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
