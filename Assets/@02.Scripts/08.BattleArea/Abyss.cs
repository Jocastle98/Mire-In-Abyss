using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public abstract class Abyss : MonoBehaviour
{
    public abstract void SetPortal();
    public abstract void BattleAreaClear();

    public delegate void ClearDelegate();
    public ClearDelegate OnClearBattleArea;

    public GameObject portal;

    public void DeActivatePortal()
    {
        portal.SetActive(false);
    }
    
    public void ActivatePortal(GameObject portal,GameObject setPos)
    {
        portal.SetActive(true);
        portal.transform.position = setPos.transform.position;
    }
    
    /// <summary>
    /// 해당 오브젝트의 자식들을 파괴
    /// </summary>
    /// <param name="obj"></param>
    public void ClearObjs(GameObject obj)
    {
        foreach (Transform child in obj.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
