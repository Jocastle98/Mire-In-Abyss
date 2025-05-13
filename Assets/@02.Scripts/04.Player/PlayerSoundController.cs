using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerSoundController : MonoBehaviour
{
    [Header("Player Movement AudioClips")]
    // 이동 중 호흡 보이스?
    public AudioClip[] footstepAudioClips;
    
    [Space(10)]
    [Header("Player Jump AudioClips")]
    public AudioClip[] jumpVoiceAudioClips;
    public AudioClip[] landingVoiceAudioClips;
    public AudioClip landingAudioClip;
    
    [Space(10)]
    [Header("Player Attack AudioClips")]
    public AudioClip[] attackVoiceAudioClips;
    public AudioClip[] swordSwingAudioClips;
    public AudioClip[] swordHitAudioClips;
    
    [Space(10)]
    [Header("Player Hit AudioClips")]
    public AudioClip[] hitVoiceAudioClips;
    public AudioClip[] hitAudioClips;
    public AudioClip[] blockShieldAudioClips;
    
    [Space(10)]
    [Header("Player Death AudioClips")]
    public AudioClip[] deathVoiceAudioClips;
    
    [Space(10)]
    [Header("Player Skill AudioClips")]
    public AudioClip[] skillVoiceAudioClips;
    public AudioClip[] skillAudioClips;
    
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
            if (footstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, footstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(footstepAudioClips[index], transform.position /*, 볼륨 */);
            }
        }
    }

    private void OnJumpSound(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            var index = Random.Range(0, jumpVoiceAudioClips.Length);
            AudioSource.PlayClipAtPoint(jumpVoiceAudioClips[index], transform.position);
        }
    }
    
    private void OnLandSound(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(landingAudioClip, transform.position /*, 볼륨 */);
            
            var index = Random.Range(0, landingVoiceAudioClips.Length);
            AudioSource.PlayClipAtPoint(landingVoiceAudioClips[index], transform.position);
        }
    }

    public void OnSwordSwingSound()
    {
        if (swordSwingAudioClips.Length > 0)
        {
            var index = Random.Range(0, swordSwingAudioClips.Length);
            AudioSource.PlayClipAtPoint(swordSwingAudioClips[index], transform.position);
            
            var index2 = Random.Range(0, attackVoiceAudioClips.Length);
            AudioSource.PlayClipAtPoint(attackVoiceAudioClips[index2], transform.position);
        }
    }
    
    public void OnSwordHitSound()
    {
        // 맞는 몬스터에 따라 타격음을 다르게 할지?
        if (swordHitAudioClips.Length > 0)
        {
            var index = Random.Range(0, swordHitAudioClips.Length);
            AudioSource.PlayClipAtPoint(swordHitAudioClips[index], transform.position);
        }
    }

    public void OnHitSound(bool isDefend)
    {
        if (isDefend)
        {
            if (blockShieldAudioClips.Length > 0)
            {
                var index = Random.Range(0, blockShieldAudioClips.Length);
                AudioSource.PlayClipAtPoint(blockShieldAudioClips[index], transform.position);
            }
        }
        else
        {
            if (hitAudioClips.Length > 0)
            {
                var index = Random.Range(0, hitAudioClips.Length);
                AudioSource.PlayClipAtPoint(hitAudioClips[index], transform.position);
                
                var index2 = Random.Range(0, hitVoiceAudioClips.Length);
                AudioSource.PlayClipAtPoint(hitVoiceAudioClips[index2], transform.position);
            }
        }
    }

    public void OnDeathSound()
    {
        if (deathVoiceAudioClips.Length > 0)
        {
            var index = Random.Range(0, deathVoiceAudioClips.Length);
            AudioSource.PlayClipAtPoint(deathVoiceAudioClips[index], transform.position);
        }
    }
    
    public void OnSkillSound(SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.Skill1:
                AudioSource.PlayClipAtPoint(skillAudioClips[0], transform.position);
                break;
            case SkillType.Skill2:
                AudioSource.PlayClipAtPoint(skillAudioClips[1], transform.position);
                break;
            case SkillType.Skill3:
                AudioSource.PlayClipAtPoint(skillAudioClips[2], transform.position);
                break;
            case SkillType.Skill4:
                AudioSource.PlayClipAtPoint(skillAudioClips[3], transform.position);
                break;
        }

        if (skillVoiceAudioClips.Length > 0)
        {
            var index = Random.Range(0, skillVoiceAudioClips.Length);
            AudioSource.PlayClipAtPoint(skillVoiceAudioClips[index], transform.position);
        }
    }
}