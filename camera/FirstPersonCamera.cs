using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public Transform lookAt;
    public Transform camTransform;

    private Camera cam;
    private Player_FP player;

    [SerializeField]private float headBobtAmt = 0.1f;
    [SerializeField] private float camSensitivityX = 3;
    [SerializeField] private float camSensitivityY = 3;

    private float distance = 0.01f;
    private float currentX = 0.0f;
    private float currentY = 0.0f;
    private float playerHeight;
    private float headMovement = 0.0f;    

    private void Start()
    {
        cam = Camera.main;
        //camTransform = transform;
        playerHeight = lookAt.localScale.y;
        player = lookAt.GetComponent<Player_FP>();
    }

    private void LateUpdate() 
    {
        Vector3 playerHead = new Vector3(lookAt.position.x, lookAt.position.y+(playerHeight*0.93f) + headMovement, lookAt.position.z);
        Vector3 dir = new Vector3(0,0,-distance);

        // no neck breaking
        currentX = Mathf.Clamp(currentX, -85, 85);

        Quaternion rotation = Quaternion.Euler(currentX,currentY,0);
        camTransform.position = playerHead + rotation * dir;
        camTransform.LookAt(playerHead);
        player.lookDirection = rotation * Vector3.forward;
    }

    public void Look(Vector2 v) 
    {
        
        // Don't divide with zero
        camSensitivityX = camSensitivityX == 0 ? 3 : camSensitivityX;
        camSensitivityY = camSensitivityY == 0 ? 3 : camSensitivityY;
        
        currentX -= v.y * (1/camSensitivityX);
        currentY += v.x * (1/camSensitivityY);
    }

    public void ZoomCam(Vector2 v) {
        Vector2 vN = v.normalized;
        //Debug.Log("Zoom " + vN + " Distance = " + distance);
        distance += vN.y/5;
        distance = Mathf.Clamp(distance, 0.1f, 6);
    }

    public void HeadMove(float amount, bool sprinting) {
        //Debug.Log(sprinting);
        amount = Mathf.Clamp(amount, 0,1);

        float stepLerp = _Utilities.mapRange(amount, 0,1, Mathf.PI,Mathf.PI*2);
        float sinMove = Mathf.Sin(stepLerp); 
        float headBob = sprinting ? 0.25f : headBobtAmt;
        float camLerp = _Utilities.mapRange(sinMove, -1,1, -headBob,headBob);

        headMovement = Mathf.Sin(camLerp);
    }
}
