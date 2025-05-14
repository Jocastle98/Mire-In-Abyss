using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AudioEnums;
using UnityEngine.Serialization;

public class AudioManager : Singleton<AudioManager>
{
    [Header("오디오 소스")]
    [SerializeField] private AudioSource mBgmSource;
    [SerializeField] private AudioSource mSfxSource;
    [SerializeField] private AudioSource mUiSource;

    [Header("오디오 클립")]
    [SerializeField] private AudioClip[] mUiClips;
    
    [Header("오브젝트 풀 클립")]
    [SerializeField] private AudioClip[] mPooledSfxClips;
    [SerializeField] private AudioClip[] mBgmClips; // 0: 인트로, 1: 마을, 2: 필드, 3: 던전

    [Header("오브젝트 풀 세팅")]
    [SerializeField] private int mSfxPoolSize = 16;
    [SerializeField] private int mBgmPoolSize = 2;
    private AudioSource[] mPooledSources;
    private AudioSource[] mBgmSources;
    private int mNextPool = 0;
    private int mNextBgm = 0;

    #region Player SFX 클립

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

    #endregion
    
    
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Awake()
    {
        InitPlayerSfx();
        mPooledSources = new AudioSource[mSfxPoolSize];
        for (int i = 0; i < mSfxPoolSize; i++)
        {
            var go = new GameObject($"PooledSfx_{i}");
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            mPooledSources[i] = src;
        }
        mBgmSources = new AudioSource[mBgmPoolSize];
        mBgmSources[0] = mBgmSource;
        for (int i = 1; i < mBgmPoolSize; i++)
        {
            var go = new GameObject($"BgmSrc_{i}");
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            mBgmSources[i] = src;
        }
    }
    private void Start()
    {
        AudioListener.volume = PlayerPrefs.GetFloat(Constants.MasterVolumeKey, 1f);
        AudioListener.pause  = PlayerPrefs.GetInt(Constants.MasterMuteKey, 0) == 1;

        float bgmVol  = PlayerPrefs.GetFloat(Constants.BGMVolumeKey, 1f);
        bool  bgmMute = PlayerPrefs.GetInt(Constants.BgmMuteKey, 0) == 1;
        foreach (var src in mBgmSources)
        {
            src.volume = bgmVol;
            src.mute   = bgmMute;
            src.loop   = true;
        }

        mUiSource.volume = PlayerPrefs.GetFloat(Constants.UIVolumeKey, 1f);
        mUiSource.mute   = PlayerPrefs.GetInt(Constants.UiMuteKey,  0) == 1;
        
        mSfxSource.volume = PlayerPrefs.GetFloat(Constants.SFXVolumeKey, 1f);
        mSfxSource.mute   = PlayerPrefs.GetInt(Constants.SeMuteKey,    0) == 1;

        float poolVol  = PlayerPrefs.GetFloat(Constants.SFXVolumeKey, 1f);
        bool poolMute  = PlayerPrefs.GetInt(Constants.SeMuteKey,    0) == 1;
        foreach (var src in mPooledSources)
        {
            src.volume = poolVol;
            src.mute   = poolMute;
        }
    }

    /// <summary>
    /// SoundPresenter에서 슬라이더 할당 
    /// </summary>
    public void InitSliders(
        Slider masterSlider,
        Slider bgmSlider,
        Slider sfxSlider,
        Slider uiSlider)
    {
        if (masterSlider != null)
        {
            masterSlider.value = AudioListener.volume;
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        if (bgmSlider != null)
        {
            bgmSlider.value = mBgmSource.volume;
            bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = mSfxSource.volume;
            sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }
        if (uiSlider != null)
        {
            uiSlider.value = mUiSource.volume;
            uiSlider.onValueChanged.AddListener(OnUIVolumeChanged);
        }
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

    #region 슬라이더 변경 값 볼륨 기억

    private void OnMasterVolumeChanged(float v)
    {
        AudioListener.volume = v;
        PlayerPrefs.SetFloat(Constants.MasterVolumeKey, v);
        PlayerPrefs.Save();
    }

    private void OnBgmVolumeChanged(float v)
    {
        foreach (var src in mBgmSources)
            src.volume = v;
        PlayerPrefs.SetFloat(Constants.BGMVolumeKey, v);
        PlayerPrefs.Save();
    }

    private void OnSfxVolumeChanged(float v)
    {
        mSfxSource.volume = v;
        PlayerPrefs.SetFloat(Constants.SFXVolumeKey, v);
        PlayerPrefs.Save();
    }

    private void OnUIVolumeChanged(float v)
    {
        mUiSource.volume = v;
        PlayerPrefs.SetFloat(Constants.UIVolumeKey, v);
        PlayerPrefs.Save();
    }

    #endregion
    

    #region mute 관련

    public void SetMasterMute(bool isMuted)
    {
        AudioListener.pause = isMuted;
        PlayerPrefs.SetInt(Constants.MasterMuteKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetBgmMute(bool isMuted)
    {
        foreach (var src in mBgmSources)
            src.mute = isMuted;
        PlayerPrefs.SetInt(Constants.BgmMuteKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetSeMute(bool isMuted)
    {
        mSfxSource.mute = isMuted;
        PlayerPrefs.SetInt(Constants.SeMuteKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetUiMute(bool isMuted)
    {
        mUiSource.mute = isMuted;
        PlayerPrefs.SetInt(Constants.UiMuteKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    #endregion
    

    public void PlayBgm(EBgmType type)
    {
        int idx = (int)type;
        if (mBgmClips == null || idx < 0 || idx >= mBgmClips.Length) return;
        var clip = mBgmClips[idx];
        if (clip == null) return;

        // 이전 BGM 정지
        int prev = (mNextBgm + mBgmPoolSize - 1) % mBgmPoolSize;
        mBgmSources[prev].Stop();

        // 다음 풀 소스에서 재생
        var src = mBgmSources[mNextBgm];
        src.clip = clip;
        src.Play();
        mNextBgm = (mNextBgm + 1) % mBgmPoolSize;
    }

    public void StopBgm()
    {
        foreach (var src in mBgmSources)
            if (src.isPlaying) src.Stop();
    }

    public void PlaySfx(ESfxType type)
    {
        if (mSfxClips == null || !mSfxClips.TryGetValue(type, out var clips) || clips == null || clips.Length == 0) return;
        var clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) return;
        mSfxSource.PlayOneShot(clip);
    }

    public void PlayPoolSfx(ExSfxType type)
    {
        int idx = (int)type;
        if (mPooledSfxClips == null || idx < 0 || idx >= mPooledSfxClips.Length) return;
        var clip = mPooledSfxClips[idx];
        if (clip == null) return;
        var src = mPooledSources[mNextPool];
        src.PlayOneShot(clip);
        mNextPool = (mNextPool + 1) % mSfxPoolSize;
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