using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PalyerMovement : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float turnSpeed = 100f;


    void Update()
    {
        if (Input.GetKey(KeyCode.W))
            transform.position += Vector3.forward * moveSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.S))
            transform.position += -Vector3.forward * moveSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.A))
            transform.Rotate(Vector3.up, -turnSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.D))
            transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime);
    }
}