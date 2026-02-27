using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hiding : MonoBehaviour
{
    public GameObject player;        
    public CameraFollow cameraFollow;  
    public Transform hidingSpot;

    private bool playerInRange = false;
    private bool hiding = false;
    public float zoom;

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

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleHide();
        }
    }

    void ToggleHide()
    {
        hiding = !hiding;

        if (hiding)
        {
            cameraFollow.playerTarget = hidingSpot;
            cameraFollow.offset = new Vector3(
                cameraFollow.offset.x,
                cameraFollow.offset.y - zoom,
                cameraFollow.offset.z
            );
            player.SetActive(false);
        }
        else
        {
            player.SetActive(true);
            cameraFollow.playerTarget = player.transform;
            cameraFollow.offset = new Vector3(
                cameraFollow.offset.x,
                cameraFollow.offset.y + zoom,
                cameraFollow.offset.z
            );
        }
    }
}