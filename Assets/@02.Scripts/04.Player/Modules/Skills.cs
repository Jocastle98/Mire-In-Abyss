using System.Collections.Generic;
using System.Linq;
using Events.Player;
using R3;
using UnityEngine;

public sealed class SkillDataController : MonoBehaviour
{
    readonly Dictionary<int, SkillInfo> mSkillInfoMap = new();
    public IReadOnlyDictionary<int, SkillInfo> Skills => mSkillInfoMap;

    public readonly Subject<SkillUsed> SkillUsed = new();
    public readonly Subject<SkillUpdated> SkillUpdated = new();

    public void AddSkill(SkillInfo skillInfo)
    {
        mSkillInfoMap.Add(skillInfo.ID, skillInfo);
    }

    public void AddSkills(List<SkillInfo> skillInfos)
    {
        foreach (var skillInfo in skillInfos)
        {
            AddSkill(skillInfo);
        }
    }

    public List<SkillInfo> GetSkillInfos()
    {
        return mSkillInfoMap.Values.ToList();
    }
}
