public interface IPlayerState
{
    // 해당 상태로 진입했을 때 호출되는 메서드
    void OnEnter(PlayerController playerController);
    
    // 해당 상태에 머물러 있을 때 Update 주기로 호출되는 메서드
    void OnUpdate();
    
    // 해당 상태에서 빠져 나갈 때 호출되는 메서드
    void OnExit();
}
