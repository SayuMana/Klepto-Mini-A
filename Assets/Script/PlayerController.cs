using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;

public class PlayerController : MonoBehaviour
{
    CharacterController characterController;
    public float movementSpeed;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        Movement();
    }

    void Movement()
    {
        float moveX = -Input.GetAxis("Horizontal");
        float moveZ = -Input.GetAxis("Vertical");
        Vector3 move = new Vector3(moveX, 0, moveZ);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            characterController.Move(move * (movementSpeed + 5) * Time.deltaTime); // sprint
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            characterController.Move(move * (movementSpeed - 5) * Time.deltaTime); // sneak
        }
        characterController.Move(move * movementSpeed * Time.deltaTime);

        if (moveX == 0 && moveZ == 0) return;
        float heading = MathF.Atan2(moveX, moveZ);
        transform.rotation = Quaternion.Euler(0, heading * Mathf.Rad2Deg, 0);
    }


}
