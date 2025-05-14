using System;
using System.Collections;
using System.Collections.Generic;
using Events.HUD;
using UIHUDEnums;
using UnityEngine;

public class AbyssMoveController : MonoBehaviour, IMapTrackable
{
    public delegate void BattleAreaMoveDelegate();
    public BattleAreaMoveDelegate battleAreaMoveDelegate;

    public Transform MapAnchor => transform;

    public MiniMapIconType IconType => MiniMapIconType.Portal;

    //TODO: 포탈 드러나는 부분에 연결 시 삭제
    void Awake()
    {
        OpenPortal();
    }

    public void OpenPortal()
    {
        TrackableEventHelper.PublishSpawned(this);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("Player", System.StringComparison.OrdinalIgnoreCase))
        {
            battleAreaMoveDelegate.Invoke();
        }
    }
}
