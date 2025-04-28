using System.Collections.Generic;
using Events.HUD;
using Events.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BuffSlotView : MonoBehaviour
{
    [SerializeField] private Image mBuffImage;
    [SerializeField] private Image mBuffCoolMask;
    private Image mBuffImageBG;
    private float mDuration;
    private float mCoolTime;
    private int mID = -1;
    private static readonly Color mBuffColor = new Color32(165, 255, 255, 255);
    private static readonly Color mDebuffColor = new Color32(255, 189, 189, 255);
    static readonly Dictionary<int, Sprite> mCacheBuffSpriteDict = new();

    void Awake()
    {
        mBuffImageBG = GetComponent<Image>();
    }

    void Update()
    {
        progressBuffCoolTime();
    }
    public void Bind(BuffAdded buffInfo, Sprite sprite)
    {
        if (mID != buffInfo.ID)
        {
            mID = buffInfo.ID;
            mBuffImage.sprite = sprite;
        }

        mBuffImageBG.color = buffInfo.IsDebuff ? mDebuffColor : mBuffColor;
        mDuration = buffInfo.Duration;
        mCoolTime = 0;
        mBuffImage.fillAmount = 0;
    }

    private void progressBuffCoolTime()
    {
        if (mCoolTime < mDuration)
        {
            mCoolTime += Time.deltaTime;
            mBuffCoolMask.fillAmount = mCoolTime / mDuration;
            if (mCoolTime >= mDuration)
            {
                PublishBuffEnd();
            }
        }
    }

    private void PublishBuffEnd()
    {
        R3EventBus.Instance.Publish(new BuffEnded(mID));
        mID = -1;
    }
}
