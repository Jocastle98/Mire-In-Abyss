using System;
using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class AchievementPanelController : PopupPanelController
{
    [SerializeField] private GameObject achievementBlockPrefab;
    [SerializeField] private Transform scrollViewContent;
    [SerializeField] private AchievementDatabase achievementDatabase;

    private List<AchievementBlock> mAchievementBlocks = new List<AchievementBlock>();

    private void Start()
    {
        if (achievementDatabase != null)
        {
            InitializeAchievementList();
        }
    }

    private void InitializeAchievementList()
    {
        ClearAchievementBlocks();

        List<Achievement> allAchievements = achievementDatabase.AllAchievements;

        foreach (var achievement in allAchievements)
        {
            GameObject blockObject = Instantiate(achievementBlockPrefab, scrollViewContent);
            AchievementBlock block = blockObject.GetComponent<AchievementBlock>();

            if (block != null)
            {
                block.Initialize(achievement);
                mAchievementBlocks.Add(block);
            }
        }

        if (scrollViewContent is RectTransform rectTransform)
        {
            Canvas.ForceUpdateCanvases();
            rectTransform.anchoredPosition = Vector2.zero;
        }
        
    }

    private void ClearAchievementBlocks()
    {
        foreach (var block in mAchievementBlocks)
        {
            if (block != null && block.gameObject != null)
            {
                Destroy(block.gameObject);
            }
        }
        mAchievementBlocks.Clear();
    }

    public void RefreshAchievementList()
    {
        InitializeAchievementList();
    }

    public override void Show()
    {
        base.Show();
        if (achievementDatabase != null)
        {
            RefreshAchievementList();
        }
    }


    /// <summary>
    /// 닫기 버튼 클릭 시 호출되는 메서드
    /// </summary>
    public void OnClickCloseButton()
    {
        Hide(() => { mPlayer.SetPlayerState(PlayerState.Idle); });
    }
}
