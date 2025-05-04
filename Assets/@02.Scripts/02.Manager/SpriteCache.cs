using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UIEnums;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public sealed class SpriteCache : Singleton<SpriteCache>
{
    private UniTask mPreloadTask;
    private bool mbIsPreloaded = false;
    private readonly Dictionary<int, Sprite> mItemSpritesMap  = new();
    private readonly Dictionary<int, Sprite> mSkillSpritesMap = new();
    private readonly Dictionary<int, Sprite> mBuffSpritesMap  = new();


    public Sprite GetSprite(SpriteType type, int id)
    {
        var dict = GetDict(type);
        if (dict.TryGetValue(id, out var sp)) 
        {
            return sp;
        }

        Debug.LogWarning($"Sprite {type} {id} not found");
        return null;
    }

    //TODO: 로딩창과 연계
    /* ---------- PRELOAD ---------- */
    public UniTask PreloadAsync()
    {
        if (mbIsPreloaded)
        {
            return UniTask.CompletedTask;
        }

        if (mPreloadTask.Status == UniTaskStatus.Pending)
        {
            return mPreloadTask;
        }

        mPreloadTask = actuallyPreloadAsync();       // 모든 Addressables label 로드
        mbIsPreloaded = true;    
        return mPreloadTask;
    }

    private async UniTask actuallyPreloadAsync()
    {
        var tasks = new UniTask[]
        {
            LoadLabelAsync(mItemSpritesMap,  "Item_Icons"),
            LoadLabelAsync(mSkillSpritesMap, "Skill_Icons"),
            LoadLabelAsync(mBuffSpritesMap,  "Buff_Icons")
        };
        await UniTask.WhenAll(tasks);
    }

    /* ---------- HELPERS ---------- */
    Dictionary<int, Sprite> GetDict(SpriteType t) =>
        t switch
        {
            SpriteType.Item  => mItemSpritesMap,
            SpriteType.Skill => mSkillSpritesMap,
            SpriteType.Buff  => mBuffSpritesMap,
            _ => null
        };

    async UniTask LoadLabelAsync(Dictionary<int,Sprite> dict, string label)
    {
        var h = Addressables.LoadAssetsAsync<Sprite>(label, sp =>
        {
            int id = int.Parse(sp.name.Split('_')[1]); // item_101
            dict[id] = sp;
        });
        await h.Task;
    }

    /* ---------- Optional release ---------- */
    public void Clear()
    {
        foreach (var sp in mItemSpritesMap.Values)  Addressables.Release(sp);
        foreach (var sp in mSkillSpritesMap.Values) Addressables.Release(sp);
        foreach (var sp in mBuffSpritesMap.Values)  Addressables.Release(sp);
        mItemSpritesMap.Clear(); mSkillSpritesMap.Clear(); mBuffSpritesMap.Clear();
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }
}
