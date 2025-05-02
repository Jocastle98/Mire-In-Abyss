using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    [System.Serializable]
    public class DropItem
    {
        public GameObject itemPrefab;
        [Range(0, 1)] public float dropChance = 0.25f;
    }

    [SerializeField] private List<DropItem> dropItems = new List<DropItem>();
    [SerializeField] private float dropForce = 3f;
    [SerializeField] private float dropRadius = 1f;

    public void DropItemOnDeadth()
    {
        foreach (var item in dropItems)
        {
            if (Random.value <= item.dropChance)
            {
                ItemDrop(item.itemPrefab);
            }
        }
    }

    private void ItemDrop(GameObject itemPrefab)
    {
        if (itemPrefab == null) return;

        Vector3 randomDirection = new Vector3(
            Random.Range(-1f, 1f),
            0.5f,
            Random.Range(-1f, 1)).normalized;

        Vector3 position = transform.position + Vector3.up * 0.5f;
        GameObject droppedItem = Instantiate(itemPrefab, position, Quaternion.identity);

        Rigidbody rigid = droppedItem.GetComponent<Rigidbody>();
        if (rigid != null)
        {
            rigid.AddForce(randomDirection * dropForce, ForceMode.Impulse);
        }
    }
}
