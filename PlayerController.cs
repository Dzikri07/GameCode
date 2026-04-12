using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpForce = 10f;

    [Header("Double Jump")]
    public int maxJumps = 2; // ganti ke 1 kalau mau single jump
    private int jumpCount;

    [Header("Stats")]
    public int maxHP = 100;
    private int currentHP;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded;
    private bool isDead;
    private bool isAttacking;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentHP = maxHP;
    }

    void Update()
    {
        if (isDead) return;

        HandleMovement();
        HandleJump();
        HandleAttack();
    }

    void HandleMovement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && moveInput != 0;
        bool isWalking = !isRunning && moveInput != 0;

        float speed = isRunning ? runSpeed : walkSpeed;
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);

        anim.SetBool("isRunning", isRunning);
        anim.SetBool("isWalking", isWalking);

        if (moveInput > 0) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        if (moveInput < 0) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
    }

    void HandleJump()
    {
        // Reset jump saat menyentuh tanah
        if (isGrounded) jumpCount = 0;

        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpCount++;
            anim.SetBool("isJumping", true);
        }
    }

    void HandleAttack()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Z)) && !isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        anim.SetBool("isAttacking", true);

        yield return new WaitForSeconds(0.5f);

        isAttacking = false;
        anim.SetBool("isAttacking", false);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        Debug.Log("HP: " + currentHP);

        if (currentHP <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;
        rb.velocity = Vector2.zero;
        anim.SetBool("isDead", true);
        this.enabled = false;
        Debug.Log("Player Mati!");
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            anim.SetBool("isJumping", false);
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}
