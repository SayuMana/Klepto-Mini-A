using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorManager : MonoBehaviour
{
    public GameObject door;
    private bool playerInRange = false;

    private Vector3 closedPos;
    private Vector3 openPos;

    public float closedPosY;
    public float speedDoor;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void Start()
    {
        closedPos = door.transform.localPosition;
        openPos = closedPos + new Vector3(0, closedPosY, 0);
    }

    void Update()
    {
        if (playerInRange)
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
