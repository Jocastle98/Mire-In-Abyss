using System.Collections.Generic;
using Events.Player.Modules;
using R3;
using UnityEngine;

public sealed class BuffController : MonoBehaviour
{
    public class BuffState { public float remain; public float dur; }

    readonly Dictionary<int, BuffState> mBuffs = new();
    readonly List<int> mRecycle = new();
    public IReadOnlyDictionary<int, BuffState> ActiveBuffs => mBuffs;

    public readonly Subject<BuffAdded> Added = new();
    public readonly Subject<BuffRefreshed> Refreshed = new();
    public readonly Subject<BuffEnded> Ended = new();


    void Update()
    {
        float dt = Time.deltaTime;
        foreach (var kv in mBuffs)
        {
            kv.Value.remain -= dt;
            if (kv.Value.remain <= 0)
            {
                mRecycle.Add(kv.Key);   // 삭제 후보
            }
        }

        foreach (int id in mRecycle) 
        {
            mBuffs.Remove(id);
            Ended.OnNext(new BuffEnded(id));
        }
        mRecycle.Clear();
    }

    public void AddBuff(int id, float dur, bool isDebuff = false)
    {
        if (mBuffs.TryGetValue(id, out var s))
        {
            s.dur = dur;
            s.remain = dur;
            Refreshed.OnNext(new BuffRefreshed(id, dur));
        }
        else
        {
            mBuffs[id] = new() { remain = dur, dur = dur };
            Added.OnNext(new BuffAdded(id, dur, isDebuff));
        }
    }

    public void RemoveBuff(int id)
    {
        if (mBuffs.TryGetValue(id, out var s))
        {
            mBuffs.Remove(id);
            Ended.OnNext(new BuffEnded(id));
        }
    }
}
