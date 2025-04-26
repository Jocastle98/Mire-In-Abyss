using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;
using Events.Player;

public sealed class CurrencyPresenter : HudPresenterBase
{
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _soulText;

    private void Start()
    {
        subscribeEvents();
    }

    private void subscribeEvents()
    {
        R3EventBus.Instance.Receive<CurrencyChanged>()
            .Subscribe(e =>
            {
                setCurrencyText(e.Gold, e.Soul);
            })
            .AddTo(mCD);
    }

    private void setCurrencyText(int gold, int soul)
    {
        _goldText.text = gold.ToString();
        _soulText.text = soul.ToString();
    }
}