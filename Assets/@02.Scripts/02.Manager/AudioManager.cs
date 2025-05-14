using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AudioEnums;
using R3;
using Cysharp.Threading.Tasks;
using UnityEngine.Serialization;

public class AudioManager : Singleton<AudioManager>
{
    [Header("오디오 소스")]
    [SerializeField] private AudioSource mBgmSource;
    [SerializeField] private AudioSource mSfxSource;
    [SerializeField] private AudioSource mUiSource;

    [Header("오디오 클립")]
    [SerializeField] private AudioClip[] mBgmClips; // 0: 인트로, 1: 마을, 2: 필드, 3: 던전
    [SerializeField] private AudioClip[] mUiClips;
    
    [Space(10)]
    [Header("SFX 클립")]
    public AudioClip[] footstepAudioClips;
    public AudioClip[] gruntVoiceAudioClips;
    public AudioClip[] landingVoiceAudioClips;
    public AudioClip[] landingAudioClips;
    public AudioClip[] attackVoiceAudioClips;
    public AudioClip[] swordSwingAudioClips;
    public AudioClip[] swordHitAudioClips;
    public AudioClip[] hitVoiceAudioClips;
    public AudioClip[] hitAudioClips;
    public AudioClip[] blockShieldAudioClips;
    public AudioClip[] stunVoiceAudioClips;
    public AudioClip[] deathVoiceAudioClips;
    public AudioClip[] skillVoiceAudioClips;
    public AudioClip[] skill1AudioClips;
    public AudioClip[] skill2AudioClips;
    public AudioClip[] skill3AudioClips;
    public AudioClip[] skill4AudioClips;
    public AudioClip[] interactionVoiceAudioClips;
    
    private Dictionary<ESfxType, AudioClip[]> mSfxClips;
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void InitAudioDataFromUserData()
    {
        AudioListener.volume = UserData.Instance.MasterVolume;
        mBgmSource.volume = UserData.Instance.BgmVolume;
        mSfxSource.volume = UserData.Instance.SeVolume;
        mUiSource.volume = UserData.Instance.UiVolume;

        AudioListener.pause = UserData.Instance.IsMasterMuted;
        mBgmSource.mute = UserData.Instance.IsBgmMuted;
        mSfxSource.mute = UserData.Instance.IsSeMuted;
        mUiSource.mute = UserData.Instance.IsUiMuted;
        
        InitPlayerSfx();

        subscribeUserAudioData();
    }

    private void subscribeUserAudioData()
    {
        UserData.Instance.ObsMasterVolume.Subscribe(OnMasterVolumeChanged).AddTo(this);
        UserData.Instance.ObsBgmVolume.Subscribe(OnBgmVolumeChanged).AddTo(this);
        UserData.Instance.ObsSeVolume.Subscribe(OnSfxVolumeChanged).AddTo(this);
        UserData.Instance.ObsUiVolume.Subscribe(OnUIVolumeChanged).AddTo(this);
        
    }

    private void InitPlayerSfx()
    {
        mSfxClips = new Dictionary<ESfxType, AudioClip[]>
        {
            { ESfxType.FootstepEffect, footstepAudioClips },
            { ESfxType.GruntVoice, gruntVoiceAudioClips },
            { ESfxType.LandVoice, landingVoiceAudioClips },
            { ESfxType.LandEffect, landingAudioClips },
            { ESfxType.AttackVoice, attackVoiceAudioClips },
            { ESfxType.SwordSwingEffect, swordSwingAudioClips },
            { ESfxType.EnemyHitEffect, swordHitAudioClips },
            { ESfxType.PlayerHitVoice, hitVoiceAudioClips },
            { ESfxType.PlayerHitEffect, hitAudioClips },
            { ESfxType.ShieldBlockEffect, blockShieldAudioClips },
            { ESfxType.StunVoice, stunVoiceAudioClips },
            { ESfxType.DeathVoice, deathVoiceAudioClips },
            { ESfxType.SkillVoice, skillVoiceAudioClips },
            { ESfxType.Skill1Effect, skill1AudioClips },
            { ESfxType.Skill2Effect, skill2AudioClips },
            { ESfxType.Skill3Effect, skill3AudioClips },
            { ESfxType.Skill4Effect, skill4AudioClips },
            { ESfxType.InteractionVoice, interactionVoiceAudioClips }
        };
    }
    
    private void OnMasterVolumeChanged(float v)
    {
        AudioListener.volume = v;
        UserData.Instance.MasterVolume = v;
    }

    private void OnBgmVolumeChanged(float v)
    {
        mBgmSource.volume = v;
        UserData.Instance.BgmVolume = v;
    }

    private void OnSfxVolumeChanged(float v)
    {
        mSfxSource.volume = v;
        UserData.Instance.SeVolume = v;
    }

    private void OnUIVolumeChanged(float v)
    {
        mUiSource.volume = v;
        UserData.Instance.UiVolume = v;
    }

    public void SetMasterMute(bool isMuted)
    {
        AudioListener.pause = isMuted;
        UserData.Instance.IsMasterMuted = isMuted;
    }

    public void SetBgmMute(bool isMuted)
    {
        mBgmSource.mute = isMuted;
        UserData.Instance.IsBgmMuted = isMuted;
    }

    public void SetSeMute(bool isMuted)
    {
        mSfxSource.mute = isMuted;
        UserData.Instance.IsSeMuted = isMuted;
    }

    public void SetUiMute(bool isMuted)
    {
        mUiSource.mute = isMuted;
        UserData.Instance.IsUiMuted = isMuted;
    }

    public void PlayBgm(EBgmType type)
    {
        int idx = (int)type;
        if (mBgmClips == null || idx < 0 || idx >= mBgmClips.Length)
            return;
        var clip = mBgmClips[idx];
        if (clip == null) return;
        mBgmSource.clip = clip;
        mBgmSource.loop = true;
        mBgmSource.Play();
    }

    public void StopBgm()
    {
        if (mBgmSource.isPlaying)
            mBgmSource.Stop();
    }

    public void PlaySfx(ESfxType type)
    {
        if (mSfxClips == null || !mSfxClips.TryGetValue(type, out var clips) || clips == null || clips.Length == 0) return;
        var clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) return;
        mSfxSource.PlayOneShot(clip);
    }

    public void PlayUi(EUiType type)
    {
        int idx = (int)type;
        if (mUiClips == null || idx < 0 || idx >= mUiClips.Length) return;
        var clip = mUiClips[idx];
        if (clip == null) return;
        mUiSource.PlayOneShot(clip);
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case Constants.MainMenuScene:
                PlayBgm(EBgmType.Intro);
                break;
            case Constants.TownScene:
                PlayBgm(EBgmType.Town);
                break;
            case  Constants.AbyssFieldScene:
                PlayBgm(EBgmType.Field);
                break;
            case Constants.AbyssDungeonScene:
                PlayBgm(EBgmType.Dungeon);
                break;
            default:
                StopBgm();
                break;
        }
    }
}