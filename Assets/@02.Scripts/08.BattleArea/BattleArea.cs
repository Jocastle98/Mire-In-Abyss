using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BattleArea : MonoBehaviour
{
    public abstract void BattleAreaInit(GameObject player, int levelDesign);
    
    public abstract void BattleAreaClear();

    public delegate void ClearDelegate();
    public ClearDelegate OnClearBattleArea;
}
