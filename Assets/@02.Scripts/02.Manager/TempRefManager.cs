using UnityEngine;
using UnityEngine.SceneManagement;

public class TempRefManager : Singleton<TempRefManager>
{
    public PlayerStats PlayerStats => mPlayerStats;
    public GameObject Player => mPlayer;

    [SerializeField] private PlayerStats mPlayerStats;
    [SerializeField] private GameObject mPlayer;

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }
}
