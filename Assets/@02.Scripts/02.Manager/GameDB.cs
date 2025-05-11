using Cysharp.Threading.Tasks;
using Events.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(QuestDatabase))]
public class GameDB : Singleton<GameDB>, IInitializable
{
    public QuestDatabase QuestDatabase { get; private set; }
    public SpriteCache SpriteCache { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        QuestDatabase = GetComponent<QuestDatabase>();
        SpriteCache = new SpriteCache();
    }



    public async UniTask InitializeAsync()
    {
        var tasks = new UniTask[]
        {
            //TODO: 모든 DB들 초기화 추가
            SpriteCache.InitializeAsync()
        };
        await UniTask.WhenAll(tasks);
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }
}
