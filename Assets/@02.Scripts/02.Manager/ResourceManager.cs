using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 매니저 인스턴스로 사용하면 됨
/// PoolManager와 연결됨 -> Poolable 컴포넌트가 붙은 게임 오브젝트 자동으로 풀링 관리
/// 생성: Instantiate
/// 파괴: Destroy
/// </summary>
public class ResourceManager
{
    private T Load<T>(string path) where T : Object
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
    
    /// <summary>
    /// 게임오브젝트를 생성하는 메서드
    /// Poolable 컴포넌트가 있으면 풀링으로 관리(자동으로 Pop)
    /// Poolable 컴포넌트가 없으면 그냥 생성
    /// 경로 설정 부분만 수정하면 범용성 있게 만들 수 있음
    /// </summary>
    /// <param name="path"> 게임오브젝트 리소스의 경로 </param>
    /// <param name="parent"> 부모오브젝트, 기본값은 null </param>
    /// <returns></returns>
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
    
    /// <summary>
    /// 게임 오브젝트를 비활성화 하거나 파괴하는 메서드
    /// Poolable 컴포넌트가 있으면 풀링으로 관리(자동으로 Push)
    /// Poolable 컴포넌트가 없으면 파괴
    /// </summary>
    /// <param name="obj"> 대상 게임 오브젝트 </param>
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
