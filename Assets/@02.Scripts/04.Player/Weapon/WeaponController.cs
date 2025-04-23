using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour, IObservable<GameObject>
{
    [Header("Attack Settings")]
    [SerializeField] private int mWeaponPower = 10;
    [SerializeField] private float mAttackDistance = 1.5f;
    [SerializeField] private Vector3 mAttackBoxSize = new Vector3(2.0f, 2.0f, 1.0f); // 박스 크기
    [SerializeField] private LayerMask mTargetLayerMask;
    [SerializeField] private float mAttackArcHeight = 2.0f;

    private PlayerController mPlayerController;
    private List<IObserver<GameObject>> mObservers = new List<IObserver<GameObject>>();
    private HashSet<Collider> mHitColliders;
    private bool mbIsAttacking = false;
    private bool mbHitDetected = false;

    private void Start()
    {
        mHitColliders = new HashSet<Collider>();
    }

    private void FixedUpdate()
    {
        if (mbIsAttacking)
        {
            Vector3 attackCenter;
            Quaternion attackRotation;
            CalculateAttackPositionAndRotation(out attackCenter, out attackRotation);
            
            // 박스 내 적 감지
            Collider[] hits = Physics.OverlapBox(attackCenter, mAttackBoxSize / 2, attackRotation, 
                mTargetLayerMask, QueryTriggerInteraction.UseGlobal);

            mbHitDetected = hits.Length > 0;
            
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
        else
        {
            mbIsAttacking = false;
        }
    }
    
    // 공중 공격 위치 & 회전 계산 (반원 궤적 + 박스 기울기)
    private void CalculateAttackPositionAndRotation(out Vector3 position, out Quaternion rotation)
    {
        // 공중: 카메라 각도 기반 반원 궤적 + 회전
        Transform cameraTransform = Camera.main.transform;
        
        // 1. 카메라의 피치 각도 추출 (-90°~90°)
        float pitchAngle = Vector3.SignedAngle(
            Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up),
            cameraTransform.forward,
            cameraTransform.right
        );
        
        // 2. 반원 각도로 정규화 (0°~180°)
        if (mPlayerController.IsGrounded)
        {
            pitchAngle = Mathf.Clamp(pitchAngle, -90.0f, 0.0f);
        }
        float normalizedAngle = Mathf.Clamp(pitchAngle + 90.0f, 0.0f, 180.0f);
        float rad = normalizedAngle * Mathf.Deg2Rad;
        
        // 3. 반원 좌표 계산
        float x = Mathf.Sin(rad) * mAttackDistance;
        float y = Mathf.Cos(rad) * mAttackArcHeight;
        Vector3 localOffset = new Vector3(0.0f, y, x); // Z축으로 전방 이동

        // 4. 월드 좌표 변환
        position = mPlayerController.GetComponent<CharacterController>().bounds.center + 
                   mPlayerController.transform.TransformDirection(localOffset);

        // 5. 박스 회전 계산 (캐릭터 전방 + 카메라 피치 반영)
        Quaternion yawRotation = Quaternion.LookRotation(
            mPlayerController.transform.forward,
            Vector3.up
        );
        Quaternion pitchRotation = Quaternion.Euler(pitchAngle, 0.0f, 0.0f);
        rotation = yawRotation * pitchRotation;
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

        Vector3 attackPos;
        Quaternion attackRot;
        CalculateAttackPositionAndRotation(out attackPos, out attackRot);

        Gizmos.color = mbHitDetected ? Color.red : Color.green;
        Gizmos.matrix = Matrix4x4.TRS(attackPos, attackRot, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, mAttackBoxSize);
        Gizmos.matrix = Matrix4x4.identity;
    }
#endif
}