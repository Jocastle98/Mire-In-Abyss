using System;
using Cysharp.Threading.Tasks;

public sealed class SettingTabPanelPresenter : TabPanelPresenter
{
    public override UniTask Hide(Action onComplete = null)
    {
        // UserData.Instance.Save();
        return base.Hide(onComplete);
    }
}
