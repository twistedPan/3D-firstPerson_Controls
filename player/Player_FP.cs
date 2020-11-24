using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_FP : MonoBehaviour
{
    [SerializeField] private float speed = 250.0f;
    [SerializeField] private float jumpForce = 6.0f;
    [SerializeField] private float lowJumpGrav = 4.0f;
    [SerializeField] private float bigJumpGrav = 2.5f;
    [SerializeField]private float envSpeedVal = 1.0f;
    [SerializeField]private float stepCycle = 0.0f;
    [SerializeField]private float nextStep = 0.0f;
    [SerializeField]private float runstepInterval = 0.5f;
    [SerializeField]private float stepInterval = 10.0f;
    [SerializeField]private float death_Depht = -50.0f;
    public bool isGrounded = true;
    public bool isJumping = false;
    public bool isSprinting = false;
    public bool isWalking = false;
    public bool isCarrying = false;
    public bool playerIsStopped = false;
    public int playerState = 0;
    public float playerHeight;
    public Vector3 lookDirection;

    private Rigidbody rb;    
    private PlayerController controls;
    private FirstPersonCamera fpCam;
    private Transform cam;
    private Vector2 movement;
    private bool isMoving;    
    private bool isInteracting = false;
    private Vector3 origin;
    //private float turnSmoothTime = 0.1f;
    //private float turnSmoothVeloc;

    private void Awake() 
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main.transform;
        fpCam = Camera.main.GetComponent<FirstPersonCamera>();

        controls = new PlayerController();
        controls.Player.Move.performed += ctx => movement = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += _ => movement = new Vector2(0,0);

        controls.Player.Sprint.performed += _ => Sprint();
        controls.Player.Sprint.canceled += _ => Sprint();

        controls.Player.Look.started += ctx => FindObjectOfType<FirstPersonCamera>().Look(ctx.ReadValue<Vector2>());

        controls.Player.Zoom.performed += ctx => FindObjectOfType<FirstPersonCamera>().ZoomCam(ctx.ReadValue<Vector2>());
        //controls.Player.Zoom.performed += ctx => FindObjectOfType<FirstPersonCamera>().ZoomCam(ctx.ReadValue<Vector2>());

        controls.Player.Action.performed += _ => Interact();
        controls.Player.Action.canceled += _ => isInteracting = false;

        controls.Player.Move.performed += _ => isMoving = true; 
        controls.Player.Move.canceled += _ => isMoving = false;

        controls.Player.Jump.performed += _ => Jump(); // sJumping = true; // Jump(); // 
        controls.Player.Jump.canceled += _ => isJumping = false;

        origin = transform.position;
        playerHeight = transform.localScale.y*0.93f;
    }

    void FixedUpdate()
    { 
        //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 10, Color.yellow);

        Vector3 direction = new Vector3(movement.x, 0.0f, movement.y);
        //Debug.Log("Input Direction: " + direction);
        
        Vector3 moveDir = new Vector3(0.0f, 0.0f, 0.0f);
        
        isWalking = movement.magnitude > 0 ? true : false;

        isGrounded = GroundCheck();

        if (direction == Vector3.zero) {stepCycle = nextStep = 0;}

        // Jumping & Falling
        if (rb.velocity.y < 0) // -> big Jump
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (bigJumpGrav - 1) * Time.deltaTime;
        } 
        else if (rb.velocity.y > 0 && !isJumping) // -> Fall
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpGrav - 1) * Time.deltaTime;
        }

        float targetAngle = Mathf.Atan2(direction.x,direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0f, cam.eulerAngles.y, 0f);

        // Walk Direction & Orientation
        if (direction.magnitude == 0) // -> stand still
        { 
            moveDir = Quaternion.Euler(0f,targetAngle, 0f) * Vector3.zero;
        } 
        else if (direction.magnitude > 0) // -> move
        {
            moveDir = Quaternion.Euler(0f,targetAngle, 0f) * Vector3.forward;
        }    


        if(!playerIsStopped) 
        {
            float movingSpeed = speed * envSpeedVal * Time.deltaTime;
            rb.velocity = new Vector3(moveDir.x * movingSpeed, rb.velocity.y, moveDir.z * movingSpeed);
        }


        if (isGrounded && !playerIsStopped && isWalking) StepCycle(speed);

        // Moving States
        if (!isMoving && isGrounded && !isJumping) playerState = 0; // idle
        else if (isMoving && isGrounded && !isJumping && isSprinting) playerState = 3; // sprinting
        else if (isMoving && isGrounded && !isJumping) playerState = 1; // walking
            else if (!isGrounded) playerState = 2; // inAir

        

        // if fall of map spawn at origin
        if (transform.position.y <= death_Depht)
        {
            transform.position = origin;
        }
    }

    
    // Interact
    public void Interact() 
    {
        //interactionCheck.ActionCheck();
    }


    void Jump()
    {
        if (GroundCheck())
        {
            rb.velocity = Vector3.up * jumpForce;
            isJumping = true;
        } else 
            isJumping = false;
    }

    bool GroundCheck()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down, out hit, playerHeight+0.5f);
        return hit.collider != null;
    }

    void Sprint() 
    {
        if (isSprinting) 
        {
            speed/=2;
            isSprinting = false;
        } else 
        {
            speed*=2; 
            isSprinting = true;
        }
    }

    void StepCycle(float speed)
    {
        if (movement.magnitude > 0) 
        {
            stepCycle += (movement.magnitude + (speed*(isSprinting ? runstepInterval : 1f))/10)*
                             Time.fixedDeltaTime;
        }
        else if (stepCycle > 1)
        {
            stepCycle = 0;
            nextStep = 0;
        }

        float stepProgressAmount = (nextStep - stepCycle) / stepInterval;
        fpCam.HeadMove(stepProgressAmount, isSprinting);
        
        if (!(stepCycle > nextStep))
        {
            return;
        }

        nextStep = stepCycle + stepInterval;
        
        
        PlayFootStep();
    }

    void PlayFootStep()
    {
        // Play Footstep Audio
    }

    public void TogglePlayerMovement()
    {
        if (playerIsStopped) playerIsStopped = false;
        else playerIsStopped = true;
    }


    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();
}
