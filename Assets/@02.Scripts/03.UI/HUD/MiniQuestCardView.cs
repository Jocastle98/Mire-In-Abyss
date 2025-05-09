using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MiniQuestCardView : MonoBehaviour
{
    [SerializeField] TMP_Text  mTitleText;
    [SerializeField] TMP_Text  mDescText;
    [SerializeField] TMP_Text  mProgressText;
    [SerializeField] Image     mBG;

    static readonly Color CompletedColor = new(0,0.75f,0,0.2f);
    static readonly Color ActiveColor    = new(1f,1f,0,0.2f);

    public string ID { get; private set; }
    public int Progress { get; private set; }
    public int Target { get; private set; }

    public bool IsCompleted = false;

    public void Bind(string id, int progress, int target)    
    {
        ID = id;
        Target = target;
        Progress = progress;

        var quest = getQuestInfo(id);
        mTitleText.text  = quest.Title;
        mDescText.text   = quest.RequestInformation;
        mProgressText.text = $"{Progress} / {Target}";
        mBG.color        = IsCompleted ? CompletedColor : ActiveColor;
    }

    public void QuestUpdated(int progress)
    {
        Progress = progress;
        mProgressText.text = $"{Progress} / {Target}";
    }

    public void QuestCompleted()
    {
        IsCompleted = true;
        mBG.color = CompletedColor;
    }

    private Quest getQuestInfo(string id)
    {
        return GameDB.Instance.QuestDatabase.GetQuestById(id);
    }
}