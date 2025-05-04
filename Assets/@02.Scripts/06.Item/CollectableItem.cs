using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    [SerializeField] private int mItemID;
    [SerializeField] private float mDisplayDuration = 2f;
    [SerializeField] private Color mToastColor = Color.white;
    [SerializeField] private float mRotationSpeed = 30f;

    [SerializeField] private ItemEffectSystem mItemEffectSystem;
    [SerializeField] private ItemDatabase mItemDatabase;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CollectItem(other.gameObject);
        }
    }

    private void CollectItem(GameObject player)
    {
        PlayerHub.Instance.Inventory.AddItem(mItemID, 1);
        Item item = mItemDatabase.GetItemByID(mItemID);

        string message = $"{item.ItemName} : {item.Description}";
        R3EventBus.Instance.Publish(new Events.HUD.ToastPopup(message, mDisplayDuration, mToastColor));
        
        Destroy(gameObject);
    }

    private void Update()
    {
        transform.Rotate(0, mRotationSpeed * Time.deltaTime, 0);
    }
}
