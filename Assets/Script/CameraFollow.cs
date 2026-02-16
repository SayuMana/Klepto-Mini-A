using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform playerTarget;
    public float cameraSpeed;
    public Vector3 offset;

    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, playerTarget.position + offset, cameraSpeed * Time.deltaTime);
    }
}