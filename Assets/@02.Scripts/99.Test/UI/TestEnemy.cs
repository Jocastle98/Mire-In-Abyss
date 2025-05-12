using Events.HUD;
using UIHUDEnums;
using UnityEngine;

public class TestEnemy : MonoBehaviour, IMapTrackable, IHpTrackable
{
    [SerializeField] private Transform mMapAnchor;
    [SerializeField] private Transform mHpAnchor;

    public Transform MapAnchor => mMapAnchor;
    public Transform HpAnchor => mHpAnchor;

    public MiniMapIconType IconType => MiniMapIconType.Enemy;
}