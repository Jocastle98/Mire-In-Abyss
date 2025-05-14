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

    // 구르기나 돌진기도 같이 사용
    public void OnGruntSound()
    {
        AudioManager.Instance.PlaySfx(ESfxType.GruntVoice);
    }
    
    public void OnLandSound()
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

    public void OnStunSound()
    {
        AudioManager.Instance.PlaySfx(ESfxType.StunVoice);
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
            case SkillType.ProjectileFire:
                AudioManager.Instance.PlaySfx(ESfxType.ProjectileFire);
                break;
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