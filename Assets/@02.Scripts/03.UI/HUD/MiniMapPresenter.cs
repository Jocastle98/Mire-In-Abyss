using System.Collections.Generic;
using UnityEngine;
using R3;
using UIHUDEnums;
using Events.HUD;
using Events.Gameplay;
using Unity.VisualScripting;


public sealed class MiniMapPresenter : HudPresenterBase
{
    [Header("Refs")] 
    [SerializeField] RectTransform mOtherIconLayer;
    [SerializeField] RectTransform mPlayerIconLayer;
    [SerializeField] Camera mMiniMapCam;
    Transform mPlayer;
    [SerializeField] private List<MiniMapIcon> mIconPrefabs;

    private Camera mMainCam;
    private MiniMapIcon mPlayerIcon;
    private RectTransform mPlayerIconRT;
    private readonly Dictionary<int, MiniMapIcon> mIconsMap = new(); // id → icon
    private List<ObjectPool<MiniMapIcon>> mIconPools = new();
    private float mWorldToUIScale;

    private void Awake()
    {
        mIconPools.Add(new(mIconPrefabs[(int)MiniMapIconType.Player], mPlayerIconLayer, 1));
        mIconPools.Add(new(mIconPrefabs[(int)MiniMapIconType.Enemy], mOtherIconLayer, 20));
        mIconPools.Add(new(mIconPrefabs[(int)MiniMapIconType.Boss], mOtherIconLayer, 2));
        mIconPools.Add(new(mIconPrefabs[(int)MiniMapIconType.Shop], mOtherIconLayer, 1));
        mIconPools.Add(new(mIconPrefabs[(int)MiniMapIconType.Portal], mOtherIconLayer, 2));
        mIconPools.Add(new(mIconPrefabs[(int)MiniMapIconType.QuestBoard], mOtherIconLayer, 1));
        mIconPools.Add(new(mIconPrefabs[(int)MiniMapIconType.SoulStoneShop], mOtherIconLayer, 1));


        mPlayerIcon = mIconPools[(int)MiniMapIconType.Player].Rent();
        
        // 플레이어 아이콘 회전을 위한 할당
        mPlayerIconRT = mPlayerIcon.GetComponent<RectTransform>();
        mMainCam = Camera.main;
    }


    void Start()
    {
        mPlayer = TempRefManager.Instance.Player.transform;
    }
    public override void Initialize()
    {
        subscribeEvents();

        mWorldToUIScale = mOtherIconLayer.rect.width / (mMiniMapCam.orthographicSize * 2); // 월드 → UI 스케일
    }

    void LateUpdate()
    {
        // 플레이어 아이콘 업데이트
        var playerIconRotation = mPlayerIconRT.rotation.eulerAngles;
        playerIconRotation.z = -mMainCam.transform.rotation.eulerAngles.y;
        mPlayerIconRT.rotation = Quaternion.Euler(playerIconRotation);
        var playerWorldPos = mPlayer.position;
        var playerMinimapPos = mMiniMapCam.transform.InverseTransformPoint(playerWorldPos);
        mPlayerIconRT.anchoredPosition = playerMinimapPos * mWorldToUIScale;

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

    private void subscribeEvents()
    {
        R3EventBus.Instance.Receive<EntitySpawned<IMapTrackable>>()
            .Subscribe(e => spawnIcon(e.Entity.MapAnchor, e.Entity.IconType))
            .AddTo(mCD);
        R3EventBus.Instance.Receive<EntityDestroyed<IMapTrackable>>()
            .Subscribe(e => despawnIcon(e.Entity.MapAnchor, e.Entity.IconType))
            .AddTo(mCD);
        R3EventBus.Instance.Receive<EnterDeepAbyss>()
            .Subscribe(e => despawnAllIconsWithoutPlayer())
            .AddTo(mCD);
    }

    private bool isOutOfMinimap(Vector3 minimapCamPos)
    {
        float half = mMiniMapCam.orthographicSize;
        
        return Mathf.Abs(minimapCamPos.x) > half || Mathf.Abs(minimapCamPos.y) > half;
    }

    private void spawnIcon(Transform target, MiniMapIconType iconType)
    {
        var icon = mIconPools[(int)iconType].Rent();
        icon.Init(target, iconType);
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

    private void despawnAllIconsWithoutPlayer()
    {
        foreach (var pair in mIconsMap)
        {
            pair.Value.ResetIcon();
            mIconPools[(int)pair.Value.IconType].Return(pair.Value);
        }
        mIconsMap.Clear();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        despawnAllIconsWithoutPlayer();

        mIconsMap.Clear();
    }
}