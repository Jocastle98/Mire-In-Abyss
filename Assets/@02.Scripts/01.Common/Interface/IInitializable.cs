using Cysharp.Threading.Tasks;

/// 전역 매니저·DB가 지켜야 할 최소 규약
public interface IInitializable
{
    UniTask InitializeAsync();         // 완료될 때 “이 모듈은 준비 끝”
}
