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


    void Start()
    {
        subscribeEvents();
        R3EventBus.Instance.Receive<Preloaded>()
            .Subscribe(OnPreloaded)
            .AddTo(this);
    }

    private void OnPreloaded(Preloaded e)
    {
        if (e.IsPreloaded)
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
                    slot.Bind(e.CooldownTime, e.KeyCode, e.ID);
                }
            })
            .AddTo(mCD);
    }

    private void setSkillSlots()
    {
        List<TempSkillInfo> skillInfos = getSkillInfos();

        // 기본 스킬 정보 등록
        for (int i = 0; i < mDefaultSkillSlots.Count; i++)
        {
            mDefaultSkillSlots[i].Bind(skillInfos[i].CooldownTime, skillInfos[i].KeyCode, skillInfos[i].ID);
            mSlots[skillInfos[i].ID] = mDefaultSkillSlots[i];
        }

        // 추가 스킬 정보 등록
        for (int i = mDefaultSkillSlots.Count; i < skillInfos.Count; i++)
        {
            var slot = Instantiate(mSlotPrefab, mSkillSlotRoot);
            slot.Bind(skillInfos[i].CooldownTime, skillInfos[i].KeyCode, skillInfos[i].ID);
            mSlots[skillInfos[i].ID] = slot;
        }

    }

    private List<TempSkillInfo> getSkillInfos()
    {
        //TODO: 사용할 스킬들 한 번에 받아오기(여기서는 임시 작성)
        List<TempSkillInfo> skillInfos = new();
        skillInfos.Add(new TempSkillInfo(0, KeyCode.Mouse0, 0f));
        skillInfos.Add(new TempSkillInfo(1, KeyCode.Mouse1, 0f));
        skillInfos.Add(new TempSkillInfo(2, KeyCode.Mouse1, 0f));
        skillInfos.Add(new TempSkillInfo(3, KeyCode.LeftControl, 0f));
        skillInfos.Add(new TempSkillInfo(4, KeyCode.LeftShift, 3f));
        skillInfos.Add(new TempSkillInfo(5, KeyCode.F, 5f));
        skillInfos.Add(new TempSkillInfo(6, KeyCode.Alpha1, 5f));
        skillInfos.Add(new TempSkillInfo(7, KeyCode.Alpha2, 5f));
        skillInfos.Add(new TempSkillInfo(8, KeyCode.Alpha3, 5f));
        skillInfos.Add(new TempSkillInfo(9, KeyCode.Alpha4, 5f));

        return skillInfos;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        foreach (var s in mSlots.Values)
        {
            Destroy(s.gameObject);
        }
        mSlots.Clear();
    }
}
