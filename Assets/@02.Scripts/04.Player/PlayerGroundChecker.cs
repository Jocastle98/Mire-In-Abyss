using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundChecker : MonoBehaviour
{
    // 겹쳐진 땅에서도 땅 접촉 확인을 위한 변수
    public bool bIsGrounded { get; private set; }
    private int mGroundCheckCount = 0;
    
    private float mGroundedTimeout = 0.2f;
    private float mLastGroundedTime;
    private float mCheckInterval = 0.1f;
    
    private void FixedUpdate()
    {
        // 타이머 기반 땅 확인
        bIsGrounded = mGroundCheckCount > 0 || (Time.time - mLastGroundedTime <= mGroundedTimeout);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            mGroundCheckCount++;
            mLastGroundedTime = Time.time; // 닿는 순간 타이머 갱신
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            mLastGroundedTime = Time.time; // 닿는 동안 타이머 갱신 유지
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            mGroundCheckCount = Mathf.Max(0, mGroundCheckCount - 1);
        }
    }
}