using System.Collections.Generic;
using Events.Player.Modules;
using R3;
using UnityEngine;

public sealed class Inventory : MonoBehaviour
{
    readonly Dictionary<int, int> mItems = new(); // ID, amount

    public IReadOnlyDictionary<int, int> Items => mItems;
    public int Gold { get; private set; }
    public int Soul { get; private set; }

    /* Rx Subject – UI/Log/Network가 구독 */
    public readonly Subject<ItemAdded> ItemAdded = new();
    public readonly Subject<ItemSubtracked> ItemRemoved = new();
    public readonly Subject<GoldAdded> GoldAdded = new();
    public readonly Subject<GoldSubTracked> GoldSubTracked = new();
    public readonly Subject<SoulAdded> SoulAdded = new();
    public readonly Subject<SoulSubTracked> SoulSubTracked = new();

    public void Init(int soul)
    {
        Soul = soul;
        GoldAdded.OnNext(new GoldAdded(0, 0));
        AddSoul(0);
    }

    public void AddItem(int id, int addedAmt = 1)
    {
        if (!mItems.TryGetValue(id, out var cur))
        {
            mItems[id] = addedAmt;
        }
        else
        {
            mItems[id] = cur + addedAmt;
        }

        //UserData Item 해금 여부 업데이트
        UserData.Instance.SetItemUnlock(id);

        ItemAdded.OnNext(new ItemAdded(id, addedAmt, mItems[id]));
    }

    public bool RemoveItem(int id, int removedAmt = 1)
    {
        if (!mItems.TryGetValue(id, out var cur) || cur < removedAmt)
        {
            Debug.LogError("Item is less than 0");
            return false;
        }
        mItems[id] = cur - removedAmt;
        ItemRemoved.OnNext(new ItemSubtracked(id, removedAmt, mItems[id]));
        return true;
    }

    public void AddGold(int addedAmt)
    {
        float multiplier = GameObject.FindObjectOfType<PlayerStats>().GetGoldMultiplier();

        int adjustAmt = Mathf.RoundToInt(addedAmt * multiplier);
        Gold += adjustAmt;
        GoldAdded.OnNext(new GoldAdded(addedAmt, Gold));
    }

    public void SubTrackGold(int removedAmt)
    {
        Gold -= removedAmt;
        if (Gold < 0)
        {
            Debug.LogError("Gold is less than 0");
            Gold = 0;
        }

        GoldSubTracked.OnNext(new GoldSubTracked(removedAmt, Gold));
    }

    public void AddSoul(int addedAmt)
    {
        float multiplier = GameObject.FindObjectOfType<PlayerStats>().GetSoulStoneMultiplier();

        int adjustedAmt = Mathf.RoundToInt(addedAmt * multiplier);
        Soul += adjustedAmt;

        //UserData Soul 업데이트
        UserData.Instance.Soul += adjustedAmt;

        SoulAdded.OnNext(new SoulAdded(addedAmt, Soul));
    }

    public void SubTrackSoul(int removedAmt)
    {
        Soul -= removedAmt;
        if (Soul < 0)
        {
            Debug.LogError("Soul is less than 0");
            Soul = 0;
        }

        //UserData Soul 업데이트
        UserData.Instance.Soul -= removedAmt;

        SoulSubTracked.OnNext(new SoulSubTracked(removedAmt, Soul));
    }

    public void ResetItems()
    {
        mItems.Clear();
    }
}
