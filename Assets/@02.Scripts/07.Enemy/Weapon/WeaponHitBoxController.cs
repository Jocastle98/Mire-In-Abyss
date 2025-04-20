using UnityEngine;

public class WeaponHitboxController : MonoBehaviour
{
    private Collider _hitCollider;

    void Awake()
    {
        var hitbox = GetComponentInChildren<HitBox>();
        _hitCollider = hitbox.GetComponent<Collider>();
    }

    public void EnableHitbox()
    {
        _hitCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        _hitCollider.enabled = false;
    }
}