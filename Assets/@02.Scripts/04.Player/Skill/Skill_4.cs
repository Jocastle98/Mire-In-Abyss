using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_4 : MonoBehaviour
{
    private int mDamage;
    private float mDamageMultiplier;
    private float mRadius;
    private Vector3 mTargetPoint;
    private GameObject mProjectile_Effect;
    private Action mFireSoundPlay;
    private GameObject mProjectile_Explode;
    private Action mExplodeSoundPlay;
    
    private bool mbHasArrived = false;
    
    private float mSpeed = 20.0f;
    private float mDuration = 1.0f;
    private float mTimer = 0.0f;

    private void FixedUpdate()
    {
        if (!mbHasArrived)
        {
            mTimer = 0.0f;
            
            FallToTarget();
        }
        else
        {
            mTimer += Time.deltaTime;

            if (mTimer >= mDuration)
            {
                GameManager.Instance.Resource.Destroy(mProjectile_Explode);
                GameManager.Instance.Resource.Destroy(gameObject);
            }
        }
    }
    
    public void Init(int damage, float damageMultiplier, float radius, Vector3 targetPoint, Action fireSoundPlay, Action explodeSoundPlay)
    {
        mDamage = damage;
        mDamageMultiplier = damageMultiplier;
        mRadius = radius;
        mTargetPoint = targetPoint;
        mFireSoundPlay = fireSoundPlay;
        mExplodeSoundPlay = explodeSoundPlay;
        
        mbHasArrived = false;
        
        mProjectile_Effect = GameManager.Instance.Resource.Instantiate("Skill_4_Projectile_Effect", 3, transform);
        mFireSoundPlay?.Invoke();
    }

    private void FallToTarget()
    {
        Vector3 direction = (mTargetPoint - transform.position).normalized;
        float distanceThisFrame = mSpeed * Time.deltaTime;

        // 목표 지점까지 거리가 가까워지면 도착 처리
        if (Vector3.Distance(transform.position, mTargetPoint) <= distanceThisFrame)
        {
            GameManager.Instance.Resource.Destroy(mProjectile_Effect);
            mProjectile_Explode = GameManager.Instance.Resource.Instantiate("Skill_4_Projectile_Explode");
            mExplodeSoundPlay?.Invoke();
            
            mProjectile_Explode.transform.position = mTargetPoint;
            mProjectile_Explode.transform.rotation = Quaternion.identity;
            
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
                enemy.SetHit((int)(mDamage * mDamageMultiplier),- 1);
                
                // Todo: 흡혈효과 처리 추가해야함
            }
        }
    }
}