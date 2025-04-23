using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UIPanelEnums;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] private UIPanelType mPanelType;
    [SerializeField] private GameObject mInteractionTextUI;
    [SerializeField] private TextMeshProUGUI mTextComponent;

    private bool playerInRange = false;

    private void Start()
    {
        SetInteractionText();
        mInteractionTextUI.SetActive(false);
    }

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

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            UIPanelManager.Instance.OpenPanel(mPanelType);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            mInteractionTextUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            mInteractionTextUI.SetActive(false);
        }
    }
}
