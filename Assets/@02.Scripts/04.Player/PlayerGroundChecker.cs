using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundChecker : MonoBehaviour
{
    // 겹쳐진 땅에서도 땅 접촉 확인을 위한 변수
    public bool bIsGrounded => groundCheckCount > 0;
    private int groundCheckCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            groundCheckCount++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            groundCheckCount = Mathf.Max(0, groundCheckCount - 1);
        }
    }
}