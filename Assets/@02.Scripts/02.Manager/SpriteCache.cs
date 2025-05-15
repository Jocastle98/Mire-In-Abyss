using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UIEnums;
using UnityEngine;
using UnityEngine.AddressableAssets;

public sealed class SpriteCache : IInitializable
{
    bool mLoaded;
    readonly Dictionary<int, Sprite> mItem = new();
    readonly Dictionary<int, Sprite> mSkill = new();
    readonly Dictionary<int, Sprite> mBuff = new();

    /* ─── public API ─────────────────────────────── */
    public Sprite GetSprite(SpriteType t, int id)
    {
        if (!mLoaded)
        {
            Debug.LogWarning("SpriteCache used before preload finished");
        }

        if (GetDict(t).TryGetValue(id, out var sp))
        {
            return sp;
        }
        else
        {
            Debug.LogError($"SpriteCache: {t} {id} not found");
            return null;
        }
    }

    public Dictionary<int, Sprite> GetDict(SpriteType t) => t switch
    {
        SpriteType.Item => mItem,
        SpriteType.Skill => mSkill,
        SpriteType.Buff => mBuff,
        _ => throw new System.Exception($"SpriteCache: {t} not found")
    };

    /* ─── IInitializable ─────────────────────────── */
    public async UniTask InitializeAsync()
    {
        if (mLoaded) 
        {
            return;
        }
        
        await PreloadSpritesAsync();
        mLoaded = true;
    }

    /* ─── internal helpers ───────────────────────── */
    async UniTask PreloadSpritesAsync()
    {
        var tasks = new UniTask[]
        {
            LoadLabelAsync(mItem , "Item_Icons"),
            LoadLabelAsync(mSkill, "Skill_Icons"),
            LoadLabelAsync(mBuff , "Buff_Icons")
        };
        await UniTask.WhenAll(tasks);
    }

    async UniTask LoadLabelAsync(Dictionary<int, Sprite> dict, string label)
    {
        var h = Addressables.LoadAssetsAsync<Sprite>(label, sp =>
        {
            int id = int.Parse(sp.name.Split('_')[1]); // e.g. item_101
            dict[id] = sp;
        });
        await h.Task;
    }
}
