using UnityEngine;

public class WeaponHitBoxController : MonoBehaviour
{
    private Collider mHitCollider;

    private void Awake()
    {
        var hitbox = GetComponentInChildren<HitBox>();
        mHitCollider = hitbox.GetComponent<Collider>();
    }

    public void EnableHitbox()
    {
        mHitCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        mHitCollider.enabled = false;
    }
}