using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDeSpawnTest : MonoBehaviour
{
    public System.Action monsterDead;
    bool test = false;

    private void Update()
    {
        if (!test)
        {
            Invoke(nameof(MonsterGoToTheUSA), 2f);
            test = true;
        }
    }

    void MonsterGoToTheUSA()
    {
        monsterDead.Invoke();
        Destroy(gameObject);
    }
}
