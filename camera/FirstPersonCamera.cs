using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public Transform lookAt;
    public Transform camTransform;

    private Camera cam;

    private float distance = 0.01f;
    private float currentX = 0.0f;
    private float currentY = 0.0f;
    private float playerHeight;
    
    //private float sensitivityX = 2.0f;
    //private float sensitivityY = 0.5f;

    private void Start()
    {
        cam = Camera.main;
        //camTransform = transform;
        playerHeight = lookAt.localScale.y;
    }

    private void LateUpdate() 
    {
        Vector3 playerHead = new Vector3(lookAt.position.x, lookAt.position.y+playerHeight/4, lookAt.position.z);
        Vector3 dir = new Vector3(0,0,-distance);
        Quaternion rotation = Quaternion.Euler(currentX,currentY,0);
        camTransform.position = playerHead + rotation * dir;
        camTransform.LookAt(playerHead);
    }

    public void Look(Vector2 v) 
    {
        //Debug.Log("Look: " + v);
        currentX -= v.y;    
        currentY += v.x;
    }

    public void ZoomCam(Vector2 v) {
        Vector2 vN = v.normalized;
        Debug.Log("Zoom " + vN + " Distance = " + distance);
        distance += vN.y/5;
        distance = Mathf.Clamp(distance, 0.1f, 6);
    }
}
