using UnityEngine;


// Abyss -> Town 이동 시 플레이어 스폰 위치 조정을 위한 임시 스크립트
public class PlayerSpawnPositionController : MonoBehaviour
{
    void Awake()
    {
        TempRefManager.Instance.Player.transform.position = transform.position;
    }
    
}