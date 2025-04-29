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
    public Transform Target;
    public float Height = 30f;
    public bool RotateWithTarget = false;
    public float OrthoSize = 20f;

    private Camera mCam;

    void Awake()
    {
        mCam = GetComponent<Camera>();
        mCam.orthographic = true; // minimap = top-down ortho
        mCam.orthographicSize = OrthoSize;
        mCam.clearFlags = CameraClearFlags.SolidColor;
        mCam.backgroundColor = new Color(0, 0, 0, 0);
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
}