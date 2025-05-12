using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbyssManagerSceneTest : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("Player", System.StringComparison.OrdinalIgnoreCase))
        {
            
            AbyssManager.Instance.BattleAreaManagerInit(other.gameObject,1,3);
        }
    }
}
