using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moveable : MonoBehaviour
{
    public bool isLiftable = true;
    public bool isCounted = false;
    private Rigidbody rb;
    private Player_FP player;
    private float weight;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        player = FindObjectOfType<Player_FP>();
    }

    private void Start() {
        weight = (transform.localScale.x + transform.localScale.y + transform.localScale.z) / 4;
    }

    private void Update() {
        if(rb.velocity.y < 0) {
            rb.velocity += Vector3.up * Physics.gravity.y * (weight - 1) * Time.deltaTime;
        }

        if(transform.position.y <= -20) 
        {
            Debug.Log("Object destroied");
            Destroy(transform.gameObject);
        }
    }

    private void OnCollisionEnter(Collision other) 
    {
        if (other.gameObject.CompareTag("Interactable")) // old Obstacle
        {
            //Debug.Log("Collision with Interactable!");
            if (player.isCarrying) player.isCarrying = false;
            rb.AddForce(new Vector3(0,8,-20), ForceMode.Impulse);
        }
    }
    private void OnCollisionExit(Collision other) 
    {
    }
}
