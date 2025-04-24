using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Top-down minimap camera controller.
/// • Keeps a fixed height above the target
/// • Optional rotation to match target yaw
/// • Works with both Built-in RP and URP
/// Attach this to the MiniMapCamera GameObject.
/// </summary>
[RequireComponent(typeof(Camera))]
public sealed class MiniMapCameraController : MonoBehaviour
{
    [FormerlySerializedAs("target")]
    [Header("Target")]
    [Tooltip("Player (or object) the minimap follows")]
    public Transform Target;

    [FormerlySerializedAs("height")]
    [Header("Position")]
    [Tooltip("Height of the camera above the target (world units)")]
    public float Height = 30f;

    [FormerlySerializedAs("rotateWithTarget")]
    [Header("Rotation")]
    [Tooltip("If true, minimap rotates with the target's Y-axis")]
    public bool RotateWithTarget = false;

    [FormerlySerializedAs("orthoSize")]
    [Header("Zoom")]
    [Tooltip("Orthographic size (half of vertical world units shown)")]
    public float OrthoSize = 20f;

    Camera mCam;

    void Awake()
    {
        mCam = GetComponent<Camera>();
        mCam.orthographic = true;          // minimap = top-down ortho
        mCam.orthographicSize = OrthoSize;
        mCam.clearFlags = CameraClearFlags.SolidColor;
        mCam.backgroundColor = new Color(0, 0, 0, 0); // 완전 투명
    }

    void LateUpdate()
    {
        if (Target == null) return;

        // ─── 1. 위치 추적 ───
        Vector3 tPos = Target.position;
        transform.position = new Vector3(tPos.x, tPos.y + Height, tPos.z);

        // ─── 2. 회전 설정 ───
        float yRot = RotateWithTarget ? Target.eulerAngles.y : 0f;
        transform.rotation = Quaternion.Euler(90f, yRot, 0f);
    }

#if UNITY_EDITOR
    // 인스펙터에서 값 바꾸면 바로 적용되도록
    void OnValidate()
    {
        if (mCam == null)
        {
            mCam = GetComponent<Camera>();
        }
        mCam.orthographic = true;
        mCam.orthographicSize = OrthoSize;
    }
#endif
}