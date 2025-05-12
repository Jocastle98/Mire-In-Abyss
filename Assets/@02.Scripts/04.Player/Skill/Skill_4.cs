using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_4 : MonoBehaviour
{
    private int mDamage;
    private float mDamageMultiplier;
    private float mRadius;
    private Vector3 mTargetPoint;
    private float mSpeed = 20.0f;
    private bool mbHasArrived = false;

    private void FixedUpdate()
    {
        if (!mbHasArrived)
        {
            FallToTarget();
        }
    }
    
    public void Init(int damage, float damageMultiplier, float radius, Vector3 targetPoint)
    {
        mDamage = damage;
        mDamageMultiplier = damageMultiplier;
        mRadius = radius;
        mTargetPoint = targetPoint;
    }

    private void FallToTarget()
    {
        Vector3 direction = (mTargetPoint - transform.position).normalized;
        float distanceThisFrame = mSpeed * Time.deltaTime;

        // 목표 지점까지 거리가 가까워지면 도착 처리
        if (Vector3.Distance(transform.position, mTargetPoint) <= distanceThisFrame)
        {
            transform.position = mTargetPoint;
            mbHasArrived = true;

            // 범위 내 적 감지 및 데미지
            DetectAndDamageEnemies();
        }
        else
        {
            transform.position += direction * distanceThisFrame;
        }
    }
    
    private void DetectAndDamageEnemies()
    {
        Collider[] hits = Physics.OverlapSphere(mTargetPoint, mRadius, LayerMask.GetMask("Enemy"));
        foreach (var hit in hits)
        {
            // 예시
            EnemyBTController enemy = hit.GetComponent<EnemyBTController>();
            if (enemy != null)
            {
                enemy.SetHit((int)(mDamage * mDamageMultiplier),1);
                
                // Todo: 흡혈효과 처리 추가해야함
            }
        }

        // 이펙트 추가
        
        Destroy(gameObject, 1.0f);
    }
}
