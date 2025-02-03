using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float rotationSpeed = 5.0f;
    public GameObject target;
    private Vector3 originalPos;
    private float mouseY;
    private float rotationY = 0f; // Added to store the accumulated vertical rotation
    public float minY = -60f; // Minimum vertical angle
    public float maxY = 80f; // Maximum vertical angle

    // Start is called before the first frame update
    void Start()
    {
        // Optional: Initialize rotationY with the current rotation to prevent jumps in camera angle at start
        Vector3 angles = transform.eulerAngles;
        rotationY = angles.x;
        originalPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
        mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // Calculate new rotation, clamping in the process
        rotationY += mouseY;
        rotationY = Mathf.Clamp(rotationY, minY, maxY);

        // Apply the calculated and clamped rotation along the X axis for vertical tilt,
        // while keeping the current Y (horizontal) and Z (roll) angles the same.
        transform.position = target.transform.position + originalPos; // Follow the target
        transform.rotation = Quaternion.Euler(-rotationY, target.transform.eulerAngles.y, 0);
        
        
    }



}