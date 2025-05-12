using System;
using Cinemachine;
using Events.UI;
using UnityEngine;
using R3;
using Events.Player;
using GameEnums;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(CinemachineBrain))]
[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private float mRotationSensitivity = 1.0f;
    [SerializeField] private GameObject mCinemachineCameraTarget;
    [SerializeField] private float mCameraAngleOverride = 0.0f;
    [SerializeField] private float mThreshold = 0.01f;
    [SerializeField] private float mGroundedTopClamp = 70.0f;
    [SerializeField] private float mGroundedBottomClamp = -30.0f;

    private bool mbAcceptInput = false;

    // 공중 시야 제한 완화(90로도 하면 공격감지 콜라이더에 문제 생김)
    private float mAirTopClamp = 89f;
    private float mAirBottomClamp = -89f;

    // cinemachine
    private float mCinemachineTargetYaw;
    private float mCinemachineTargetPitch;
    private bool mbPlayerIsOnGround;

    private void Awake()
    {
        mCinemachineCameraTarget = GetComponent<CinemachineVirtualCamera>().Follow.gameObject;
        mCinemachineTargetYaw = mCinemachineCameraTarget.transform.rotation.eulerAngles.y;
    }

    private void Start()
    {
        eventsubscribe();
    }

    void eventsubscribe()
    {
        GameManager.Instance.ObserveState
        .Subscribe(s => mbAcceptInput = (s == GameState.Gameplay))
        .AddTo(this);

        R3EventBus.Instance.Receive<PlayerGrounded>()
            .Subscribe(e => mbPlayerIsOnGround = e.IsGrounded)
            .AddTo(this);
    }

    private void LateUpdate()
    {
        CameraRotation();
    }
    
    private void CameraRotation()
    {
        // if (!mbAcceptInput) 
        // {
        //     return;
        // }
        
        if (GameManager.Instance.Input.LookInput.sqrMagnitude >= mThreshold)
        {
            mCinemachineTargetYaw += GameManager.Instance.Input.LookInput.x * mRotationSensitivity;
            mCinemachineTargetPitch -= GameManager.Instance.Input.LookInput.y * mRotationSensitivity;
        }

        mCinemachineTargetYaw = ClampAngle(mCinemachineTargetYaw, float.MinValue, float.MaxValue);
        if (mbPlayerIsOnGround)
        {
            mCinemachineTargetPitch = ClampAngle(mCinemachineTargetPitch, mGroundedBottomClamp, mGroundedTopClamp);
        }
        else
        {
            mCinemachineTargetPitch = ClampAngle(mCinemachineTargetPitch, mAirBottomClamp, mAirTopClamp);
        }

        mCinemachineCameraTarget.transform.rotation = Quaternion.Euler(mCinemachineTargetPitch + mCameraAngleOverride,
            mCinemachineTargetYaw, 0.0f);
    }

    private static float ClampAngle(float localFloatAngle, float localFloatMin, float localFloatMax)
    {
        if (localFloatAngle < -360f) localFloatAngle += 360f;
        if (localFloatAngle > 360f) localFloatAngle -= 360f;
        return Mathf.Clamp(localFloatAngle, localFloatMin, localFloatMax);
    }

    [Obsolete("이 함수는 곧 R3EventBus의 이벤트 처리로 대체할 예정입니다.")]
    public void SendPlayerGrounded(bool isGround)
    {
        mbPlayerIsOnGround = isGround;
    }
}