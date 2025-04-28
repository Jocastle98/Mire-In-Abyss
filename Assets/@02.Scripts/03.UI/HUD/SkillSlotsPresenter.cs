using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Events.Player;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public sealed class SkillSlotsPresenter : HudPresenterBase
{
    [SerializeField] SkillSlotView slotPrefab;
    [SerializeField] RectTransform mSkillSlotRoot;
    [SerializeField] List<SkillSlotView> mDefaultSkillSlots = new();
    readonly Dictionary<int, SkillSlotView> slots = new();


    void Start()
    {
        subscribeEvents();
        setSkillInfos().Forget();
    }

    void subscribeEvents()
    {
        // 스킬 사용 수신
        R3EventBus.Instance.Receive<SkillUsed>()
            .Subscribe(e =>
            {
                if (slots.TryGetValue(e.ID, out var slot))
                    slot.SkillUsed();
            })
            .AddTo(mCD);

        // 스킬 쿨다운 감소, 키코드 변경 수신
        R3EventBus.Instance.Receive<SkillUpdated>()
            .Subscribe(e =>
            {
                if (slots.TryGetValue(e.ID, out var slot))
                    slot.Bind(e.CooldownTime, e.KeyCode);
            })
            .AddTo(mCD);
    }

    private async UniTaskVoid setSkillInfos()
    {
        List<TempSkillInfo> skillInfos = getSkillInfos();

        Dictionary<int, Sprite> skillIcons = await getSkillIcons(skillInfos);

        // 기본 스킬 정보 등록
        for (int i = 0; i < mDefaultSkillSlots.Count; i++)
        {
            mDefaultSkillSlots[i].Bind(skillInfos[i].CooldownTime, skillInfos[i].KeyCode, skillIcons[skillInfos[i].ID]);
            slots[skillInfos[i].ID] = mDefaultSkillSlots[i];
        }

        // 추가 스킬 정보 등록
        for (int i = mDefaultSkillSlots.Count; i < skillInfos.Count; i++)
        {
            var slot = Instantiate(slotPrefab, mSkillSlotRoot);
            slot.Bind(skillInfos[i].CooldownTime, skillInfos[i].KeyCode, skillIcons[skillInfos[i].ID]);
            slots[skillInfos[i].ID] = slot;
        }

    }

    private async UniTask<Dictionary<int, Sprite>> getSkillIcons(List<TempSkillInfo> skillInfos)
    {
        Dictionary<int, Sprite> skillIcons = new();
        var handle = Addressables.LoadAssetsAsync<Sprite>
        (
            "Skill_Icons",
            sp =>
            {
                int id = int.Parse(sp.name.Split('_')[1]);
                if (skillInfos.Any(info => info.ID == id))
                {
                    skillIcons.Add(id, sp);
                }
            }
        );
        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("Failed to load skill icons");
        }

        Addressables.Release(handle);
        return skillIcons;
    }

    private List<TempSkillInfo> getSkillInfos()
    {
        //TODO: 사용할 스킬들 한 번에 받아오기(여기서는 임시 작성)
        List<TempSkillInfo> skillInfos = new();
        skillInfos.Add(new TempSkillInfo(0, KeyCode.Mouse0, 0f));
        skillInfos.Add(new TempSkillInfo(1, KeyCode.Mouse1, 0f));
        skillInfos.Add(new TempSkillInfo(2, KeyCode.Mouse1, 0f));
        skillInfos.Add(new TempSkillInfo(3, KeyCode.LeftShift, 3f));
        skillInfos.Add(new TempSkillInfo(4, KeyCode.F, 5f));

        return skillInfos;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        foreach (var s in slots.Values)
        {
            Destroy(s.gameObject);
        }
        slots.Clear();
    }
}
