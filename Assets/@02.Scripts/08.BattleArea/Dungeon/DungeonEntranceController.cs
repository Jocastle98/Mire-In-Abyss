using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DungeonEntranceController : MonoBehaviour
{
    public DungeonRoomController roomCon;
    public GameObject door;
    public GameObject block;
    public GameObject testPlayer;
    public bool testbool = false;
    private bool testBool = false;

    private void Update()
    {
        if (testbool == true && testBool == false)
        {
            testBool = true;
            playerEnter.Invoke(testPlayer.transform);
        }
    }

    public delegate void PlayerEnter(Transform playerPos);

    PlayerEnter playerEnter;

    private Quaternion originRot;
    public bool IsClear;

    public void EntranceInit(DungeonRoomController roomCon)
    {
        this.roomCon = roomCon;
        IsClear = roomCon.IsClear;

        foreach (Transform trans in transform)
        {
            if (trans.name.Contains("Door", System.StringComparison.OrdinalIgnoreCase))
            {
                door = trans.gameObject;
                originRot = door.transform.rotation;
            }

            if (trans.name.Contains("Block", System.StringComparison.OrdinalIgnoreCase))
            {
                block = trans.gameObject;
            }
        }

        roomCon.RegisterEntrancePrefab(gameObject, this,
            () => EntranceClose(door, block), () => EntranceOpen(door, block));
        EntranceOpen(door, block);
    }

    public void RegisterSpawnFunc(PlayerEnter func)
    {
        playerEnter -= func;
        playerEnter += func;
    }

    void EntranceOpen(GameObject door, GameObject block)
    {
        door.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        block.gameObject.SetActive(false);
    }

    void EntranceClose(GameObject door, GameObject block)
    {
        if (IsClear) return;
        door.transform.rotation = originRot;
        block.gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsClear && other.name.Contains("Player", System.StringComparison.OrdinalIgnoreCase))
        {
            playerEnter.Invoke(other.gameObject.transform);
        }
    }
}
