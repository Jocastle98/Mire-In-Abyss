using UnityEngine;

public class InputBuffer
{
    private float mBufferTime;
    private float mTimer;
    private bool mbIsBuffered;
    private bool mbIsHolding;

    public InputBuffer(float bufferTime = 0.3f)
    {
        mBufferTime = bufferTime;
    }

    public void SetBuffer()
    {
        mbIsBuffered = true;
        mTimer = mBufferTime;
    }

    public void SetHold(bool isPressed)
    {
        mbIsHolding = isPressed;
    }

    public void OnBufferUpdate(float deltaTime)
    {
        if (mbIsBuffered)
        {
            mTimer -= Time.deltaTime;
            if (mTimer <= 0.0f)
            {
                mbIsBuffered = false;
            }
        }
    }

    public bool ConsumeInputBuffer()
    {
        if (mbIsBuffered)
        {
            mbIsBuffered = false;
            return true;
        }
        
        return false;
    }
    
    public bool IsBuffered => mbIsBuffered;
    public bool IsHolding => mbIsHolding;
}