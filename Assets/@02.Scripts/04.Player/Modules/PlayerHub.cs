using Events.Data;
using Events.Player;
using Events.Player.Modules;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Inventory))]
[RequireComponent(typeof(BuffController))]
[RequireComponent(typeof(QuestLog))]
[RequireComponent(typeof(SkillController))]

// 현재 PlayerHub에 Soul, QuestLog 일부 등 UserData에 저장되어야 하는 데이터가 혼용 되어있음
// 수정 전까진 "PlayerHub에서 해당 데이터를 관리" 및 구독 전파를 하고 UserData에 직접 변경하는 로직을 넣어두는 방식으로 두고 
// 수정 후 해당 데이터를 모두 UserData에 저장하고 구독 전파 방식으로 변경 예정
public class PlayerHub : Singleton<PlayerHub>
{
    public Inventory Inventory { get; private set; }
    public BuffController BuffController { get; private set; }
    public QuestLog QuestLog { get; private set; }
    public SkillController Skills { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        Inventory = GetComponent<Inventory>();
        BuffController = GetComponent<BuffController>();
        QuestLog = GetComponent<QuestLog>();
        Skills = GetComponent<SkillController>();

        // UserData를 통해 Soul, QuestLog 등 데이터 초기화 예정
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
        QuestLog.Accepted.Subscribe(e => R3EventBus.Instance.Publish(new QuestAccepted(e.ID))).AddTo(this);
        QuestLog.Progress.Subscribe(e => R3EventBus.Instance.Publish(new QuestUpdated(e.ID, e.CurrentAmount))).AddTo(this);
        QuestLog.Completed.Subscribe(e => R3EventBus.Instance.Publish(new QuestCompleted(e.ID))).AddTo(this);
        QuestLog.Rewarded.Subscribe(e => R3EventBus.Instance.Publish(new QuestRewarded(e.ID))).AddTo(this);

        // Skills
        Skills.SkillUsed.Subscribe(e => R3EventBus.Instance.Publish(new SkillUsed(e.ID))).AddTo(this);
        Skills.SkillUpdated.Subscribe(e => R3EventBus.Instance.Publish(new SkillUpdated(e.ID, e.CooldownTime, e.KeyString))).AddTo(this);
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }
}
