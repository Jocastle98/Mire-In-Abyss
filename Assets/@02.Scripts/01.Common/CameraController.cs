using Cinemachine;
using UnityEngine;

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

    // 공중 시야 제한 완화(90로도 하면 공격감지 콜라이더에 문제 생김)
    private float mAirTopClamp = 89f;
    private float mAirBottomClamp = -89f;

    // cinemachine
    private float mCinemachineTargetYaw;
    private float mCinemachineTargetPitch;
    private bool mbIsGround;

    // 디버깅용
    bool mCursorShown;


    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        mCinemachineCameraTarget = GetComponent<CinemachineVirtualCamera>().Follow.gameObject;
        mCinemachineTargetYaw = mCinemachineCameraTarget.transform.rotation.eulerAngles.y;
    }

    private void LateUpdate()
    {
        // 디버깅용
        if (Input.GetKeyDown(KeyCode.BackQuote))    // ` 키 (1 왼쪽에 있는 거)
        {
            toggleCursor(!mCursorShown);
        }


        CameraRotation();
    }

    // 디버깅용
    void toggleCursor(bool show)
    {
        mCursorShown = show;

        if (show)
        {
            Cursor.lockState = CursorLockMode.None;  // 1) 먼저 락 해제
            Cursor.visible = true;                 // 2) 그다음 보이게
        }
        else
        {
            Cursor.visible = false;                // 1) 먼저 숨김
            Cursor.lockState = CursorLockMode.Locked;// 2) 그다음 잠금
        }
    }


    public void SendPlayerGrounded(bool isGround)
    {
        mbIsGround = isGround;
    }

    private void CameraRotation()
    {
        if (GameManager.Instance.Input.LookInput.sqrMagnitude >= mThreshold)
        {
            mCinemachineTargetYaw += GameManager.Instance.Input.LookInput.x * mRotationSensitivity;
            mCinemachineTargetPitch -= GameManager.Instance.Input.LookInput.y * mRotationSensitivity;
        }

        mCinemachineTargetYaw = ClampAngle(mCinemachineTargetYaw, float.MinValue, float.MaxValue);
        if (mbIsGround)
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
}