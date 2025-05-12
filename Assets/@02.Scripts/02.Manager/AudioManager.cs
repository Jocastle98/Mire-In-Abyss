using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
using AudioEnums;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Sources")]
    [SerializeField] private AudioSource mBgmSource;
    [SerializeField] private AudioSource mSfxSource;
    [SerializeField] private AudioSource mUiSource;

    [Header("Clips")]
    [SerializeField] private AudioClip[] mBgmClips;  
    [SerializeField] private AudioClip[] mSfxClips; 
    [SerializeField] private AudioClip[] mUiClips;   

    private void Start()
    {
        AudioListener.volume = PlayerPrefs.GetFloat(Constants.MasterVolumeKey, 1f);

        if (mBgmSource != null)
            mBgmSource.volume = PlayerPrefs.GetFloat(Constants.BGMVolumeKey, 1f);

        if (mSfxSource != null)
            mSfxSource.volume = PlayerPrefs.GetFloat(Constants.SFXVolumeKey, 1f);

        if (mUiSource != null)
            mUiSource.volume = PlayerPrefs.GetFloat(Constants.UIVolumeKey, 1f);
    }

    /// <summary>
    /// Setting Panel에서 네 개 슬라이더를 바인딩
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

    public void PlayBgm(EBgmType type)
    {
        int idx = (int)type;
        if (mBgmClips == null || idx < 0 || idx >= mBgmClips.Length)
            return;

        AudioClip clip = mBgmClips[idx];
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
        if (mSfxClips == null || idx < 0 || idx >= mSfxClips.Length)
            return;

        AudioClip clip = mSfxClips[idx];
        if (clip == null) return;

        mSfxSource.PlayOneShot(clip);
    }

    public void PlayUi(EUiType type)
    {
        int idx = (int)type;
        if (mUiClips == null || idx < 0 || idx >= mUiClips.Length)
            return;

        AudioClip clip = mUiClips[idx];
        if (clip == null) return;

        mUiSource.PlayOneShot(clip);
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
    }
}
