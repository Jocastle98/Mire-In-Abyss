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
    public void Bind(BuffAdded buffInfo)
    {
        if (mID != buffInfo.ID)
        {
            mID = buffInfo.ID;
            setSpriteAsync(buffInfo.ID);
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

    async void setSpriteAsync(int id)
    {
        //TODO: 버프 이미지 로드, 어드레서블 추가
        /* if (mCacheBuffSpriteDict.TryGetValue(id, out var sp))
        {
            mBuffImage.sprite = sp;
            return;
        }

        string address = $"Buff/{id}";
        AsyncOperationHandle<Sprite> h = Addressables.LoadAssetAsync<Sprite>(address);
        await h.Task;

        if (h.Status == AsyncOperationStatus.Succeeded)
        {
            mCacheBuffSpriteDict[id] = h.Result;
            mBuffImage.sprite = h.Result;
        }
        else
        {
            Debug.LogWarning($"Buff icon {id} not found.");
        } */
    }

    private void PublishBuffEnd()
    {
        R3EventBus.Instance.Publish(new BuffEnded(mID));
        mID = -1;
    }
}
