using System;
using System.Collections.Generic;
using R3;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public sealed class R3EventBus : Singleton<R3EventBus>, IEventBus
{
    readonly Dictionary<Type, object> mSubjects = new();
    public void Publish<T>(T msg) =>
        getSubject<T>().OnNext(msg);
    public Observable<T> Receive<T>() =>
        getSubject<T>();
    Subject<T> getSubject<T>() =>
        (Subject<T>)(mSubjects.TryGetValue(typeof(T), out var v)
            ? v : mSubjects[typeof(T)] = new Subject<T>());

    public void Dispose()
    {
        foreach (var s in mSubjects.Values)
            ((IDisposable)s).Dispose();
        mSubjects.Clear();
    }
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }
}