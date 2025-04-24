using Events.Player;
using R3;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public sealed class HpBarPresenter : HudPresenterBase
{
    [SerializeField] private Image mFill;
    private void OnEnable()
    {
        R3EventBus.Instance.Receive<PlayerHpChanged>()
            .Subscribe(e => mFill.fillAmount = e.Current / (float)e.Max)
            .AddTo(mCD);
    }
}


//     // ─────────────────────────────────────────────────────────────────────────
//     // 4. EXP Bar Presenter
//     // ─────────────────────────────────────────────────────────────────────────
//     public sealed class ExpBarPresenter : HudPresenterBase
//     {
//         [SerializeField] private Image _fill;
//         [SerializeField] private TextMeshProUGUI _levelText;
//         private void OnEnable()
//         {
//             R3EventBus.Instance.Receive<PlayerExpChanged>()
//                 .Subscribe(e =>
//                 {
//                     _fill.fillAmount = e.Current / (float)e.Max;
//                 })
//                 .AddTo(_cd);
//         }
//     }
//
//     // ─────────────────────────────────────────────────────────────────────────
//     // 5. Currency Presenter (Gold / Soul)
//     // ─────────────────────────────────────────────────────────────────────────
//     public sealed class CurrencyPresenter : HudPresenterBase
//     {
//         [SerializeField] private TextMeshProUGUI _goldText;
//         [SerializeField] private TextMeshProUGUI _soulText;
//         private void OnEnable()
//         {
//             R3EventBus.Instance.Receive<CurrencyChanged>()
//                 .Subscribe(e =>
//                 {
//                     _goldText.text = e.Gold.ToString();
//                     _soulText.text = e.Soul.ToString();
//                 })
//                 .AddTo(_cd);
//         }
//     }
//
//     // ─────────────────────────────────────────────────────────────────────────
//     // 6. Skill Slot Presenter (handles array of slots)
//     // ─────────────────────────────────────────────────────────────────────────
//     [Serializable]
//     public sealed class SkillSlot
//     {
//         public Image Icon;
//         public Image CooldownMask; // radial mask; fillAmount 1→0
//     }
//
//     public sealed class SkillBarPresenter : HudPresenterBase
//     {
//         [SerializeField] private SkillSlot[] _slots;
//         private void OnEnable()
//         {
//             R3EventBus.Instance.Receive<SkillCooldown>()
//                 .Subscribe(e =>
//                 {
//                     if (e.Index < 0 || e.Index >= _slots.Length) return;
//                     _slots[e.Index].CooldownMask.fillAmount = 1f - e.Ratio; // ratio 0~1
//                 })
//                 .AddTo(_cd);
//         }
//     }
//
//     // ─────────────────────────────────────────────────────────────────────────
//     // 7. Difficulty / Timer Presenter
//     // ─────────────────────────────────────────────────────────────────────────
//     public sealed class DifficultyPresenter : HudPresenterBase
//     {
//         [SerializeField] private TextMeshProUGUI _levelText;
//         [SerializeField] private TextMeshProUGUI _timeText;
//         private void OnEnable()
//         {
//             R3EventBus.Instance.Receive<DifficultyTick>()
//                 .Subscribe(e =>
//                 {
//                     _levelText.text = $"Lv {e.Level}";
//                     _timeText.text  = $"{e.Elapsed:mm\:ss}";
//                 })
//                 .AddTo(_cd);
//         }
//     }
//
//     // ─────────────────────────────────────────────────────────────────────────
//     // 8. Boss Bar Presenter
//     // ─────────────────────────────────────────────────────────────────────────
//     public sealed class BossBarPresenter : HudPresenterBase
//     {
//         [SerializeField] private CanvasGroup _root;
//         [SerializeField] private TextMeshProUGUI _nameText;
//         [SerializeField] private Image _fill;
//         private int _maxHp;
//
//         private void OnEnable()
//         {
//             // 시작 시 숨김
//             _root.alpha = 0;
//             _root.gameObject.SetActive(false);
//
//             R3EventBus.Instance.Receive<BossEngaged>()
//                 .Subscribe(e =>
//                 {
//                     _maxHp = e.MaxHp;
//                     _nameText.text = e.Name;
//                     _fill.fillAmount = 1;
//                     ShowAsync().Forget();
//                 })
//                 .AddTo(_cd);
//
//             R3EventBus.Instance.Receive<BossHpChanged>()
//                 .Subscribe(e =>
//                 {
//                     if (_maxHp == 0) return;
//                     _fill.fillAmount = e.Current / (float)_maxHp;
//                 })
//                 .AddTo(_cd);
//         }
//
//         private async UniTaskVoid ShowAsync()
//         {
//             _root.gameObject.SetActive(true);
//             _root.alpha = 0;
//             await _root.DOFade(1, 0.25f).ToUniTask();
//         }
//     }
//
//     // ─────────────────────────────────────────────────────────────────────────
//     // 9. HUD Root bootstrap – attach to HUD Canvas root
//     // ─────────────────────────────────────────────────────────────────────────
//     public sealed class HudBootstrap : MonoBehaviour
//     {
//         [SerializeField] private HpBarPresenter _hp;
//         [SerializeField] private ExpBarPresenter _exp;
//         [SerializeField] private SkillBarPresenter _skills;
//         [SerializeField] private CurrencyPresenter _currency;
//         [SerializeField] private DifficultyPresenter _difficulty;
//         [SerializeField] private BossBarPresenter _boss;
//
//         private void Awake()
//         {
//             // This class intentionally empty – just forces serialization
//             // of references so that presenters are guaranteed to exist.
//         }
//     }
// }
//
// // ─────────────────────────────────────────────────────────────────────────────
// // END OF FILE
// // ─────────────────────────────────────────────────────────────────────────────
