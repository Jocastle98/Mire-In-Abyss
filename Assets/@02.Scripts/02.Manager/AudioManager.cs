using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AudioEnums;

public class AudioManager : Singleton<AudioManager>
{
    [Header("오디오 소스")]
    [SerializeField] private AudioSource mBgmSource;
    [SerializeField] private AudioSource mSfxSource;
    [SerializeField] private AudioSource mUiSource;

    [Header("오디오 클립")]
    [SerializeField] private AudioClip[] mBgmClips; // 0: 인트로, 1: 마을, 2: 필드, 3: 던전
    [SerializeField] private AudioClip[] mSfxClips;
    [SerializeField] private AudioClip[] mUiClips;

    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void Start()
    {
        AudioListener.volume = PlayerPrefs.GetFloat(Constants.MasterVolumeKey, 1f);
        if (mBgmSource != null) mBgmSource.volume = PlayerPrefs.GetFloat(Constants.BGMVolumeKey, 1f);
        if (mSfxSource != null) mSfxSource.volume = PlayerPrefs.GetFloat(Constants.SFXVolumeKey, 1f);
        if (mUiSource  != null) mUiSource .volume = PlayerPrefs.GetFloat(Constants.UIVolumeKey, 1f);

        AudioListener.pause = PlayerPrefs.GetInt(Constants.MasterMuteKey, 0) == 1;
        if (mBgmSource != null) mBgmSource.mute = PlayerPrefs.GetInt(Constants.BgmMuteKey, 0) == 1;
        if (mSfxSource != null) mSfxSource.mute = PlayerPrefs.GetInt(Constants.SeMuteKey,  0) == 1;
        if (mUiSource  != null) mUiSource .mute = PlayerPrefs.GetInt(Constants.UiMuteKey,  0) == 1;
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

    private void OnMasterVolumeChanged(float v)
    {
        AudioListener.volume = v;
        PlayerPrefs.SetFloat(Constants.MasterVolumeKey, v);
        PlayerPrefs.Save();
    }

    private void OnBgmVolumeChanged(float v)
    {
        mBgmSource.volume = v;
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

    public void SetMasterMute(bool isMuted)
    {
        AudioListener.pause = isMuted;
        PlayerPrefs.SetInt(Constants.MasterMuteKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetBgmMute(bool isMuted)
    {
        mBgmSource.mute = isMuted;
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
        int idx = (int)type;
        if (mSfxClips == null || idx < 0 || idx >= mSfxClips.Length) return;
        var clip = mSfxClips[idx];
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