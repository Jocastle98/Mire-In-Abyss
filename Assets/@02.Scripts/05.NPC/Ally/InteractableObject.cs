using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UIPanelEnums;
using UnityEngine;

/// <summary>
/// 플레이어가 상호작용할 수 있는 오브젝트 관리
/// </summary>
public class InteractableObject : MonoBehaviour
{
    [SerializeField] private UIPanelType mPanelType;        //상호작용 시 열릴 UI 패널 타입
    [SerializeField] private GameObject mInteractionTextUI; //상호작용 안내 UI 오브젝트(텍스트박스 오브젝트)
    [SerializeField] private TextMeshProUGUI mTextComponent;//상호작용 안내 텍스트

    private bool playerInRange = false; //플레이어가 상호작용 범위 내에 있는지 여부

    private void Start()
    {
        SetInteractionText();
        mInteractionTextUI.SetActive(false);
    }

    /// <summary>
    /// 인스펙터에 할당된 패널 타입에 따라 안내 텍스트 설정
    /// </summary>
    private void SetInteractionText()
    {
        switch (mPanelType)
        {
            case UIPanelType.SoulStoneShop:
                mTextComponent.text = "E : 영혼석 상점";
                break;
            case UIPanelType.QuestBoard:
                mTextComponent.text = "E : 퀘스트";
                break;
            case UIPanelType.EnterPortal:
                mTextComponent.text = "E : 심연으로 이동";
                break;
        }
    }

    public UIPanelType GetPanelType()
    {
        return mPanelType;
    }

    /// <summary>
    /// 플레이어가 상호작용 범위에 들어왔을 때 호출
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SetInteractionText();
            playerInRange = true;
            mInteractionTextUI.SetActive(true);

            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetNearestInteractable(this);
            }
        }
    }

    /// <summary>
    /// 상호작용 범위에 나갔을 때 호출
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            mInteractionTextUI.SetActive(false);
            
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null && playerController.NearestInteractableObject == this)
            {
                playerController.SetNearestInteractable(null);
            }
        }
    }
}
