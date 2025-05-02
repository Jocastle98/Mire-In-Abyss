using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    [SerializeField] private int mItemID;
    [SerializeField] private string mItemDescription;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private Color toastColor = Color.white;

    [SerializeField] private ItemEffectSystem itemEffectSystem;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CollectItem(other.gameObject);
        }
    }

    private void CollectItem(GameObject player)
    {
        itemEffectSystem.AcquireItem(mItemID);

        string message = $"{mItemID} : {mItemDescription}";
        R3EventBus.Instance.Publish(new Events.HUD.ToastPopup(message, displayDuration, toastColor));

        int itemID = FindItemID(mItemID);
        if (itemID > 0)
        {
            R3EventBus.Instance.Publish(new Events.Item.ItemAdded(itemID, 1));
        }

        Destroy(gameObject);
    }

    private int FindItemID(int itemID)
    {
        if (name.Contains("_"))
        {
            string[] parts = name.Split('_');
            if (parts.Length > 1 && int.TryParse(parts[1], out int id))
            {
                return id;
            }
        }

        return 101;
    }
}
