using Events.Player.Modules;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Inventory))]
[RequireComponent(typeof(BuffController))]
[RequireComponent(typeof(QuestLog))]
public class PlayerHub : Singleton<PlayerHub>
{
    public Inventory Inventory { get; private set; }
    public BuffController BuffController { get; private set; }
    public QuestLog QuestLog { get; private set; }

    void Awake()
    {
        Inventory = GetComponent<Inventory>();
        BuffController = GetComponent<BuffController>();
        QuestLog = GetComponent<QuestLog>();
    }

    void Start()
    {
        Inventory.Init(0, 0);
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        // Inventory
        Inventory.ItemAdded.Subscribe(e => R3EventBus.Instance.Publish(new ItemAdded(e.ID, e.AddedAmount, e.Total))).AddTo(this);
        Inventory.ItemRemoved.Subscribe(e => R3EventBus.Instance.Publish(new ItemSubtracked(e.ID, e.RemovedAmount, e.Total))).AddTo(this);
        Inventory.GoldAdded.Subscribe(e => R3EventBus.Instance.Publish(new GoldAdded(e.AddedAmount, e.Total))).AddTo(this);
        Inventory.GoldSubTracked.Subscribe(e => R3EventBus.Instance.Publish(new GoldSubTracked(e.RemovedAmount, e.Total))).AddTo(this);
        Inventory.SoulAdded.Subscribe(e => R3EventBus.Instance.Publish(new SoulAdded(e.AddedAmount, e.Total))).AddTo(this);
        Inventory.SoulSubTracked.Subscribe(e => R3EventBus.Instance.Publish(new SoulSubTracked(e.RemovedAmount, e.Total))).AddTo(this);

        // BuffController
        BuffController.Added.Subscribe(e => R3EventBus.Instance.Publish(new BuffAdded(e.ID, e.Duration, e.IsDebuff))).AddTo(this);
        BuffController.Refreshed.Subscribe(e => R3EventBus.Instance.Publish(new BuffRefreshed(e.ID, e.NewRemain))).AddTo(this);
        BuffController.Ended.Subscribe(e => R3EventBus.Instance.Publish(new BuffEnded(e.ID))).AddTo(this);

        // QuestLog
        QuestLog.Accepted.Subscribe(e => R3EventBus.Instance.Publish(new QuestAccepted(e.ID, e.Cur, e.Target))).AddTo(this);
        QuestLog.Progress.Subscribe(e => R3EventBus.Instance.Publish(new QuestUpdated(e.ID, e.Cur, e.Target))).AddTo(this);
        QuestLog.Completed.Subscribe(e => R3EventBus.Instance.Publish(new QuestCompleted(e.ID))).AddTo(this);
        QuestLog.Rewarded.Subscribe(e => R3EventBus.Instance.Publish(new QuestRewarded(e.ID))).AddTo(this);
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }
}
