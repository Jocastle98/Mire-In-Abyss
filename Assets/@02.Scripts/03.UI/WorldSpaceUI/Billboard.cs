using UnityEngine;

public sealed class Billboard : MonoBehaviour
{
    Camera mCam;
    void Start() 
    {
        mCam = Camera.main;
    }
    void LateUpdate()
    {
        if (mCam) transform.forward = mCam.transform.forward;
    }
}
