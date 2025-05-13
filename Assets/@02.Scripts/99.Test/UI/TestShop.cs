using Events.HUD;
using UIHUDEnums;
using UnityEngine;

public class TestShop : MonoBehaviour, IMapTrackable
{
    [SerializeField] private Transform mMapAnchor;

    public Transform MapAnchor => mMapAnchor;

    public MiniMapIconType IconType => MiniMapIconType.Shop;
}