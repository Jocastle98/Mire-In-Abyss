using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ObjectPool<T> where T : Component
{
    readonly T          mPrefab;
    readonly Transform  mParent;
    readonly Stack<T>   mStack = new();

    public ObjectPool(T prefab, Transform parent, int prewarm = 0)
    {
        mPrefab = prefab;
        mParent = parent;
        for (int i = 0; i < prewarm; i++)
        {
            mStack.Push(createInstance());
        }
    }

    T createInstance()
    {
        var obj = UnityEngine.Object.Instantiate(mPrefab, mParent);
        obj.gameObject.SetActive(false);
        return obj;
    }

    public T Rent()
    {
        var inst = mStack.Count > 0 ? mStack.Pop() : createInstance();
        inst.gameObject.SetActive(true);
        return inst;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        mStack.Push(obj);
    }

    public void Clear()
    {
        foreach (var o in mStack)
        {
            UnityEngine.Object.Destroy(o.gameObject);
            
        }
        mStack.Clear();
    }
}