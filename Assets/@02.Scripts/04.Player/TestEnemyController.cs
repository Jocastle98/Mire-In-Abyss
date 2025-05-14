using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class TestEnemyController : MonoBehaviour
{
    [SerializeField] private int mMaxHealth = 100;
    [SerializeField] private int mEnemyAttackPower = 5;
    public int EnemyAttackPower => mEnemyAttackPower;
    
    private Animator _animator;
    private Rigidbody _rigidbody;
    private CapsuleCollider _capsuleCollider;
    private int mCurrentHealth;

    [SerializeField] private PlayerStats mPlayerStats;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
        _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }

    private void Start()
    {
        mCurrentHealth = mMaxHealth;
    }

    public void SetHit(PlayerController playerController)
    {
        if (mCurrentHealth <= 0) return;

        Debug.Log("허수아비 히트!");
        _animator.SetTrigger("Hit");

        int playerAttackPower = (int)playerController.GetComponent<PlayerStats>().GetAttackPower();
        mCurrentHealth -= playerAttackPower;
        Debug.Log($"현재 체력: {mCurrentHealth}/{mMaxHealth}");

        if (mCurrentHealth <= 0)
        {
            Debug.Log("허수아비 파괴됨!");
            _animator.SetTrigger("Dead");

            _rigidbody.isKinematic = false;
            _rigidbody.useGravity = true;
            _rigidbody.constraints = RigidbodyConstraints.None;

            _capsuleCollider.isTrigger = false;

            Vector3 direction = (transform.position - playerController.transform.position).normalized;
            direction.y = 0.5f;
            Vector3 force = direction * 5.0f;
            _rigidbody.AddForce(force, ForceMode.Impulse);
        }
    }
}