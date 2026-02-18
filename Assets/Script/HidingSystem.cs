using UnityEngine;

public class HidingSystem : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode hidingKey = KeyCode.E;
    
    [Header("Status")]
    public bool isHiding = false;
    public GameObject currentHidingSpot; 

    private PlayerController playerController;
    private CameraFollow cameraFollow;
    
    private MeshRenderer playerMesh; 

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        
        playerMesh = GetComponent<MeshRenderer>(); 
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
        
        playerController.enabled = false;
        playerMesh.enabled = false; 

        if(cameraFollow != null) cameraFollow.enabled = false;

        Camera.main.transform.position = currentHidingSpot.transform.position + new Vector3(0, 2, -4);
        Camera.main.transform.LookAt(currentHidingSpot.transform);

        Debug.Log("Sembunyi...");
    }

    void ExitHiding()
    {
        isHiding = false;

        playerController.enabled = true;
        playerMesh.enabled = true; 

        if(cameraFollow != null) cameraFollow.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HidingSpot")) currentHidingSpot = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("HidingSpot")) currentHidingSpot = null;
    }
}