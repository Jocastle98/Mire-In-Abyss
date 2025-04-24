using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(CinemachineBrain))]
[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private float mRotationSensitivity = 1.0f;
    [SerializeField] private GameObject mCinemachineCameraTarget;
    [SerializeField] private float mTopClamp = 70.0f;
    [SerializeField] private float mBottomClamp = -30.0f;
    [SerializeField] private float mCameraAngleOverride = 0.0f;
    [SerializeField] private const float mThreshold = 0.01f;
    
    // cinemachine
    private float mCinemachineTargetYaw;
    private float mCinemachineTargetPitch;

    private void Awake()
    {
        /*Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;*/
        
        mCinemachineCameraTarget = GetComponent<CinemachineVirtualCamera>().Follow.gameObject;
        mCinemachineTargetYaw = mCinemachineCameraTarget.transform.rotation.eulerAngles.y;
    }
    
    private void LateUpdate()
    {
        //SetCursor();
        CameraRotation();
    }

    private void SetCursor()
    {
        if (GameManager.Instance.Input.CursorToggleInput)
        {
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
    
    private void CameraRotation()
    {
        if (GameManager.Instance.Input.LookInput.sqrMagnitude >= mThreshold)
        {
            mCinemachineTargetYaw += GameManager.Instance.Input.LookInput.x * mRotationSensitivity;
            mCinemachineTargetPitch -= GameManager.Instance.Input.LookInput.y * mRotationSensitivity;
        }
    
        mCinemachineTargetYaw = ClampAngle(mCinemachineTargetYaw, float.MinValue, float.MaxValue);
        mCinemachineTargetPitch = ClampAngle(mCinemachineTargetPitch, mBottomClamp, mTopClamp);
        
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