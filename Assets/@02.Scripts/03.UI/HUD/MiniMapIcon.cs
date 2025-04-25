using UnityEngine;

public sealed class MiniMapIcon : MonoBehaviour
{
    public RectTransform Rect { get; private set; }
    public Transform Target { get; private set; }

    void Awake() => Rect = (RectTransform)transform;
    public void Init(Transform target)
    {
        Target  = target;
    }
    
    public void ResetIcon() => Target = null;   // 풀 반납 전 호출
}
