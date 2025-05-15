using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoulStoneUpgradeData", menuName = "Game/Soul Stone Upgrade Data")]
public class SoulStoneUpgradeData : ScriptableObject
{
    [System.Serializable]
    public class UpgradeInfo
    {
        public string UpgradeId;                // 업그레이드 ID (체력, 공격력 등)
        public string Title;                    // 업그레이드 이름 (표시용)
        public string Description;              // 업그레이드 설명
        public Sprite Icon;                     // 업그레이드 아이콘
        public int[] Costs = new int[5];        // 레벨별 비용 배열 (5단계)
        public float[] Values = new float[5];   // 레벨별 적용 수치 배열 (5단계)
        public string ValueType;                // 적용 방식 (add, multiply, percent)
        [NonSerialized] public int CurrentLevel;
    }

    [Header("업그레이드 정보")] 
    [SerializeField] private List<UpgradeInfo> mUpgrades = new List<UpgradeInfo>();

    public List<UpgradeInfo> GetAllUpgrades()
    {
        return mUpgrades;
    }

    public UpgradeInfo GetUpgradeInfoById(string id)
    {
        return mUpgrades.Find(u => u.UpgradeId == id);
    }

    public void CreateDefaultUpgrade()
    {
        mUpgrades.Clear();
        
        // 1. 최대 체력
        var maxHP = new UpgradeInfo
        {
            UpgradeId = "maxHP",
            Title = "최대체력",
            Description = "캐릭터의 최대 체력을 증가시킵니다.",
            Costs = new int[] { 5, 10, 20, 35, 60 },
            Values = new float[] { 10f, 20f, 30f, 45f, 60f },
            ValueType = "flat"
        };
        mUpgrades.Add(maxHP);

        // 2. 공격력
        var attackPower = new UpgradeInfo
        {
            UpgradeId = "attackPower",
            Title = "공격력",
            Description = "캐릭터의 기본 공격력을 증가시킵니다.",
            Costs = new int[] { 7, 15, 30, 50, 85 },
            Values = new float[] { 3f, 5f, 6f, 7f, 9f },
            ValueType = "flat"
        };
        mUpgrades.Add(attackPower);

        // 3. 이동속도
        var moveSpeed = new UpgradeInfo
        {
            UpgradeId = "moveSpeed",
            Title = "이동속도",
            Description = "캐릭터의 이동 속도를 증가시킵니다.",
            Costs = new int[] { 6, 12, 24, 40, 70 },
            Values = new float[] { 0.5f, 1f, 1.2f, 2f, 2.3f },
            ValueType = "flat"
        };
        mUpgrades.Add(moveSpeed);

        // 4. 방어력
        var defense = new UpgradeInfo
        {
            UpgradeId = "defense",
            Title = "방어력",
            Description = "받는 피해를 감소시킵니다.",
            Costs = new int[] { 6, 13, 26, 45, 75 },
            Values = new float[] { 1f, 2f, 2f, 3f, 5f },
            ValueType = "flat"
        };
        mUpgrades.Add(defense);

        // 5. 치명타 확률
        var critChance = new UpgradeInfo
        {
            UpgradeId = "critChance",
            Title = "치명타확률",
            Description = "치명타가 발생할 확률을 증가시킵니다.",
            Costs = new int[] { 10, 20, 40, 70, 120 },
            Values = new float[] { 0.03f, 0.06f, 0.09f, 0.12f, 0.15f },
            ValueType = "flat"
        };
        mUpgrades.Add(critChance);
       

        // 6. 영혼석 획득량
        var soulAcquisition = new UpgradeInfo
        {
            UpgradeId = "soulAcquisition",
            Title = "영혼석",
            Description = "영혼석 획득량이 증가합니다.",
            Costs = new int[] { 15, 30, 60, 100, 150 },
            Values = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f },
            ValueType = "percent"
        };
        mUpgrades.Add(soulAcquisition);

        // 7. 쿨다운 감소
        var cooldownReduction = new UpgradeInfo
        {
            UpgradeId = "cooldownReduction",
            Title = "스킬 쿨",
            Description = "모든 스킬의 재사용 대기시간이 감소합니다.",
            Costs = new int[] { 20, 40, 80, 130, 200 },
            Values = new float[] { 0.04f, 0.08f, 0.12f, 0.16f, 0.2f },
            ValueType = "percent"
        };
        mUpgrades.Add(cooldownReduction);

        // 8. 아이템 드롭률
        var itemDropRate = new UpgradeInfo
        {
            UpgradeId = "itemDropRate",
            Title = "아이템 드랍",
            Description = "아이템 드랍 확률이 증가합니다.",
            Costs = new int[] { 13, 26, 52, 90, 140 },
            Values = new float[] { 0.05f, 0.1f, 0.15f, 0.2f, 0.3f },
            ValueType = "percent"
        };
        mUpgrades.Add(itemDropRate);
        
        // 9. 레벨업 속도 증가
        var levelUpSpeed = new UpgradeInfo
        {
            UpgradeId = "levelUp",
            Title = "레벨업",
            Description = "경험치 획득량이 증가합니다.",
            Costs = new int[] { 12, 24, 48, 80, 130 },
            Values = new float[] { 0.05f, 0.1f, 0.15f, 0.2f, 0.3f },
            ValueType = "percent"
        };
        mUpgrades.Add(levelUpSpeed);

        // 10. 골드 획득량
        var goldAcquisition = new UpgradeInfo
        {
            UpgradeId = "goldAcquisition",
            Title = "골드",
            Description = "골드 획득량이 증가합니다.",
            Costs = new int[] { 10, 20, 40, 70, 110 },
            Values = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f },
            ValueType = "percent"
        };
        mUpgrades.Add(goldAcquisition);
    }
}



#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(SoulStoneUpgradeData))]
public class SoulStoneUpgradeDataEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SoulStoneUpgradeData data = (SoulStoneUpgradeData)target;

        if (GUILayout.Button("기본 업그레이드 데이터 생성"))
        {
            data.CreateDefaultUpgrade();
            UnityEditor.EditorUtility.SetDirty(data);
        }
    }
}
#endif
