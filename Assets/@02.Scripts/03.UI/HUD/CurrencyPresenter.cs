using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;
using Events.Player;
using Events.Player.Modules;

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
        R3EventBus.Instance.Receive<GoldAdded>()
            .Subscribe(e => mGoldText.text = TempPlayerHub.Instance.Inventory.Gold.ToString())
            .AddTo(mCD);
        R3EventBus.Instance.Receive<SoulAdded>()
            .Subscribe(e => mSoulText.text = TempPlayerHub.Instance.Inventory.Soul.ToString())
            .AddTo(mCD);
    }
}