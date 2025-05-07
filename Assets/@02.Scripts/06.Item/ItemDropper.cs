using System.Collections;
using System.Collections.Generic;
using EnemyEnums;
using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    [System.Serializable]
    public class DropItem
    {
        public GameObject itemPrefab;
        [Range(0, 1)] public float dropChance = 1f; //몬스터 자체에서 아이템을 떨어뜨릴 확률
    }

    [SerializeField] private List<DropItem> dropItems = new List<DropItem>();
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

        Vector3 position = transform.position + Vector3.up * 0.5f;
        GameObject droppedItem = Instantiate(itemPrefab, position, Quaternion.identity);

        StartCoroutine(AnimateItemDrop(droppedItem, position));
    }

    private IEnumerator AnimateItemDrop(GameObject item, Vector3 startPos)
    {
        float duration = 0.5f;
        float height = 1.5f;
        float elapsed = 0;

        while (elapsed < duration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float ratio = elapsed / (duration / 2);

            Vector3 newPos = startPos;
            newPos.y = startPos.y + Mathf.Sin(ratio * Mathf.PI * 0.5f) * height;

            item.transform.position = newPos;
            yield return null;
        }

        Vector3 peakPos = item.transform.position;
        elapsed = 0;

        while (elapsed < duration* 0.5f)
        {
            elapsed += Time.deltaTime;
            float ratio = elapsed / (duration * 0.5f) ;

            Vector3 newPos = peakPos;
            newPos.y = peakPos.y - Mathf.Sin(ratio * Mathf.PI * 0.5f) * height;

            item.transform.position = newPos;
            yield return null;
        }

        Vector3 finalPos = item.transform.position;
        RaycastHit hit;
        if (Physics.Raycast(finalPos + Vector3.up, Vector3.down, out hit, 2f))
        {
            item.transform.position = new Vector3(finalPos.x, hit.point.y + 0.05f, finalPos.z);
        }
    }

    public void SetupDefaultDrops(EnemyType enemyType)
    {
        dropItems.Clear();

        switch (enemyType)
        {
            case EnemyType.Common:
                break;
            case EnemyType.Elite:
                break;
            case EnemyType.Boss:
                break;
        }
    }
}
