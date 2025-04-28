using System.Collections.Generic;
using Events.HUD;
using Events.Player;
using TMPro;
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
    public void Bind(float cooldownTime, KeyCode keyCode, Sprite sprite = null)
    {
        mSkillCooldownTime = cooldownTime;
        mSkillTimer = 0;
        mSkillCoolMask.fillAmount = 0;

        if (mSkillKeyText != null)
        {
            mSkillKeyText.text = keyCode.ToString();
        }
        if (sprite != null)
        {
            mSkillImage.sprite = sprite;
        }
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
