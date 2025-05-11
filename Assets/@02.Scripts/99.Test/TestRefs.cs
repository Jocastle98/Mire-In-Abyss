using UnityEngine;
using UnityEngine.SceneManagement;

public class TestRefs : Singleton<TestRefs>
{
    public GameObject PlayerRoot;
    public GameObject PlayerCameraRoot;
    public Camera MiniMapCam;

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }
}