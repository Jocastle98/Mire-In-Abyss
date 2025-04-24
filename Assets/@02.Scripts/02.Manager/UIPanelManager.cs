using System.Collections;
using System.Collections.Generic;
using UIPanelEnums;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIPanelManager : Singleton<UIPanelManager>
{
    [System.Serializable]
    public class PanelInfo
    {
        public UIPanelType type;
        public PopupPanelController panelPrefab;
    }

    [SerializeField] private List<PanelInfo> panels = new List<PanelInfo>();
    [SerializeField] private Transform panelParent;
    private Stack<PopupPanelController> activePopups = new Stack<PopupPanelController>();
    
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CloseAllPanels();
    }

    public void OpenPanel(UIPanelType type)
    {
        PanelInfo panelInfo = panels.Find(p => p.type == type);
        if (panelInfo != null && panelInfo.panelPrefab != null)
        {
            PopupPanelController panelInstance = Instantiate(panelInfo.panelPrefab, panelParent);
            panelInstance.Show();
        }
    }

    public void CloseAllPanels()
    {
        while (activePopups.Count > 0)
        {
            activePopups.Pop().Hide();
        }
    }

    public void PushPopup(PopupPanelController popup)
    {
        activePopups.Push(popup);
    }

    public void PopPopup(PopupPanelController popup)
    {
        if (activePopups.Count > 0 && activePopups.Peek() == popup)
        {
            activePopups.Pop();
        }
    }
}
