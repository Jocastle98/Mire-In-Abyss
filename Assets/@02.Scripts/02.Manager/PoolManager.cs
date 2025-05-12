using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ResourceManager를 통해 사용되는 풀링 매니저
/// 직접적으로 사용할 필요는 없고 ResourceManager에 의해 자동으로 풀링 관리됨
/// 풀링으로 관리하고자 하는 게임 오브젝트에 Poolable 스크립트를 붙여주면 됨
/// </summary>
public class PoolManager
{
    #region Pool

    private class Pool
    {
        public GameObject Original { get; private set; }
        public Transform Root { get; set; }
        
        private Stack<Poolable> _poolStack = new Stack<Poolable>();

        public void Init(GameObject original, int count = 5)
        {
            Original = original;
            Root = new GameObject($"{original.name}_Root").transform;

            for (int i = 0; i < count; i++)
            {
                Push(Create());
            }
        }
        
        Poolable Create()
        {
            GameObject go = Object.Instantiate<GameObject>(Original);
            go.name = Original.name;

            Poolable poolable = go.GetComponent<Poolable>();
            if (poolable == null)
            {
                poolable = go.AddComponent<Poolable>();
            }
            
            return poolable;
        }

        public void Push(Poolable poolable)
        {
            if (poolable == null)
            {
                return;
            }
            
            poolable.transform.SetParent(Root);
            poolable.gameObject.SetActive(false);
            poolable.IsUsing = false;
            
            _poolStack.Push(poolable);
        }

        public Poolable Pop(Transform parent)
        {
            Poolable poolable;
            if (_poolStack.Count > 0)
            {
                poolable = _poolStack.Pop();
            }
            else
            {
                poolable = Create();
            }
            
            poolable.gameObject.SetActive(true);

            // DontDestroyOnLoad 해제용도 -> 안하면 DontDestroyOnLoad로 생성됨
            /*if (parent == null)
            {
                                             // 현재 씬의 트랜스폼을 부모관계로 설정해줘야 함
                poolable.transform.SetParent(GameManager.Scene.CurrentScene.transform);
            }*/
            
            poolable.transform.SetParent(parent);
            poolable.IsUsing = true;
            
            return poolable;
        }
    }

    #endregion
    
    private Dictionary<string, Pool> pools = new Dictionary<string, Pool>();
    private Transform _root;
    
    public void Init()
    {
        if (_root == null)
        {
            _root = new GameObject("@Pool_Root").transform;
            Object.DontDestroyOnLoad(_root);
        }
    }

    public void CreatePool(GameObject original, int count = 5)
    {
        Pool pool = new Pool();
        pool.Init(original, count);
        pool.Root.SetParent(_root);
        
        pools.Add(original.name, pool);
    }

    public void Push(Poolable poolable)
    {
        string name = poolable.name;
        if (pools.ContainsKey(name) == false)
        {
            GameObject.Destroy(poolable.gameObject);
            return;
        }
        
        pools[name].Push(poolable);
    }

    public Poolable Pop(GameObject original, Transform parent = null)
    {
        if (pools.ContainsKey(original.name) == false)
        {
            CreatePool(original);
        }
        
        return pools[original.name].Pop(parent);
    }

    public GameObject GetOriginal(string poolName)
    {
        if (pools.ContainsKey(poolName) == false)
        {
            return null;
        }
        
        return pools[poolName].Original;
    }

    public void Clear()
    {
        foreach (Transform child in _root)
        {
            GameObject.Destroy(child.gameObject);
        }
        
        pools.Clear();
    }
}