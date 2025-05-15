using Cysharp.Threading.Tasks;
using R3;
using SceneEnums;
using UnityEngine;

public abstract class HudPresenterBase : MonoBehaviour
{
    public GameScene DisableScene = GameScene.MainMenu;
    protected readonly CompositeDisposable mCD = new();
    protected virtual void OnDisable() => mCD.Clear();

    public abstract void Initialize();
}