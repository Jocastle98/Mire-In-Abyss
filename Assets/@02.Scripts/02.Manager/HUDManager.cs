using Events.Gameplay;
using SceneEnums;
using R3;
using UnityEngine;
using System.Collections.Generic;

public sealed class HUDManager : MonoBehaviour
{
    [Header("Town 에서 비활성화될 HUD 목록")]
    [SerializeField] List<HudPresenterBase> mTownDisableHuds;


    readonly CompositeDisposable mCd = new();

    void Awake()
    {
        R3EventBus.Instance.Receive<GameplayModeChanged>()
            .Subscribe(OnModeChanged)
            .AddTo(mCd);
    }

    void OnDisable() => mCd.Dispose();

    void OnModeChanged(GameplayModeChanged e)
    {
        if (e.NewMode == GameplayMode.Town)
        {
            foreach (var hud in mTownDisableHuds)
            {
                hud.gameObject.SetActive(false);
            }
        }
        else if(e.NewMode == GameplayMode.Abyss)
        {
            foreach (var hud in mTownDisableHuds)
            {
                hud.gameObject.SetActive(true);
            }
        }
    }
}
