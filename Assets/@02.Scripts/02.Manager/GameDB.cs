using Cysharp.Threading.Tasks;
using Events.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(QuestDatabase))]
[RequireComponent(typeof(AchievementDatabase))]
[RequireComponent(typeof(ItemDatabase))]
public class GameDB : Singleton<GameDB>, IInitializable
{
    public QuestDatabase QuestDatabase { get; private set; }
    public SpriteCache SpriteCache { get; private set; }
    public AchievementDatabase AchievementDatabase { get; private set; }
    public ItemDatabase ItemDatabase { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        SpriteCache = new SpriteCache();
        QuestDatabase = GetComponent<QuestDatabase>();
        AchievementDatabase = GetComponent<AchievementDatabase>();
        ItemDatabase = GetComponent<ItemDatabase>();
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
