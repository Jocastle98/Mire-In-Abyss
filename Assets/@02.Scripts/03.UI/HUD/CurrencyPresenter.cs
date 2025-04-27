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
        R3EventBus.Instance.Receive<CurrencyChanged>()
            .Subscribe(e =>
            {
                setCurrencyText(e.Gold, e.Soul);
            })
            .AddTo(mCD);
    }

    private void setCurrencyText(int gold, int soul)
    {
        mGoldText.text = gold.ToString();
        mSoulText.text = soul.ToString();
    }
}