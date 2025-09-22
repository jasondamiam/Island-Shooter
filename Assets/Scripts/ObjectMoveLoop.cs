using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ObjectMoveLoop : MonoBehaviour
{
    public float speed = 3f;       // Movement speed
    public float sideLength = 5f;  // How long each side of the square is

    private Vector3 startPos;
    private int direction = 0;     // 0 = right, 1 = up, 2 = left, 3 = down
    private float distanceMoved = 0f;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        Vector3 moveDir = Vector3.zero;

        // Choose movement direction based on current side
        if (direction == 0) moveDir = Vector3.right;
        else if (direction == 1) moveDir = Vector3.forward;  // forward = "up" in 3D
        else if (direction == 2) moveDir = Vector3.left;
        else if (direction == 3) moveDir = Vector3.back;

        // Move the object
        float moveStep = speed * Time.deltaTime;
        transform.Translate(moveDir * moveStep, Space.World);

        distanceMoved += moveStep;

        // If finished one side of the square
        if (distanceMoved >= sideLength)
        {
            distanceMoved = 0f;
            direction = (direction + 1) % 4; // Go to next side
        }
    }
}

