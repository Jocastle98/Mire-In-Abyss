using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Events.Data;
using Events.Player;
using R3;
using UnityEngine;

public sealed class SkillSlotsPresenter : HudPresenterBase
{
    [SerializeField] SkillSlotView mSlotPrefab;
    [SerializeField] RectTransform mSkillSlotRoot;
    [SerializeField] List<SkillSlotView> mDefaultSkillSlots = new();
    private readonly Dictionary<int, SkillSlotView> mSlots = new();
    private bool mSkillInfoLoaded = false;


    public override void Initialize()
    {
        subscribeEvents();
        if (mSkillInfoLoaded)
        {
            setSkillSlots();
        }
    }

    private void subscribeEvents()
    {
        // 스킬 사용 수신
        R3EventBus.Instance.Receive<SkillUsed>()
            .Subscribe(e =>
            {
                if (mSlots.TryGetValue(e.ID, out var slot))
                {
                    slot.SkillUsed();
                }
            })
            .AddTo(mCD);

        // 스킬 쿨다운 감소, 키코드 변경 수신
        R3EventBus.Instance.Receive<SkillUpdated>()
            .Subscribe(e =>
            {
                if (mSlots.TryGetValue(e.ID, out var slot))
                {
                    slot.Bind(e.CooldownTime, e.KeyString, e.ID);
                }
            })
            .AddTo(mCD);

        R3EventBus.Instance.Receive<SkillInfoLoaded>()
            .Subscribe(e =>
            {
                mSkillInfoLoaded = true;
                setSkillSlots();
            })
            .AddTo(mCD);
    }

    private void setSkillSlots()
    {
        int skillCount = mDefaultSkillSlots.Count;
        List<SkillInfo> skillInfos = PlayerHub.Instance.Skills.GetSkillInfos();

        // 기본 스킬 정보 등록
        for (int i = 0; i < mDefaultSkillSlots.Count; i++)
        {
            mDefaultSkillSlots[i].Bind(skillInfos[i].CooldownTime, skillInfos[i].KeyString, skillInfos[i].ID);
            mSlots[skillInfos[i].ID] = mDefaultSkillSlots[i];
        }

        // 추가 스킬 정보 등록
        for (int i = mDefaultSkillSlots.Count; i < skillInfos.Count; i++)
        {
            var slot = Instantiate(mSlotPrefab, mSkillSlotRoot);
            slot.Bind(skillInfos[i].CooldownTime, skillInfos[i].KeyString, skillInfos[i].ID);
            mSlots[skillInfos[i].ID] = slot;
        }

    }

    protected override void OnDisable()
    {
        base.OnDisable();
        for (int i = mDefaultSkillSlots.Count; i < mSlots.Count; i++)
        {
            Destroy(mSlots[i].gameObject);
        }
        mSlots.Clear();
    }
}
