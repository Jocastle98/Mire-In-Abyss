using Events.HUD;
using UIHUDEnums;
using UnityEngine;

public class TestPortal : MonoBehaviour, IMapTrackable
{
    [SerializeField] private Transform mMapAnchor;

    public Transform MapAnchor => mMapAnchor;

    public MiniMapIconType IconType => MiniMapIconType.Portal;
}