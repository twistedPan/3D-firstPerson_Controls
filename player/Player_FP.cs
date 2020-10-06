using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_FP : MonoBehaviour
{
    public float jumpForce = 8.0f;
    public float lowJumpGrav = 2.0f;
    public float bigJumpGrav = 2.5f;
    public float speed = 10.0f;
    public bool flying = false;
    public bool isGrounded = true;
    public bool isJumping = false;
    public bool isSprinting = false;
    public bool isCarrying = false;
    public int playerState = 0;

    private Rigidbody rb;    
    private PlayerController controls;
    private Transform cam;
    private Vector2 movement;
    private bool isMoving;    
    private bool isInteracting = false;
    //private int groundedColliders = 0;
    private float flyValue = 0.0f;
    private float turnSmoothTime = 0.1f;
    private float turnSmoothVeloc;
    private Transform carriedObj;
    private Quaternion carriedObjRot;
    private Vector3 origin;

    private void Awake() 
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main.transform;

        controls = new PlayerController();
        controls.Player.Move.performed += ctx => movement = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += _ => movement = new Vector2(0,0);

        controls.Player.Fly.performed += ctx => flyValue = ctx.ReadValue<float>();
        controls.Player.Fly.canceled += _ => flyValue = 0;

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
    }

    void FixedUpdate()
    { 
        //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 10, Color.yellow);

        float horizontal = movement.x;
        float vertical = movement.y;
        Vector3 direction = new Vector3(horizontal, 0.0f, vertical);
        isGrounded = GroundCheck();
        //Debug.Log("Input Direction: " + direction);

        if (flying) 
        {
            rb.velocity = new Vector3(horizontal*20f, flyValue*20f, vertical*20f);
            if(GetComponent<Rigidbody>().useGravity == true) GetComponent<Rigidbody>().useGravity = false;

        } else // normal Movement
        { 
            // Jumping & Falling
            if (rb.velocity.y < 0) // -> big Jump
            {
                rb.velocity += Vector3.up * Physics.gravity.y * (bigJumpGrav - 1) * Time.deltaTime;
            } 
            else if (rb.velocity.y > 0 && !isJumping) // -> Fall
            {
                rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpGrav - 1) * Time.deltaTime;
            }

            // Walking & Orientation
            if (direction.magnitude == 0) // -> stand still & look with cam
            { 
                // Look at along direction of cam
                if(!isCarrying) 
                {
                    Vector3 relativPos = cam.position - transform.position;
                    relativPos.y = 0.0f;
                    Quaternion rotation = Quaternion.LookRotation(-relativPos, Vector3.up);
                    transform.rotation = rotation;
                }

                rb.velocity = new Vector3(0f, rb.velocity.y, 0f); // stops gliding 

            } 
            else if (direction.magnitude >= 0.1f) // -> move & free cam
            {
                // Look at along movement
                float targetAngle = Mathf.Atan2(direction.x,direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVeloc, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
                
                Vector3 moveDir = Quaternion.Euler(0f,targetAngle, 0f) * Vector3.forward;
                rb.velocity = new Vector3(moveDir.x * speed, rb.velocity.y, moveDir.z * speed);
            }

            // Moving States
            if (!isMoving && isGrounded && !isJumping) playerState = 0; // idle
            else if (isMoving && isGrounded && !isJumping && isSprinting) playerState = 3; // sprinting
            else if (isMoving && isGrounded && !isJumping) playerState = 1; // walking
            else if (!isGrounded) playerState = 2; // inAir

        }
        

        // Move lifted Object with Player
        if (isCarrying) 
        {
            //Debug.Log("Player: " + transform.forward);
            Vector3 objSize = carriedObj.transform.localScale;
            float distance = objSize.x > objSize.z ? objSize.x : objSize.z;
            if (distance < 2) distance = 2f;
            Vector3 facing = transform.position + 
            Vector3.Scale(transform.forward,new Vector3(distance/1.2f,0,distance/1.2f));
            carriedObj.position = new Vector3(facing.x, objSize.y/2 + facing.y - 1, facing.z);
            carriedObj.rotation = transform.rotation;
            //carriedObj.rotation = Quaternion.Euler(carriedObjRot.x, carriedObjRot.y, carriedObjRot.z);
        }

        // if fall of map spawn at origin
        if (transform.position.y <= -10)
        {
            transform.position = origin;
        }
    }

    
    // Interact
    public void Interact() {
        isInteracting = true;
        if (isCarrying) isCarrying = false;
        else LiftCheck();

        isInteracting = false;
    }


    // check if liftable Object is infront of player ? lift : nothing
    void LiftCheck()
    {
        Vector3 playerPos = transform.position + new Vector3(0,0,0);
        RaycastHit hit;
        if (Physics.Raycast(playerPos, transform.TransformDirection(Vector3.forward), out hit, 2.0f))
        {
            //Debug.Log(hit + " isCarrying: " + isCarrying);
            Debug.DrawRay(playerPos, transform.TransformDirection(Vector3.forward) * 10, Color.yellow);
             
            if (hit.transform.gameObject.CompareTag("Interactable")) 
            {
                if (hit.transform.gameObject.GetComponent<Moveable>().isLiftable && !isCarrying)
                {
                    if (isInteracting && !isCarrying) CarryObject(hit.transform.gameObject.transform);
                }
            } 
        }
    }
    // Sets object to lifting
    private void CarryObject(Transform target) 
    {
        //Debug.Log("is Liftig");
        carriedObj = target;
        carriedObjRot = carriedObj.rotation;
        isCarrying = true;
        //target.position = transform.position + new Vector3(1f, 2f, 1f);
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
        Physics.Raycast(transform.position, Vector3.down, out hit, 1f);
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


    // COLLISIONS
    private void OnCollisionEnter(Collision other) 
    {
        /* 
        if (other.gameObject.CompareTag("Obstacle")) 
        {
            //Debug.Log("Collision with Obstacle!");
            rb.AddForce(new Vector3(0,6,-10), ForceMode.Impulse);
        } 
        */
    }
    private void OnCollisionExit(Collision other) 
    {

    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();
}
