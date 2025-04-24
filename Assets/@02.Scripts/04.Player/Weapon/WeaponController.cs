using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour, IObservable<GameObject>
{
    [Header("Attack Settings")]
    [SerializeField] private float mAttackDistance = 1.5f;
    [SerializeField] private Vector3 mAttackBoxSize = new Vector3(2.0f, 2.0f, 1.0f); // 박스 크기
    [SerializeField] private int mWeaponPower = 10;
    [SerializeField] private LayerMask mTargetLayerMask;

    private PlayerController mPlayerController;
    private List<IObserver<GameObject>> mObservers = new List<IObserver<GameObject>>();
    private HashSet<Collider> mHitColliders;
    private bool mbIsAttacking = false;

    private void Start()
    {
        mHitColliders = new HashSet<Collider>();
    }

    private void FixedUpdate()
    {
        if (mbIsAttacking)
        {
            Vector3 attackCenter = mPlayerController.GetComponent<CharacterController>().bounds.center
                                   + mPlayerController.transform.forward * mAttackDistance;
        
            // 박스 내 적 감지
            Collider[] hits = Physics.OverlapBox(attackCenter, mAttackBoxSize / 2, transform.rotation, 
                mTargetLayerMask, QueryTriggerInteraction.UseGlobal);

            // 감지된 적 처리
            foreach (var hit in hits)
            {
                if (!mHitColliders.Contains(hit))
                {
                    mHitColliders.Add(hit);
                    Notify(hit.gameObject);
                }
            }
        }
    }

    public void SetPlayer(PlayerController playerController)
    {
        mPlayerController = playerController;
    }

    public int GetWeaponPower()
    {
        return mWeaponPower;
    }
    
    public void AttackStart()
    {
        mbIsAttacking = true;
        mHitColliders.Clear();
    }
    
    public void AttackEnd()
    {
        mbIsAttacking = false;
    }
    
    // 옵저버 패턴 구현
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
        if (mPlayerController == null)
        {
            return;
        }
        
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Vector3 center = mPlayerController.GetComponent<CharacterController>().bounds.center
                         + mPlayerController.transform.forward * mAttackDistance;
        Gizmos.matrix = Matrix4x4.TRS(center, mPlayerController.transform.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, mAttackBoxSize);
        Gizmos.matrix = Matrix4x4.identity;
    }
#endif
}