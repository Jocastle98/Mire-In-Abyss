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
        mItemEffectSystem.AcquireItem(mItemID);

        Item item = mItemDatabase.GetItemByID(mItemID);

        string message = $"{item.ItemName} : {item.Description}";
        R3EventBus.Instance.Publish(new Events.HUD.ToastPopup(message, mDisplayDuration, mToastColor));

        R3EventBus.Instance.Publish(new Events.Item.ItemAdded(item.ID, 1));
        
        Destroy(gameObject);
    }

    private void Update()
    {
        transform.Rotate(0, mRotationSpeed * Time.deltaTime, 0);
    }
}
