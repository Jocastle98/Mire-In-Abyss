using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class TestEnemyController : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int _currentHealth;

    private Rigidbody _rigidbody;
    private CapsuleCollider _capsuleCollider;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
        _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }

    private void Start()
    {
        _currentHealth = maxHealth;
    }

    public void SetHit(PlayerController playerController)
    {
        if (_currentHealth <= 0) return;

        Debug.Log("허수아비 히트!");

        _currentHealth -= playerController.AttackPower;
        Debug.Log($"현재 체력: {_currentHealth}/{maxHealth}");

        if (_currentHealth <= 0)
        {
            Debug.Log("허수아비 파괴됨!");

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