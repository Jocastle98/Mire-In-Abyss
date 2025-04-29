using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_1 : MonoBehaviour
{
    private int mDamage;
    private float mDamageMultiplier;
    private Vector3 mDirection;
    private float mSpeed = 20.0f;
    private float mDistance;
    private float mDistanceTraveled;

    private void FixedUpdate()
    {
        MoveEffect();
    }
    
    public void Init(int damage, float damageMultiplier, float distance, Vector3 direction)
    {
        mDamage = damage;
        mDamageMultiplier = damageMultiplier;
        mDistance = distance;
        mDirection = direction;
    }

    private void MoveEffect()
    {
        float moveDistance = mSpeed * Time.deltaTime;
        transform.Translate(mDirection * moveDistance, Space.World);
        
        mDistanceTraveled += moveDistance;

        if (mDistanceTraveled >= mDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Debug.Log(other.gameObject.name);
            var enemy = other.gameObject.GetComponent<EnemyBTController>();
            if (enemy != null)
            {
                enemy.SetHit((int)(mDamage * mDamageMultiplier));
            }
        }
    }
}
