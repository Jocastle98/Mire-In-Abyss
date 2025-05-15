using System;
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

    //Temp
    private int mDifficultyLevel = 1;
    private float mDifficultyProgress = 0;
    private DateTime mStartUtc;

    void Awake()
    {
        DisableScene = SceneEnums.GameScene.Town;
    }

    public override void Initialize()
    {
        subscribeEvents();
        mDifficultyLevelText.text = getTempLevelText(1);
        mNextDifficultyLevelText.text = getTempLevelText(2);
        mLevelProgressBar.SetProgress(0);
        mAbyssElapseText.text = "00:00";

        //Temp
        mDifficultyLevel = 1;
        mDifficultyProgress = 0;
        mStartUtc = DateTime.UtcNow;
    }

    void Update()
    {
        tempDifficultyTestProgress();
    }

    private void tempDifficultyTestProgress()
    {
        mDifficultyProgress += Time.deltaTime * 0.02f;
        if (mDifficultyProgress >= 1)
        {
            mDifficultyProgress = 0;
            mDifficultyLevel++;
            R3EventBus.Instance.Publish(new DifficultyChanged(mDifficultyLevel));
        }

        R3EventBus.Instance.Publish(new DifficultyProgressed(mDifficultyProgress));

        TimeSpan elapsed = DateTime.UtcNow - mStartUtc;
        R3EventBus.Instance.Publish(new PlayTimeChanged(elapsed));
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

    private void updateDifficultyUI(DifficultyChanged e)
    {
        // TODO: 게임 난이도 기획 사항과 합일 시키기 (해당 역할을 담당하는 곳에서 값을 받아오는 방식으로 연결)
        // TODO: 난이도 이미지와 연결
        // mDifficultyLevelImage.sprite = 해당 난이도 이미지

        // TODO: 난이도 끝에 도달했을 시 Next Level 처리
        mDifficultyLevelText.text = getTempLevelText(e.DifficultyLevel);
        mNextDifficultyLevelText.text = getTempLevelText(e.DifficultyLevel + 1);
    }

    private void updateDifficultyProgressUI(DifficultyProgressed e)
    {
        // TODO: 난이도 끝에 도달했을 시 ProgressBar 처리
        mLevelProgressBar.SetProgress(e.DifficultyProgress);
    }

    private void updateElpseUI(PlayTimeChanged e)
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