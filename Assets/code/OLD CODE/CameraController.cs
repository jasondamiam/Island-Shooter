using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform followTarget;
    [SerializeField] float rotationSpeed = 2f;
    [SerializeField] float distance = 5;
    [SerializeField] float maxDistance = 10;
    [SerializeField] float minDistance = 3;

    [SerializeField] float minVericalAngle = -45;
    [SerializeField] float maxVericalAngle = +45;

    [SerializeField] Vector2 framingOffset;

    [SerializeField] bool invertX;
    [SerializeField] bool invertY;



    float rotationX;
    float rotationY;

    float invertXVal;
    float invertYVal;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        invertXVal = (invertX) ? -1 : 1;
        invertYVal = (invertY) ? -1 : 1;

        rotationX += Input.GetAxis("Mouse Y") * invertYVal * rotationSpeed;
        rotationX = Mathf.Clamp(rotationX, minVericalAngle, maxVericalAngle );
        
        rotationY += Input.GetAxis("Mouse X") * invertXVal * rotationSpeed;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            distance = Math.Max(distance - scroll, minDistance);
        }
        else if (scroll < 0f)
        {
            distance = Math.Min(distance - scroll, maxDistance);
        }

        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);
        
        var focusPostion = followTarget.position + new Vector3(framingOffset.x, framingOffset.y);
        

        transform.position = focusPostion - targetRotation * new Vector3(0, 0, distance);
        transform.rotation = targetRotation;
    }

    public Quaternion GetPlanarRotation()
    {
        return Quaternion.Euler(0, rotationY, 0);
    }

    public void hoi() { }
}

