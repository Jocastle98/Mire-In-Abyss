using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : MonoBehaviour
{
    private int mDamage;
    private float mDamageMultiplier;
    private float mHitRadius = 1.5f;
    private LayerMask mEnemyLayer;
    
    private HashSet<GameObject> mHitEnemies = new HashSet<GameObject>();
    
    public void Init(int damage, float damageMultiplier, float hitRadius, LayerMask enemyLayer)
    {
        mDamage = damage;
        mDamageMultiplier = damageMultiplier;
        mHitRadius = hitRadius;
        mEnemyLayer = enemyLayer;
        
        mHitEnemies.Clear();
    }
    
    private void FixedUpdate()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, mHitRadius, mEnemyLayer);
        foreach (var hit in hits)
        {
            if (!mHitEnemies.Contains(hit.gameObject))
            {
                mHitEnemies.Add(hit.gameObject);
                var enemy = hit.GetComponent<EnemyBTController>();
                if (enemy != null)
                {
                    enemy.SetHit((int)(mDamage * mDamageMultiplier), -1);
                    // 흡혈 추가 처리 등도 여기에
                }
            }
        }
    }
}