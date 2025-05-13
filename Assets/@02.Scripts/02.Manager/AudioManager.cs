using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AudioEnums;
using R3;
using Cysharp.Threading.Tasks;

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

        subscribeUserAudioData();
    }

    private void subscribeUserAudioData()
    {
        UserData.Instance.ObsMasterVolume.Subscribe(OnMasterVolumeChanged).AddTo(this);
        UserData.Instance.ObsBgmVolume.Subscribe(OnBgmVolumeChanged).AddTo(this);
        UserData.Instance.ObsSeVolume.Subscribe(OnSfxVolumeChanged).AddTo(this);
        UserData.Instance.ObsUiVolume.Subscribe(OnUIVolumeChanged).AddTo(this);
        
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