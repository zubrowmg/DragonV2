using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class testMovement : MonoBehaviour
{
    public InputAction playerControls;
    public Rigidbody2D rb;

    Vector2 moveDir = Vector2.zero;

    public float speed = 30f;
    void Update()
    {
        moveDir = playerControls.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(moveDir.x * speed, moveDir.y * speed);
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }
}
