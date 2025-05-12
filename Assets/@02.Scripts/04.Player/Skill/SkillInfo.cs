using PlayerEnums;
using UnityEngine;

public class SkillInfo
{
    public int ID;
    public string KeyString;
    public float CooldownTime;

    public SkillInfo(int id, string keyString, float cooldownTime)
    {
        ID = id;
        KeyString = keyString;
        CooldownTime = cooldownTime;
    }

    public void SetCooldown(float cooldownTime)
    {
        CooldownTime = cooldownTime;
    }
}
