using System.Collections.Generic;
using Events.HUD;
using Events.Player;
using TMPro;
using UIEnums;
using UnityEngine;
using UnityEngine.UI;

public sealed class SkillSlotView : MonoBehaviour
{
    [SerializeField] private Image mSkillImage;
    [SerializeField] private Image mSkillCoolMask;
    [SerializeField] private TMP_Text mSkillKeyText;
    private float mSkillCooldownTime;
    private float mSkillTimer;

    void Update()
    {
        progressSkillCoolTime();
    }
    public void Bind(float cooldownTime, string keyString, int id)
    {
        mSkillCooldownTime = cooldownTime;
        mSkillTimer = 0;

        
        if(mSkillCoolMask != null)
        {
            mSkillCoolMask.fillAmount = 0;
        }
        if (mSkillKeyText != null)
        {
            mSkillKeyText.text = keyString;
        }
        if (mSkillImage.sprite == null)
        {
            mSkillImage.sprite = GameDB.Instance.SpriteCache.GetSprite(SpriteType.Skill, id);
        }
    }

    public void UpdateSkillCoolTime(float cooldownTime)
    {
        mSkillCooldownTime = cooldownTime;
    }

    public void SkillUsed()
    {
        mSkillTimer = mSkillCooldownTime;
        mSkillCoolMask.fillAmount = 1;
    }

    private void progressSkillCoolTime()
    {
        if (mSkillTimer > 0)
        {
            mSkillTimer -= Time.deltaTime;
            mSkillCoolMask.fillAmount = mSkillTimer / mSkillCooldownTime;
        }
    }

}
