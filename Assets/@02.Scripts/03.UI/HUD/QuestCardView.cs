using Events.Quest;
using TMPro;
using UIHUDEnums;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public sealed class QuestCardView : MonoBehaviour
{
    [SerializeField] TMP_Text  mTitleText;
    [SerializeField] TMP_Text  mDescText;
    [SerializeField] Image     mBG;

    static readonly Color CompletedColor = new(0,0.75f,0,0.2f);
    static readonly Color ActiveColor    = new(1f,1f,0,0.2f);

    public TempQuestInfo QuestInfo { get; private set; }
    public bool IsCompleted => QuestInfo.State == QuestState.Completed;

    public void Bind(Events.Quest.TempQuestInfo info)
    {
        QuestInfo = info;

        mTitleText.text  = info.Title;
        mDescText.text   = info.ShortDesc;
        mBG.color        = info.State == QuestState.Completed ? CompletedColor : ActiveColor;
    }

    public void ApplyQuestComplete()
    {
        QuestInfo = new TempQuestInfo(QuestInfo.ID, QuestInfo.Title, QuestInfo.ShortDesc, QuestState.Completed);
        mBG.color = CompletedColor;
    }
}