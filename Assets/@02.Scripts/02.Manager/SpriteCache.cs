using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Events.Data;
using R3;
using UIEnums;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public sealed class SpriteCache
{
    private bool mbIsPreloaded = false;
    private readonly Dictionary<int, Sprite> mItemSpritesMap  = new();
    private readonly Dictionary<int, Sprite> mSkillSpritesMap = new();
    private readonly Dictionary<int, Sprite> mBuffSpritesMap  = new();
    //private readonly Dictionary<int, Sprite> mAchievementSpritesMap = new();


    public Sprite GetSprite(SpriteType type, int id)
    {
        checkPreloaded();

        var dict = getDict(type);
        if (dict.TryGetValue(id, out var sp)) 
        {
            return sp;
        }

        Debug.LogWarning($"Sprite {type} {id} not found");
        return null;
    }

    public Dictionary<int, Sprite> GetDict(SpriteType type)
    {
        checkPreloaded();
        
        return getDict(type);
    }

    //TODO: 로딩창과 연계
    /* ---------- PRELOAD ---------- */

    public async UniTask PreloadSprites()
    {
        var tasks = new UniTask[]
        {
            LoadLabelAsync(mItemSpritesMap,  "Item_Icons"),
            LoadLabelAsync(mSkillSpritesMap, "Skill_Icons"),
            LoadLabelAsync(mBuffSpritesMap,  "Buff_Icons"),
            //LoadLabelAsync(mAchievementSpritesMap, "Achievement_Icons")
        };
        await UniTask.WhenAll(tasks);

        mbIsPreloaded = true;
    }

    private void checkPreloaded()
    {
        if (!mbIsPreloaded)
        {
            Debug.LogWarning("SpriteCache: GetSprite called before preload finished");
        }
    }

    /* ---------- HELPERS ---------- */
    private Dictionary<int, Sprite> getDict(SpriteType t) =>
        t switch
        {
            SpriteType.Item  => mItemSpritesMap,
            SpriteType.Skill => mSkillSpritesMap,
            SpriteType.Buff  => mBuffSpritesMap,
            //SpriteType.Achievement => mAchievementSpritesMap,
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
        //foreach (var sp in mAchievementSpritesMap.Values) Addressables.Release(sp);
        mItemSpritesMap.Clear(); mSkillSpritesMap.Clear(); mBuffSpritesMap.Clear();
    }
}
