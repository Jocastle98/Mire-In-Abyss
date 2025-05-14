using System;
using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoulStoneShopPanelController : BaseUIPanel
{
    [System.Serializable]
    public class UpgradeButton
    {
        public GameObject Background;           //업그레이드 배경
        public GameObject SelectorImage;        //선택시 활성화 되는 이미지
        public Button UpgradeButtonUI;          //업그레이드 버튼
        public TextMeshProUGUI TitleText;       //업그레이드 이름 텍스트
        public Image IconImage;                 //업그레이드 아이콘 이미지
        public GameObject[] LevelImages;        //레벨 표시 이미지 배열 5개
    }

    [Header("업그레이드 정보")] 
    [SerializeField] private SoulStoneUpgradeData mUpgradeData;
    [SerializeField] private List<SoulStoneUpgradeData.UpgradeInfo> mUpgradeInfos = new List<SoulStoneUpgradeData.UpgradeInfo>();

    [Header("UI요소")] 
    [SerializeField] private List<UpgradeButton> mUpgradeButtons = new List<UpgradeButton>();

    [SerializeField] private TextMeshProUGUI mDescriptionText;      //선택된 업그레이드 설명
    [SerializeField] private TextMeshProUGUI mSoulStoneCountText;   //현재 보유 영혼석
    [SerializeField] private TextMeshProUGUI mCostText;             //강화 비용
    [SerializeField] private Button mUpgradeApplyButton;            //강화 적용 버튼
    [SerializeField] BackButton mBackButton;

    [Header("참조")] 
    [SerializeField] private PlayerStats mPlayerStats;

    private SoulStoneUpgradeData.UpgradeInfo mSelectedUpgrade;   //현재 선택된 업그레이드
    private int mSoulStones = 0;          //현재 보유 영혼석

    private const string UPGRADE_LEVEL_SAVE_KEY = "SoulStoneUpgradeLevels";

    protected override void Awake()
    {
        base.Awake();

        if (mBackButton != null)
        {
            mBackButton.SetAfterCloseAction(() =>
            {
                mPlayer?.SetPlayerState(PlayerState.Idle);
            });
        }
    }
    
    private void OnEnable()
    {
        mSoulStones = PlayerHub.Instance.Inventory.Soul;
        UpdateSoulStoneText();

        if (mSelectedUpgrade != null)
        {
            UpdateUpgradeApplyButton();
        }
    }
    
    private void Start()
    {
        if (mPlayerStats == null)
        {
            mPlayerStats = FindObjectOfType<PlayerStats>();
        }

        mUpgradeInfos = mUpgradeData.GetAllUpgrades();
        InitializeUI();
        
    }

    private void InitializeUI()
    {
        UpdateSoulStoneText();
        mCostText.text = "";
        
        for (int i = 0; i < mUpgradeButtons.Count && i < mUpgradeInfos.Count; i++)
        {
            UpgradeButton button = mUpgradeButtons[i];
            SoulStoneUpgradeData.UpgradeInfo info = mUpgradeInfos[i];

            //버튼 설정
            button.TitleText.text = info.Title;
            button.IconImage.sprite = info.Icon;

            //현재 레벨에 맞게 레벨 이미지 활성화
            UpdateLevelImages(button, info.CurrentLevel);
            
            //선택 이미지 비활성화
            button.SelectorImage.SetActive(false);

            int index = i;
            button.UpgradeButtonUI.onClick.AddListener(() =>
            {
                // 업그레이드 아이콘 버튼 클릭 시 
                AudioManager.Instance.PlayUi(AudioEnums.EUiType.Click);
                OnUpgradeButtonClicked(index);
            });
        }
        
        //강화 적용 버튼 이벤트 설정
        mUpgradeApplyButton.onClick.AddListener(() => {
            AudioManager.Instance.PlayUi(AudioEnums.EUiType.Click);  
            OnUpgradeApplyButtonClicked();
        });
        mUpgradeApplyButton.interactable = false;

        mDescriptionText.text = "강화할 능력을 선택하세요";
    }

    private void OnUpgradeButtonClicked(int index)
    {
        foreach (var button in mUpgradeButtons)
        {
            button.SelectorImage.SetActive(false);
        }
        mUpgradeButtons[index].SelectorImage.SetActive(true);
        mSelectedUpgrade = mUpgradeInfos[index];
        UpdateDescriptionText();
        UpdateUpgradeApplyButton();
    }

    private void OnUpgradeApplyButtonClicked()
    {
        if (mSelectedUpgrade == null || mSelectedUpgrade.CurrentLevel >= 5) return;
        int cost = mSelectedUpgrade.Costs[mSelectedUpgrade.CurrentLevel];

        //영혼석 충분한지 확인
        if (mSoulStones < cost)
        {
            R3EventBus.Instance.Publish(new Events.HUD.ToastPopup("영혼석이 부족합니다.", 2f, Color.red));
            return;
        }

        //영혼석 차감
        mSoulStones -= cost;
        UpdateSoulStoneText();
        PlayerHub.Instance.Inventory.SubTrackSoul(cost);
        
        //현재 레벨 증가
        mSelectedUpgrade.CurrentLevel++;
        
        //강화 효과 적용
        ApplyUpgradeEffect();

        //UI 업데이트
        int buttonIndex = mUpgradeInfos.IndexOf(mSelectedUpgrade);
        if (buttonIndex >= 0 && buttonIndex < mUpgradeButtons.Count)
        {
            UpdateLevelImages(mUpgradeButtons[buttonIndex], mSelectedUpgrade.CurrentLevel);
        }

        //설명 텍스트 업데이트
        UpdateDescriptionText();
        //강화 적용 버튼 상태 업데이트
        UpdateUpgradeApplyButton();
        
        R3EventBus.Instance.Publish(new Events.HUD.ToastPopup($"{mSelectedUpgrade.Title} 강화 완료!", 2f, Color.yellow));
    }

    private void UpdateLevelImages(UpgradeButton button, int level)
    {
        for (int i = 0; i < button.LevelImages.Length; i++)
        {
            button.LevelImages[i].SetActive(i < level);
        }
    }

    private void UpdateDescriptionText()
    {
        if (mSelectedUpgrade == null)
        {
            mDescriptionText.text = "강화할 능력을 선택하세요";
            mCostText.text = "";
            return;
        }

        int currentLevel = mSelectedUpgrade.CurrentLevel;
        string description = mSelectedUpgrade.Description;
        if (currentLevel >= 5)
        {
            mDescriptionText.text = "최대 레벨에 도달했습니다";
            mCostText.text = "";
            return;
        }

        //현재 효과 및 다음 레벨 효과 정보
        float currentValue = currentLevel > 0 ? mSelectedUpgrade.Values[currentLevel - 1] : 0;
        float nextValue = mSelectedUpgrade.Values[currentLevel];
        
        //효과 표시 방식
        string valueFormat = mSelectedUpgrade.ValueType == "percent" ? "{0:P0}" : "{0:F1}";
        string currentValueText = string.Format(valueFormat, currentValue);
        string nextValueText = string.Format(valueFormat, nextValue);

        if (mSelectedUpgrade.UpgradeId == "critChance")
        {
            currentValueText = string.Format("{0:P0}", currentValue);
            nextValueText = string.Format("{0:P0}", nextValue);
        }
        
        //비용 정보
        int cost = mSelectedUpgrade.Costs[currentLevel];

        mDescriptionText.text = $"{description} \n 현재: {currentValueText} 다음: {nextValueText}";
        mCostText.text = $"{cost}";
    }

    private void UpdateUpgradeApplyButton()
    {
        if (mSelectedUpgrade == null || mSelectedUpgrade.CurrentLevel >= 5)
        {
            mUpgradeApplyButton.interactable = false;
            return;
        }

        int cost = mSelectedUpgrade.Costs[mSelectedUpgrade.CurrentLevel];
        mUpgradeApplyButton.interactable = mSoulStones >= cost;
    }

    private void UpdateSoulStoneText()
    {
        mSoulStoneCountText.text = mSoulStones.ToString();
    }

    private void ApplyUpgradeEffect()
    {
        if (mPlayerStats == null || mSelectedUpgrade == null) return;

        int level = mSelectedUpgrade.CurrentLevel;
        if (level <= 0 || level > 5) return;

        float value = mSelectedUpgrade.Values[level - 1];
        string valueType = mSelectedUpgrade.ValueType;

        switch (mSelectedUpgrade.UpgradeId)
        {
            case "maxHP":
                mPlayerStats.ModifyMaxHP(value, valueType);
                break;
            case "attackPower":
                mPlayerStats.ModifyAttackPower(value, valueType);
                break;
            case "moveSpeed":
                mPlayerStats.ModifyMoveSpeed(value, valueType);
                break;
            case "defence":
                mPlayerStats.ModifyDefence(value, valueType);
                break;
            case "critChance":
                mPlayerStats.ModifyCritChance(value, valueType);
                break;
            case "soulStone":
                if (valueType == "percent")
                {
                    mPlayerStats.SetSoulStoneMultiplier(value);
                }
                break;
            case "coolDown":
                if (valueType == "percent")
                {
                    mPlayerStats.SetCoolDownReduction(value);
                }
                break;
            case "itemDrop":
                if (valueType == "percent")
                {
                    mPlayerStats.SetItemDropRateBonus(value);
                }
                break;
            case "level":
                if (valueType == "percent")
                {
                    mPlayerStats.SetExpMultiplier(value);
                }
                break;
            case "gold":
                if (valueType == "percent")
                {
                    mPlayerStats.SetGoldMultiplier(value);
                }
                break;
            default:
                Debug.LogWarning($"알수없는 업그레이드 ID : {mSelectedUpgrade.UpgradeId}");
                break;
        }
    }

    #region 디버깅 관련

    public void SetSoulStone(int amount)
    {
        PlayerHub.Instance.Inventory.AddSoul(amount);
        UpdateSoulStoneText();
        UpdateUpgradeApplyButton();
    }

    public void AddSoultStone(int amount)
    {
        PlayerHub.Instance.Inventory.AddSoul(amount);
        UpdateSoulStoneText();
        UpdateUpgradeApplyButton();
    }

    #endregion
}
