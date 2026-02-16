using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class HidingSystem : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode hidingKey = KeyCode.E;
    
    [Header("Status")]
    public bool isHiding = false;
    private GameObject currentHidingSpot;

    private PlayerController playerController;
    private CameraFollow cameraFollow;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
    }

    void Update()
    {
        
        if (Input.GetKeyDown(hidingKey) && currentHidingSpot != null)
        {
            if (!isHiding) EnterHiding();
            else ExitHiding();
        }
    }

    void EnterHiding()
    {
        isHiding = true;
        
       
        Vector3 targetPos = currentHidingSpot.transform.position;
        targetPos.y = transform.position.y; 
        transform.position = targetPos;

        
        playerController.enabled = false;
        if(cameraFollow != null) cameraFollow.enabled = false;

        //Debug.Log("Sembunyi di: " + currentHidingSpot.name);
    }

    void ExitHiding()
    {
        isHiding = false;

        
        playerController.enabled = true;
        if(cameraFollow != null) cameraFollow.enabled = true;

      //  Debug.Log("Keluar dari tempat sembunyi");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HidingSpot"))
        {
            currentHidingSpot = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("HidingSpot"))
        {
            currentHidingSpot = null;
        }
    }
}

