using System.Collections.Generic;
using Events.HUD;
using Events.Player;
using TMPro;
using UIEnums;
using UnityEngine;
using UnityEngine.UI;

public sealed class BuffSlotView : MonoBehaviour
{
    [SerializeField] private Image mBuffImage;
    [SerializeField] private Image mBuffCoolMask;
    private Image mBuffImageBG;
    private float mBuffDurationTime;
    private float mBuffTimer;
    private int mID = -1;
    private static readonly Color mBuffColor = new Color32(165, 255, 255, 255);
    private static readonly Color mDebuffColor = new Color32(255, 189, 189, 255);

    void Awake()
    {
        mBuffImageBG = GetComponent<Image>();
    }

    void Update()
    {
        progressBuffCoolTime();
    }
    public void Bind(BuffAdded buffInfo)
    {
        if (mID != buffInfo.ID)
        {
            mID = buffInfo.ID;
            mBuffImage.sprite = SpriteCache.Instance.GetSprite(SpriteType.Buff, buffInfo.ID);
        }

        mBuffImageBG.color = buffInfo.IsDebuff ? mDebuffColor : mBuffColor;
        mBuffDurationTime = buffInfo.Duration;
        mBuffTimer = 0;
        mBuffImage.fillAmount = 0;
    }

    private void progressBuffCoolTime()
    {
        if (mBuffTimer < mBuffDurationTime)
        {
            mBuffTimer += Time.deltaTime;
            mBuffCoolMask.fillAmount = mBuffTimer / mBuffDurationTime;
            if (mBuffTimer >= mBuffDurationTime)
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
