using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager
{
    public T Load<T>(string path) where T : Object
    {
        if (typeof(T) == typeof(GameObject))
        {
            string name = path;
            int index = name.LastIndexOf('/');
            if (index >= 0)
            {
                name = name.Substring(index + 1);
            }

            GameObject go = GameManager.Instance.Pool.GetOriginal(name);
            if (go != null)
            {
                return go as T;
            }
        }
        
        return Resources.Load<T>(path);
    }
    
    public GameObject Instantiate(string path, Transform parent = null)
    {
        // 필요하면 더 범용성있게 수정 가능(현재는 플레이어 이펙트만)
        GameObject original = Load<GameObject>($"Player/Effects/{path}");

        if (original == null)
        {
            Debug.LogError($"Can't find parent of {path}");
            return null;
        }
        
        // 만약에 풀링이 필요한 객체(Poolable 컴포넌트가 있는 객체)라면 풀링 매니저를 통해 객체를 가져옴
        if (original.GetComponent<Poolable>() != null)
        {
            return GameManager.Instance.Pool.Pop(original, parent).gameObject;
        }
        
        GameObject go = Object.Instantiate(original, parent);
        go.name = original.name;
        
        return go;
    }
    
    public void Destroy(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }

        // 만약에 풀링이 필요한 객체(Poolable 컴포넌트가 있는 객체)라면 풀링 매니저에 위탁
        Poolable poolable = obj.GetComponent<Poolable>();
        if (poolable != null)
        {
            GameManager.Instance.Pool.Push(poolable);
        }
        else
        {
            Object.Destroy(obj);
        }
    }
}
