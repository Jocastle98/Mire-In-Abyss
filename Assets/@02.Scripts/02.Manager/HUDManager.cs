using Events.Gameplay;
using SceneEnums;
using R3;
using UnityEngine;
using System.Collections.Generic;

public sealed class HUDManager : MonoBehaviour
{
    [SerializeField] List<HudPresenterBase> mAllHuds;


    readonly CompositeDisposable mCd = new();

    void Awake()
    {
        R3EventBus.Instance.Receive<GameplaySceneChanged>()
            .Subscribe(OnModeChanged)
            .AddTo(mCd);
    }

    void OnDisable() => mCd.Dispose();

    void OnModeChanged(GameplaySceneChanged e)
    {
        foreach (var hud in mAllHuds)
        {
            if (hud.DisableScene != e.NewScene)
            {
                hud.gameObject.SetActive(true);
                hud.Initialize();
            }
            else
            {
                hud.gameObject.SetActive(false);
            }
        }
    }
}
