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
    [Header("Refs")] 
    [SerializeField] RectTransform mOtherIconLayer;
    [SerializeField] RectTransform mPlayerIconLayer;
    [SerializeField] Camera mMiniMapCam;
    [SerializeField] Transform mPlayer;
    [SerializeField] private List<MiniMapIcon> mIconPrefabs;

    private Camera mMainCam;
    private RectTransform mPlayerIconRT;
    private readonly Dictionary<int, MiniMapIcon> mIconsMap = new(); // instanceID → icon
    private List<ObjectPool<MiniMapIcon>> mIconPools = new();
    private float mWorldToUIScale;

    private void Awake()
    {
        mIconPools.Add(new(mIconPrefabs[(int)MiniMapIconType.Player], mPlayerIconLayer, 1));
        mIconPools.Add(new(mIconPrefabs[(int)MiniMapIconType.Enemy], mOtherIconLayer, 20));
        mIconPools.Add(new(mIconPrefabs[(int)MiniMapIconType.Boss], mOtherIconLayer, 2));
        mIconPools.Add(new(mIconPrefabs[(int)MiniMapIconType.Shop], mOtherIconLayer, 1));
        mIconPools.Add(new(mIconPrefabs[(int)MiniMapIconType.Portal], mOtherIconLayer, 2));
    }

    void OnEnable()
    {
        var playerIcon = mIconPools[(int)MiniMapIconType.Player].Rent();
        mIconsMap[mPlayer.GetInstanceID()] = playerIcon;
        
        // 플레이어 아이콘 회전을 위한 할당
        mPlayerIconRT = playerIcon.GetComponent<RectTransform>();
        mMainCam = Camera.main;

    }

    private void Start()
    {
        subscribeEvents();

        mWorldToUIScale = mOtherIconLayer.rect.width / (mMiniMapCam.orthographicSize * 2); // 월드 → UI 스케일
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
        // 플레이어 아이콘 방향 업데이트
        var playerIconRotation = mPlayerIconRT.rotation.eulerAngles;
        playerIconRotation.z = -mMainCam.transform.rotation.eulerAngles.y;
        mPlayerIconRT.rotation = Quaternion.Euler(playerIconRotation);
        
        // 아이콘 업데이트
        foreach (var pair in mIconsMap)
        {
            Transform target = pair.Value.Target; // 커스텀 속성
            if (target == null)
            {
                continue;
            }

            // 1) 위치 변환
            var worldPos = target.position;
            Vector3 minimapCamPos = mMiniMapCam.transform.InverseTransformPoint(worldPos);

            if (isOutOfMinimap(minimapCamPos))
            {
                pair.Value.gameObject.SetActive(false);
            }
            else
            {
                pair.Value.Rect.anchoredPosition = minimapCamPos* mWorldToUIScale;
                pair.Value.gameObject.SetActive(true);
            }
        }
    }

    private bool isOutOfMinimap(Vector3 minimapCamPos)
    {
        float half = mMiniMapCam.orthographicSize;
        
        return Mathf.Abs(minimapCamPos.x) > half || Mathf.Abs(minimapCamPos.y) > half;
    }

    private void spawnIcon(Transform target, MiniMapIconType iconType)
    {
        var icon = mIconPools[(int)iconType].Rent();
        icon.Init(target);
        mIconsMap[target.GetInstanceID()] = icon;
    }

    private void despawnIcon(Transform target, MiniMapIconType iconType)
    {
        if (mIconsMap.TryGetValue(target.GetInstanceID(), out var icon))
        {
            icon.ResetIcon();
            mIconPools[(int)iconType].Return(icon);
            mIconsMap.Remove(target.GetInstanceID());
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        foreach (var ico in mIconsMap.Values)
        {
            ico.ResetIcon();
        }

        mIconsMap.Clear();
    }
}