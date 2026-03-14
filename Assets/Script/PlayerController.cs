using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    CharacterController characterController;
    public float movementSpeed = 5;
    public float sprint = 3;
    float speed;

    // gravity settings (CharacterController doesn't apply gravity automatically)
    public float gravity = -9.81f;
    float verticalVelocity;

    public Animator animator;
    public readonly string moveAnimParameter = "Move";

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
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(moveX, 0, moveZ);

        float moveAnim = new Vector2(moveX, moveZ).magnitude;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = movementSpeed + sprint;
            moveAnim *= 3f;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            speed = movementSpeed - 3;
            moveAnim *= 0.5f;
        }
        else
        {
            speed = movementSpeed;
        }

        characterController.Move(move * speed * Time.deltaTime);
        animator.SetFloat(moveAnimParameter, moveAnim);

        if (moveX == 0 && moveZ == 0) return;
        float heading = MathF.Atan2(moveX, moveZ);
        transform.rotation = Quaternion.Euler(0, heading * Mathf.Rad2Deg, 0);
    }
}