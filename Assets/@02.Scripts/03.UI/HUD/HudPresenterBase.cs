using R3;
using UnityEngine;

public abstract class HudPresenterBase : MonoBehaviour
{
    protected readonly CompositeDisposable mCD = new();
    protected virtual void OnDisable() => mCD.Dispose();
}