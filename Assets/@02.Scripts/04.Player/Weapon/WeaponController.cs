using System;
using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class WeaponController : MonoBehaviour, IObservable<GameObject>
{
    [Serializable] public class WeaponTriggerZone
    {
        public Vector3 position;
        public float radius;
    }
    [SerializeField] private WeaponTriggerZone[] mTriggerZones;
    
    public int WeaponAttackPower => mWeaponAttackPower;
    [SerializeField] private int mWeaponAttackPower;
    [SerializeField] private LayerMask mTargetLayerMask;
    
    private List<IObserver<GameObject>> mObservers = new List<IObserver<GameObject>>();

    // 충돌 처리
    private Vector3[] mPreviousPositions;
    private HashSet<Collider> mHitColliders; // 같은 콜라이더가 감지 되도 한 번만 저장됨
    private Ray mRay = new Ray();
    private RaycastHit[] mHits = new RaycastHit[10];
    private bool mbIsAttacking = false;

    private void Start()
    {
        mPreviousPositions = new Vector3[mTriggerZones.Length];
        mHitColliders = new HashSet<Collider>();
    }

    public void AttackStart()
    {
        mbIsAttacking = true;
        mHitColliders.Clear();

        for (int i = 0; i < mTriggerZones.Length; i++)
        {
            mPreviousPositions[i] = transform.position + transform.TransformVector(mTriggerZones[i].position);
        }
    }

    public void AttackEnd()
    {
        mbIsAttacking = false;
    }

    private void FixedUpdate()
    {
        if (mbIsAttacking)
        {
            for (int i = 0; i < mTriggerZones.Length; i++)
            {
                var worldPosition = transform.position + transform.TransformVector(mTriggerZones[i].position);
                var direction = (worldPosition - mPreviousPositions[i]);
                mRay.origin = mPreviousPositions[i];
                mRay.direction = direction;
                
                var hitCount = Physics.SphereCastNonAlloc(mRay, mTriggerZones[i].radius, mHits, 
                    direction.magnitude, mTargetLayerMask, QueryTriggerInteraction.UseGlobal);
                
                for (int j = 0; j < hitCount; j++)
                {
                    var hit = mHits[j];
                    if (!mHitColliders.Contains(hit.collider))
                    {
                        // Time.timeScale = 0.0f;
                        // StartCoroutine(ResumeTimeScale());
                        
                        mHitColliders.Add(hit.collider);
                        Notify(hit.collider.gameObject);
                    }
                }
                mPreviousPositions[i] = worldPosition;
            }
        }
    }

    public void Subscribe(IObserver<GameObject> observer)
    {
        if (!mObservers.Contains(observer))
        {
            mObservers.Add(observer);
        }
    }

    public void Unsubscribe(IObserver<GameObject> observer)
    {
        mObservers.Remove(observer);
    }

    public void Notify(GameObject value)
    {
        foreach (var observer in mObservers)
        {
            observer.OnNext(value);
        }
    }

    private void OnDestroy()
    {
        var capyObservers = new List<IObserver<GameObject>>(mObservers);
        foreach (var observer in capyObservers)
        {
            observer.OnCompleted();
        }
        mObservers.Clear();
    }
    
#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (mbIsAttacking)
        {
            for (int i = 0; i < mTriggerZones.Length; i++)
            {
                var worldPosition = transform.position + transform.TransformVector(mTriggerZones[i].position);
                var direction = (worldPosition - mPreviousPositions[i]);
                
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(worldPosition, mTriggerZones[i].radius);
                
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(worldPosition + direction, mTriggerZones[i].radius);
            }
        }
        else
        {
            foreach (var triggerZone in mTriggerZones)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(triggerZone.position, triggerZone.radius);
            }
        }
    }
    
#endif
}