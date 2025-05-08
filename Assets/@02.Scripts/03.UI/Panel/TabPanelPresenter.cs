using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class TabPanelPresenter : BaseUIPanel
{
    [Header("Tab Buttons")]
    [SerializeField] List<TabButtonView> mTabButtonViews;
    [Header("Pages")]
    [SerializeField] List<GameObject> mPanels;

    private List<Button> mButtons;

    void Start()
    {
        mButtons = new List<Button>();
        for (int i = 0; i < mTabButtonViews.Count; i++)
        {
            mButtons.Add(mTabButtonViews[i].GetComponent<Button>());
            int j = i;
            mButtons[i].onClick.AddListener(() => Show(mPanels[j]));
        }
        Show(mPanels[0]);                 // 기본 탭 선택
    }

    void Show(GameObject targetPanel)
    {
        for (int i = 0; i < mPanels.Count; i++)
        {
            mTabButtonViews[i].SetSelected(targetPanel == mPanels[i]);
            mPanels[i].SetActive(targetPanel == mPanels[i]);
        }
    }
}
