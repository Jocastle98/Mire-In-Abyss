using UIHUDEnums;
using UnityEngine;

public sealed class MiniMapIcon : MonoBehaviour
{
    public RectTransform Rect { get; private set; }
    public Transform Target { get; private set; }
    public MiniMapIconType IconType { get; private set; }

    void Awake() => Rect = (RectTransform)transform;
    public void Init(Transform target, MiniMapIconType iconType)
    {
        Target  = target;
        IconType = iconType;
    }
    
    public void ResetIcon() => Target = null;   // 풀 반납 전 호출
}
