using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_1 : MonoBehaviour
{
    private int mDamage;
    private float mDamageMultiplier;
    private float mDistance;
    private Vector3 mDirection;
    private Action mFireSoundPlay;
    
    private float mSpeed = 20.0f;
    private float mDistanceTraveled;

    private void FixedUpdate()
    {
        MoveEffect();
    }
    
    public void Init(int damage, float damageMultiplier, float distance, Vector3 direction, Action fireSoundPlay)
    {
        mDamage = damage;
        mDamageMultiplier = damageMultiplier;
        mDistance = distance;
        mDirection = direction;
        mFireSoundPlay = fireSoundPlay;
        
        mDistanceTraveled = 0.0f;
        mFireSoundPlay?.Invoke();
    }

    private void MoveEffect()
    {
        float moveDistance = mSpeed * Time.deltaTime;
        transform.Translate(mDirection * moveDistance, Space.World);
        
        mDistanceTraveled += moveDistance;

        if (mDistanceTraveled >= mDistance)
        {
            GameManager.Instance.Resource.Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            var enemy = other.gameObject.GetComponent<EnemyBTController>();
            if (enemy != null)
            {
                enemy.SetHit((int)(mDamage * mDamageMultiplier),- 1);
                
                // Todo: 흡혈효과 처리 추가해야함
            }
        }
    }
}