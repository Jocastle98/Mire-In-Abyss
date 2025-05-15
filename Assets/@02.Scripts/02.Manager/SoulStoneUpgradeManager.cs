using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SoulStoneUpgradeManager : Singleton<SoulStoneUpgradeManager>
{
    [SerializeField] private SoulStoneUpgradeData mUpgradeData;
    private PlayerStats mPlayerStats;

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded; // ensure we re‑apply when gameplay scene reloads
    }
    void Initialize() // Start였던 것
    {
        mPlayerStats = TempRefManager.Instance.Player.GetComponent<PlayerStats>();

        var upgradeInfos = mUpgradeData.GetAllUpgrades();
        foreach (var info in upgradeInfos)
        {
            info.CurrentLevel = UserData.Instance.GetSoulUpgradeLevel(info.UpgradeId);
            int level = info.CurrentLevel - 1;
            if (level < 0 || level >= 5) continue;

            for (int i = 0; i <= level; i++)
            {
                ApplyStat(info.UpgradeId, info.Values[i], info.ValueType);
            }
        }
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == Constants.TownScene)
        {
            Initialize();
        }
    }

    public void ApplyStat(string id, float value, string valueType)
    {
        switch (id)
        {
            case "maxHP": mPlayerStats.ModifyMaxHP(value, valueType); break;
            case "attackPower": mPlayerStats.ModifyAttackPower(value, valueType); break;
            case "moveSpeed": mPlayerStats.ModifyMoveSpeed(value, valueType); break;
            case "defence": mPlayerStats.ModifyDefence(value, valueType); break;
            case "critChance": mPlayerStats.ModifyCritChance(value, valueType); break;
            case "soulAcquisition": if (valueType == "percent") mPlayerStats.SetSoulStoneMultiplier(value); break;
            case "cooldownReduction": if (valueType == "percent") mPlayerStats.SetCoolDownReduction(value); break;
            case "itemDropRate": if (valueType == "percent") mPlayerStats.SetItemDropRateBonus(value); break;
            case "levelUp": if (valueType == "percent") mPlayerStats.SetExpMultiplier(value); break;
            case "goldAcquisition": if (valueType == "percent") mPlayerStats.SetGoldMultiplier(value); break;
            default: Debug.LogWarning($"Unknown upgrade id {id}"); break;
        }
    }
}