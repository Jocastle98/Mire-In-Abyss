using UnityEngine;

public class SkillInfo
{
    public int ID;
    public KeyCode KeyCode;
    public float CooldownTime;

    public SkillInfo(int id, KeyCode keyCode, float cooldownTime)
    {
        ID = id;
        KeyCode = keyCode;
        CooldownTime = cooldownTime;
    }

    public void SetCooldown(float cooldownTime)
    {
        CooldownTime = cooldownTime;
    }
}
