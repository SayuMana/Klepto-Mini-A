using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorManager : MonoBehaviour
{
    public GameObject door;

    public string requiredKeyID;
    public int requiredAmount = 1;
    bool canOpen;

    private bool playerInRange = false;
    PlayerInventory playerInventory;

    private Vector3 closedPos;
    private Vector3 openPos;

    public float moveX, moveY;
    public float speedDoor;

    void Awake()
    {
        closedPos = door.transform.localPosition;
        openPos = closedPos + new Vector3(moveX, moveY, 0);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerInventory = other.GetComponent<PlayerInventory>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void Update()
    {
        if (playerInventory == null) return;  

        canOpen = true;
        if (!string.IsNullOrEmpty(requiredKeyID))
        {
            canOpen = playerInventory.HasKey(requiredKeyID, requiredAmount);
        }

        if (playerInRange && canOpen)
        {
            door.transform.localPosition = Vector3.Lerp(
                door.transform.localPosition,
                openPos,
                Time.deltaTime * speedDoor
            );
        }
        else
        {
            door.transform.localPosition = Vector3.Lerp(
                door.transform.localPosition,
                closedPos,
                Time.deltaTime * speedDoor
            );
        }
    }
}
