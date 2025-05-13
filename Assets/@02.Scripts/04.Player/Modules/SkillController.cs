using System.Collections.Generic;
using System.Linq;
using Events.Data;
using Events.Player;
using PlayerEnums;
using R3;
using UnityEngine;

public sealed class SkillController : MonoBehaviour
{
    private Dictionary<SkillType, SkillInfo> mSkillInfoMap = new();
    public IReadOnlyDictionary<SkillType, SkillInfo> Skills => mSkillInfoMap;

    public readonly Subject<SkillUsed> SkillUsed = new();
    public readonly Subject<SkillUpdated> SkillUpdated = new();

    // 현재 PlayerController에서 임시로 스킬 정보 가져오는 중
    //TODO: 스킬 정보 관리 기능 추가 필요
    public void SetSkills(Dictionary<SkillType, SkillInfo> skillInfos)
    {
        mSkillInfoMap = skillInfos;
        R3EventBus.Instance.Publish(new SkillInfoLoaded());
    }

    public List<SkillInfo> GetSkillInfos()
    {
        return mSkillInfoMap.Values.ToList();
    }

    public void UseSkill(SkillType skillType)
    {
        if (mSkillInfoMap.TryGetValue(skillType, out var skillInfo))
        {
            SkillUsed.OnNext(new SkillUsed(skillInfo.ID));
        }
    }

    public void UpdateSkillCooldown(SkillType skillType, float cooldownTime)
    {
        if (mSkillInfoMap.TryGetValue(skillType, out var skillInfo))
        {
            skillInfo.CooldownTime = cooldownTime;
            SkillUpdated.OnNext(new SkillUpdated(skillInfo.ID, cooldownTime, skillInfo.KeyString));
        }
    }
}
