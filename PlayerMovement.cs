using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody2D))]

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 10f;

    private Rigidbody2D rb;
    private Vector2 currentInput;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    
    }

    // Dipanggil otomatis oleh PlayerInput saat ada input gerak
    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        currentInput = input;
    }

    // Dipanggil otomatis oleh PlayerInput saat tombol jump
    public void OnJump(InputValue value)
    {
        
        if (rb == null) return;
        
        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }
    void Update()
    {
        // Gerak horizontal
        rb.velocity = new Vector2(currentInput.x * speed, rb.velocity.y);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}