using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float mRotationSpeed = 100.0f;
    [SerializeField] private float mDistance = 5.0f;
    [SerializeField] private LayerMask mObstacleLayerMask;
    
    private Transform mTarget;
    
    private float mAzimuthAngle;
    private float mPolarAngle = 45.0f;

    private void Start()
    {
        // 커서 설정
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    private void LateUpdate()
    {
        Vector2 lookInput = GameManager.Instance.Input.LookInput;
        
        mAzimuthAngle += lookInput.x * mRotationSpeed * Time.deltaTime;
        mPolarAngle -= lookInput.y * mRotationSpeed * Time.deltaTime;
        mPolarAngle = Mathf.Clamp(mPolarAngle, 10.0f, 45.0f);
        
        // 벽 감지 처리
        var currentDistance = AdjustCameraDistance();
        
        // 구면좌표계 -> _polarAngle 수직 회전 각도, _azimuthAngle 수평 회전 각도
        var cartesianPosition = GetCameraPosition(currentDistance, mPolarAngle, mAzimuthAngle);
        var cameraPosition = mTarget.position - cartesianPosition;
        
        transform.position = cameraPosition;
        transform.LookAt(mTarget);
    }
    
    private Vector3 GetCameraPosition(float r, float polarAngle, float azimuthAngle)
    {
        float b = r * Mathf.Cos(polarAngle * Mathf.Deg2Rad);
        float z = b * Mathf.Cos(azimuthAngle * Mathf.Deg2Rad);
        float y = r * - Mathf.Sin(polarAngle * Mathf.Deg2Rad);
        float x = b * Mathf.Sin(azimuthAngle * Mathf.Deg2Rad);
        
        return new Vector3(x, y, z);
    }
    
    // 카메라와 타겟 사이에 장애물이 있을 때 카메라와 타겟간의 거리를 조절하는 메서드
    private float AdjustCameraDistance()
    {
        var currentDistance = mDistance;
        
        // 타켓에서 카메라 방향으로 레이저 발사
        Vector3 direction = GetCameraPosition(1.0f, mPolarAngle, mAzimuthAngle).normalized;
        RaycastHit hit;
        
        // 타겟에서 카메라 예정 위치까지 레이케스트 발사
        if (Physics.Raycast(mTarget.position, -direction, out hit, mDistance, mObstacleLayerMask))
        {
            float offset = 0.3f;
            currentDistance = hit.distance - offset;
            currentDistance = Mathf.Max(currentDistance, 0.5f);
        }
        return currentDistance;
    }
    
    public void SetTarget(Transform target)
    {
        mTarget = target;
        
        var cartesianPosition = GetCameraPosition(mDistance, mPolarAngle, mAzimuthAngle);
        var cameraPosition = mTarget.position - cartesianPosition;
        
        transform.position = cameraPosition;
        transform.LookAt(mTarget);
    }
}
