using System.Collections;
using System.Collections.Generic;
using AudioEnums;
using PlayerEnums;
using UnityEngine;

public class PlayerSoundController : MonoBehaviour
{
    private Animator mPlayerAnimator;
    
    private void Start()
    {
        mPlayerAnimator = GetComponent<Animator>();
    }
    
    // 발소리, 나중에 사운드매니저로 관리해야 함
    private void OnFootstepSound(AnimationEvent animationEvent)
    {
        var mobilityLayer = mPlayerAnimator.GetLayerIndex("Mobility Layer");
        var skillLayer = mPlayerAnimator.GetLayerIndex("Skill Layer");
        
        if (animationEvent.animatorClipInfo.weight > 0.5f && 
            (mPlayerAnimator.GetLayerWeight(mobilityLayer) < 1.0f && mPlayerAnimator.GetLayerWeight(skillLayer) < 1.0f))
        {
            AudioManager.Instance.PlaySfx(ESfxType.FootstepEffect);
        }
    }

    private void OnJumpSound(AnimationEvent animationEvent)
    {
        AudioManager.Instance.PlaySfx(ESfxType.JumpVoice);
    }
    
    private void OnLandSound(AnimationEvent animationEvent)
    {
        AudioManager.Instance.PlaySfx(ESfxType.LandVoice);
        AudioManager.Instance.PlaySfx(ESfxType.LandEffect);
    }

    public void OnSwordSwingSound()
    {
        AudioManager.Instance.PlaySfx(ESfxType.AttackVoice);
        AudioManager.Instance.PlaySfx(ESfxType.SwordSwingEffect);
    }
    
    public void OnSwordHitSound()
    {
        AudioManager.Instance.PlaySfx(ESfxType.EnemyHitEffect);
    }

    public void OnHitSound(bool isDefend)
    {
        if (isDefend)
        {
            AudioManager.Instance.PlaySfx(ESfxType.ShieldBlockEffect);
        }
        else
        {
            AudioManager.Instance.PlaySfx(ESfxType.PlayerHitVoice);
            AudioManager.Instance.PlaySfx(ESfxType.PlayerHitEffect);
        }
    }

    public void OnDeathSound()
    {
        AudioManager.Instance.PlaySfx(ESfxType.DeathVoice);
    }
    
    public void OnSkillSound(SkillType skillType)
    {
        AudioManager.Instance.PlaySfx(ESfxType.SkillVoice);
        
        switch (skillType)
        {
            case SkillType.Skill1:
                AudioManager.Instance.PlaySfx(ESfxType.Skill1Effect);
                break;
            case SkillType.Skill2:
                AudioManager.Instance.PlaySfx(ESfxType.Skill2Effect);
                break;
            case SkillType.Skill3:
                AudioManager.Instance.PlaySfx(ESfxType.Skill3Effect);
                break;
            case SkillType.Skill4:
                AudioManager.Instance.PlaySfx(ESfxType.Skill4Effect);
                break;
        }
    }
}