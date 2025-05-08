using Cysharp.Threading.Tasks;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    bool mCursorShown;


    private void LateUpdate()
    {
        // 디버깅 용
        if (UnityEngine.Input.GetKeyDown(KeyCode.BackQuote))
        {
            toggleCursor(!mCursorShown);
        }
    }
    
    void toggleCursor(bool show)
    {
        mCursorShown = show;

        if (show)
        {
            Cursor.lockState = CursorLockMode.None;  // 1) 먼저 락 해제
            Cursor.visible = true;                 // 2) 그다음 보이게
        }
        else
        {
            Cursor.visible = false;                // 1) 먼저 숨김
            Cursor.lockState = CursorLockMode.Locked;// 2) 그다음 잠금
        }
    }
}
