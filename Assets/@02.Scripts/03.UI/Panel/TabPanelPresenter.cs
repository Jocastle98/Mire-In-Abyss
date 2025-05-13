using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabPanelPresenter : BaseUIPanel
{
    [Header("Tab Buttons")]
    [SerializeField] List<TabButtonView> mTabButtonViews;
    [Header("Pages")]
    [SerializeField] List<TabPresenterBase> mPages;

    private List<Button> mButtons;

    void Start()
    {
        mButtons = new List<Button>();
        for (int i = 0; i < mTabButtonViews.Count; i++)
        {
            mPages[i].Initialize();
            mButtons.Add(mTabButtonViews[i].GetComponent<Button>());
            int j = i;
            mButtons[i].onClick.AddListener(() => show(mPages[j]));
        }
        show(mPages[0]);                 // 기본 탭 선택
    }

    void show(TabPresenterBase targetPanel)
    {
        for (int i = 0; i < mPages.Count; i++)
        {
            mTabButtonViews[i].SetSelected(targetPanel == mPages[i]);
            mPages[i].gameObject.SetActive(targetPanel == mPages[i]);
        }
    }
}
