using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;
using Events.Player;

public sealed class CurrencyPresenter : HudPresenterBase
{
    [SerializeField] private TextMeshProUGUI mGoldText;
    [SerializeField] private TextMeshProUGUI mSoulText;

    private void Start()
    {
        subscribeEvents();
    }

    private void subscribeEvents()
    {
        R3EventBus.Instance.Receive<GoldChanged>()
            .Subscribe(e => mGoldText.text = e.Gold.ToString())
            .AddTo(mCD);
        R3EventBus.Instance.Receive<SoulChanged>()
            .Subscribe(e => mSoulText.text = e.Soul.ToString())
            .AddTo(mCD);
    }
}