using Cysharp.Threading.Tasks;
using Events.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(QuestDatabase))]
public class GameDB : Singleton<GameDB>
{
    public QuestDatabase QuestDatabase { get; private set; }
    public SpriteCache SpriteCache { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        QuestDatabase = GetComponent<QuestDatabase>();
        SpriteCache = new SpriteCache();
    }

    async void Start()
    {
        await SpriteCache.PreloadSprites();
        R3EventBus.Instance.Publish(new Preloaded(true));
    }




    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }
}
