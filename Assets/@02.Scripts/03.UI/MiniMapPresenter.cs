using System;
using System.Collections.Generic;
using Events.Abyss;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using R3;
using UIHUDEnums;
using Unity.VisualScripting;
using UnityEngine.Serialization;


public sealed class MiniMapPresenter : HudPresenterBase
{
    [Header("Refs")] [SerializeField] RectTransform mIconLayer; // IconLayer
    [SerializeField] Camera mMiniMapCam; // MiniMapCamera
    [SerializeField] Transform mPlayer; // Player Transform
    [SerializeField] private List<MiniMapIcon> mIconPrefabs;

    private readonly Dictionary<int, MiniMapIcon> mIcons = new(); // instanceID → icon
    private List<ObjectPool<MiniMapIcon>> mIconPools = new();

    private void Awake()
    {
        mIconPools.Add(new(mIconPrefabs[(int)MiniMapIconType.Player], mIconLayer, 1));
        mIconPools.Add(new(mIconPrefabs[(int)MiniMapIconType.Enemy], mIconLayer, 20));
        mIconPools.Add(new(mIconPrefabs[(int)MiniMapIconType.Boss], mIconLayer, 2));
        mIconPools.Add(new(mIconPrefabs[(int)MiniMapIconType.Shop], mIconLayer, 1));
        mIconPools.Add(new(mIconPrefabs[(int)MiniMapIconType.Portal], mIconLayer, 2));
    }

    void OnEnable()
    {
        var playerIcon = mIconPools[(int)MiniMapIconType.Player].Rent();
        mIcons[mPlayer.GetInstanceID()] = playerIcon;

        subscribeEvents();
    }

    private void subscribeEvents()
    {
        R3EventBus.Instance.Receive<EnemySpawned>()
            .Subscribe(e => spawnIcon(e.Transform, MiniMapIconType.Enemy))
            .AddTo(mCD);
        R3EventBus.Instance.Receive<EnemyDied>()
            .Subscribe(e => despawnIcon(e.Transform, MiniMapIconType.Enemy))
            .AddTo(mCD);
        R3EventBus.Instance.Receive<BossSpawned>()
            .Subscribe(e => spawnIcon(e.Transform, MiniMapIconType.Boss))
            .AddTo(mCD);
        R3EventBus.Instance.Receive<BossDied>()
            .Subscribe(e => despawnIcon(e.Transform, MiniMapIconType.Boss))
            .AddTo(mCD);
        R3EventBus.Instance.Receive<ShopSpawned>()
            .Subscribe(e => spawnIcon(e.Transform, MiniMapIconType.Shop))
            .AddTo(mCD);
        R3EventBus.Instance.Receive<ShopClosed>()
            .Subscribe(e => despawnIcon(e.Transform, MiniMapIconType.Shop))
            .AddTo(mCD);
        R3EventBus.Instance.Receive<PortalSpawned>()
            .Subscribe(e => spawnIcon(e.Transform, MiniMapIconType.Portal))
            .AddTo(mCD);
        R3EventBus.Instance.Receive<PortalClosed>()
            .Subscribe(e => despawnIcon(e.Transform, MiniMapIconType.Portal))
            .AddTo(mCD);
    }

    void LateUpdate()
    {
        foreach (var pair in mIcons)
        {
            Transform target = pair.Value.Target; // 커스텀 속성
            if (target == null)
            {
                continue;
            }

            // 1) 위치 변환
            var worldPos = target.position;
            Vector3 minimapLocalpos = mMiniMapCam.transform.InverseTransformPoint(worldPos);

            if (isOutOfMinimap(minimapLocalpos))
            {
                pair.Value.gameObject.SetActive(false);
            }
            else
            {
                pair.Value.Rect.anchoredPosition = minimapLocalpos;
                pair.Value.gameObject.SetActive(true);
            }
        }
    }

    private bool isOutOfMinimap(Vector3 minimapPos)
    {
        float half = mMiniMapCam.orthographicSize;
        minimapPos.x = Mathf.Clamp(minimapPos.x, -half, half);
        minimapPos.z = Mathf.Clamp(minimapPos.z, -half, half);
        return Mathf.Abs(minimapPos.x) > half || Mathf.Abs(minimapPos.z) > half;
    }

    private void spawnIcon(Transform target, MiniMapIconType iconType)
    {
        var icon = mIconPools[(int)iconType].Rent();
        icon.Init(target);
        mIcons[target.GetInstanceID()] = icon;
    }

    private void despawnIcon(Transform target, MiniMapIconType iconType)
    {
        if (mIcons.TryGetValue(target.GetInstanceID(), out var icon))
        {
            icon.ResetIcon();
            mIconPools[(int)iconType].Return(icon);
            mIcons.Remove(target.GetInstanceID());
        }
    }

    protected override void OnDisable()
    {
        mCD.Dispose();
        foreach (var ico in mIcons.Values)
        {
            ico.ResetIcon();
        }

        mIcons.Clear();
    }
}