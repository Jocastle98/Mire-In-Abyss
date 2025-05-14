using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TorchController : MonoBehaviour
{
    Light torchLight;
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "LightsSensor")
        {
            if (torchLight == null) GetTorchLight();
            torchLight.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "LightsSensor")
        {
            torchLight.enabled = false;
        }
    }

    private void GetTorchLight()
    {
        torchLight = gameObject.GetComponent<Light>();
        torchLight.enabled = false;
    }
}
