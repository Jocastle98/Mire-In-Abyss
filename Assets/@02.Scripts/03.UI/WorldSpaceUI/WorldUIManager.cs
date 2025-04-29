using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Events.Abyss;
using Events.Combat;
using R3;
using UnityEngine;

public sealed class WorldUIManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] DamageTextView mDmgTextPrefab;
    [SerializeField] HpBarUI mHpBarPrefab;
    [SerializeField] RectTransform mDamageTextRoot;
    [SerializeField] RectTransform mHpBarRoot;
    [ColorUsage(true, true)]
    [SerializeField] private Color mDefaultDamageTextColor = Color.white;

    [Header("Damage Text")]
    [SerializeField] float mDmgTextLifeTime = 0.4f;
    [SerializeField] float mDmgTextFadeTime = 0.4f;
    [SerializeField] float mDmgTextFadeDelay = 0.2f;
    [SerializeField] float mDmgTextOffsetX = 0.1f;
    [SerializeField] float mDmgTextOffsetY = 0.8f;
    [SerializeField] float mDmgTextFloatHeight = 0.3f;

    /* Pools */
    private ObjectPool<DamageTextView> mDmgPool;
    private ObjectPool<HpBarUI> mHpPool;

    /* Active track */
    private readonly Dictionary<Transform, HpBarUI> mHpBars = new();
    private readonly CompositeDisposable mCD = new();
    private Camera mWorldCam;


    void Awake()
    {
        mDmgPool = new(mDmgTextPrefab, mDamageTextRoot, 32);
        mHpPool = new(mHpBarPrefab, mHpBarRoot, 8);
        mWorldCam = Camera.main;
    }

    void Start()
    {
        subscribeEvents();
    }

    void LateUpdate()
    {
        /* follow targets */
        foreach (var kv in mHpBars)
        {
            var t = kv.Key;
            if (t != null)
            {
                kv.Value.transform.position = t.position;
            }
        }
    }

    void subscribeEvents()
    {
        /* Damage Popup */
        R3EventBus.Instance.Receive<DamagePopup>()
            .Subscribe(p => SpawnDamageText(p).Forget())
            .AddTo(mCD);

        /* Enemy HP */
        R3EventBus.Instance.Receive<EnemyHpChanged>()
            .Subscribe(e => UpdateHpBar(e))
            .AddTo(mCD);

        R3EventBus.Instance.Receive<EnemyDied>()
            .Subscribe(e => DeleteHpBar(e.UIAnchor))
            .AddTo(mCD);
    }

    /* -------- Damage -------- */
    async UniTaskVoid SpawnDamageText(DamagePopup p)
    {
        var view = mDmgPool.Rent();
        view.GetComponent<Billboard>().enabled = true;
        var start = getSpawnPos(p);
        view.transform.position = start;
        if(p.Color == default)
        {
            view.SetText(p.Amount, mDefaultDamageTextColor);
        }
        else
        {
            view.SetText(p.Amount, p.Color);
        }

        // 대미지 텍스트 생성 후 날아가는 방향
        Vector2 dir2D = new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(0.5f, 1f)).normalized;
        Vector3 dir3D = mWorldCam.transform.right * dir2D.x + mWorldCam.transform.up * dir2D.y;

        var moveTask = view.transform.DOMove(start + dir3D * mDmgTextFloatHeight, mDmgTextLifeTime).SetEase(Ease.OutQuad).ToUniTask();

        // 페이드
        var fadeTask = view.Text.DOFade(0, mDmgTextFadeTime)
                 .SetDelay(mDmgTextFadeDelay)     
                 .SetEase(Ease.InQuad).ToUniTask();

        await UniTask.WhenAll(moveTask, fadeTask);

        view.GetComponent<Billboard>().enabled = false;
        mDmgPool.Return(view);
    }

    private Vector3 getSpawnPos(in DamagePopup p)
    {
        Vector3 pos = p.WorldPos + Vector3.up * mDmgTextOffsetY;

        float horiz = Random.Range(-mDmgTextOffsetX, mDmgTextOffsetX);
        pos += mWorldCam.transform.right * horiz;

        return pos;
    }

    /* -------- HP Bar -------- */
    void UpdateHpBar(EnemyHpChanged e)
    {
        if (!mHpBars.TryGetValue(e.Anchor, out var bar))
        {
            bar = mHpPool.Rent();
            bar.GetComponent<Billboard>().enabled = true;
            mHpBars[e.Anchor] = bar;
        }
        bar.SetProgress(e.Current / (float)e.Max);

        if (e.Current <= 0)
        {
            DeleteHpBar(e.Anchor);
        }
    }

    void DeleteHpBar(Transform target)
    {
        if (mHpBars.TryGetValue(target, out var bar))
        {
            bar.GetComponent<Billboard>().enabled = false;
            mHpPool.Return(bar);
            mHpBars.Remove(target);
        }
    }

    void OnDisable()
    {
        mCD.Dispose();
        foreach (var v in mHpBars.Values)
        {
            mHpPool.Return(v);
        }
        mHpBars.Clear();
    }
}
