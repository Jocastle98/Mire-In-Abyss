using Events.Abyss;
using Events.Player;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public sealed class DifficultyPresenter : HudPresenterBase
{
    [SerializeField] TextMeshProUGUI mDifficultyLevelText; // Easy, Normal, Hard , …
    [SerializeField] TextMeshProUGUI mNextDifficultyLevelText;
    [SerializeField] TextMeshProUGUI mAbyssElapseText; // 00:00 어비스 진입 후 경과 시간
    [SerializeField] private Image mDifficultyLevelImage;
    [SerializeField] private ProgressBarUI mLevelProgressBar;


    void Start()
    {
        subscribeEvents();
    }

    private void subscribeEvents()
    {
        R3EventBus.Instance.Receive<DifficultyChanged>()
            .Subscribe(updateDifficultyUI)
            .AddTo(mCD);
        R3EventBus.Instance.Receive<DifficultyProgressed>()
            .Subscribe(updateDifficultyProgressUI)
            .AddTo(mCD);
        R3EventBus.Instance.Receive<PlayTimeChanged>()
            .Subscribe(updateElpseUI)
            .AddTo(mCD);
    }

    void updateDifficultyUI(DifficultyChanged e)
    {
        // TODO: 게임 난이도 기획 사항과 합일 시키기 (해당 역할을 담당하는 곳에서 값을 받아오는 방식으로 연결)
        // TODO: 난이도 이미지와 연결
        // mDifficultyLevelImage.sprite = 해당 난이도 이미지

        // TODO: 난이도 끝에 도달했을 시 Next Level 처리
        mDifficultyLevelText.text = getTempLevelText(e.DifficultyLevel);
        mNextDifficultyLevelText.text = getTempLevelText(e.DifficultyLevel + 1);
    }

    void updateDifficultyProgressUI(DifficultyProgressed e)
    {
        // TODO: 난이도 끝에 도달했을 시 ProgressBar 처리
        mLevelProgressBar.SetProgress(e.DifficultyProgress);
    }

    void updateElpseUI(PlayTimeChanged e)
    {
        mAbyssElapseText.text = $"{e.Elapsed:mm\\:ss}";
    }

    // TODO: 게임 난이도 기획 사항과 합일 시키기 (해당 역할을 담당하는 곳에서 값을 받아오는 방식으로 연결)
    private string getTempLevelText(int level)
    {
        string ret = level switch
        {
            <= 1 => "Easy",
            <= 2 => "Normal",
            <= 3 => "Hard",
            <= 4 => "Hell",
            <= 5 => "Hell+",
            _ => "Hell++"
        };

        return ret;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}