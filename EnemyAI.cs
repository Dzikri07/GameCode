using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float patrolSpeed = 2f;
    public float patrolDistance = 5f;

    [Header("Chase Settings")]
    public float chaseSpeed = 4f;
    public float detectionRadius = 5f;
    public float loseRadius = 7f;

    [Header("Jump Settings")]
    public bool canJump = true;
    public float jumpForce = 8f;
    public LayerMask groundLayer;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    private Rigidbody2D rb;
    private Transform player;
    private Vector2 startPosition;
    private bool isFacingRight = true;
    private bool isGrounded;

    private float flipCooldown = 0f;
    private const float FLIP_COOLDOWN_TIME = 0.4f;

    private float jumpCooldown = 0f;
    private const float JUMP_COOLDOWN_TIME = 1.2f;

    // Batas patroli kiri & kanan dalam world space — dihitung di Awake
    private float patrolLeft;
    private float patrolRight;

    private enum State { Patrolling, Chasing, Returning }
    private State currentState = State.Patrolling;

    // ─────────────────────────────────────────
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 3f;

        // Simpan posisi awal di Awake supaya pasti benar sebelum Start lain jalan
        startPosition = transform.position;

        // Hitung batas patroli langsung di sini — tidak bergantung inspector lagi
        patrolLeft  = startPosition.x - patrolDistance;
        patrolRight = startPosition.x + patrolDistance;
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // Auto cari / buat GroundCheck
        if (groundCheck == null)
        {
            Transform existing = transform.Find("GroundCheck");
            if (existing != null)
                groundCheck = existing;
            else
            {
                GameObject gc = new GameObject("GroundCheck");
                gc.transform.SetParent(transform);
                gc.transform.localPosition = new Vector3(0f, -0.55f, 0f);
                groundCheck = gc.transform;
            }
        }
    }

    // ─────────────────────────────────────────
    void Update()
    {
        if (flipCooldown > 0f) flipCooldown -= Time.deltaTime;
        if (jumpCooldown > 0f) jumpCooldown -= Time.deltaTime;

        CheckGrounded();

        // Kalau player belum ditemukan, coba cari lagi
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            DoPatrol();
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        float distToStart  = Vector2.Distance(transform.position, startPosition);

        switch (currentState)
        {
            case State.Patrolling:
                DoPatrol();
                if (distToPlayer <= detectionRadius)
                    currentState = State.Chasing;
                break;

            case State.Chasing:
                DoChase();
                if (distToPlayer > loseRadius)
                    currentState = State.Returning;
                break;

            case State.Returning:
                DoReturn();
                if (distToStart < 0.3f)
                {
                    rb.velocity = new Vector2(0f, rb.velocity.y);
                    currentState = State.Patrolling;
                }
                // Kalau player datang lagi saat balik
                if (distToPlayer <= detectionRadius)
                    currentState = State.Chasing;
                break;
        }
    }

    // ─────────────────────────────────────────
    // PATROL — pakai world position batas kiri/kanan
    // ─────────────────────────────────────────
    void DoPatrol()
    {
        float posX = transform.position.x;

        // Balik arah kalau sudah sampai batas
        if (posX >= patrolRight && isFacingRight)
            TryFlip();
        else if (posX <= patrolLeft && !isFacingRight)
            TryFlip();

        // Balik kalau ada dinding atau tepi (dengan cooldown)
        if (flipCooldown <= 0f)
        {
            if (IsWallAhead() || IsEdgeAhead())
                TryFlip();
        }

        float dir = isFacingRight ? 1f : -1f;
        rb.velocity = new Vector2(dir * patrolSpeed, rb.velocity.y);
    }

    // ─────────────────────────────────────────
    // CHASE
    // ─────────────────────────────────────────
    void DoChase()
    {
        float dirX = player.position.x - transform.position.x;

        if (dirX > 0.05f && !isFacingRight) TryFlip();
        else if (dirX < -0.05f && isFacingRight) TryFlip();

        rb.velocity = new Vector2(Mathf.Sign(dirX) * chaseSpeed, rb.velocity.y);

        if (canJump && isGrounded && jumpCooldown <= 0f)
        {
            if (IsWallAhead())
                Jump();
            else if (player.position.y > transform.position.y + 1f)
                Jump();
        }
    }

    // ─────────────────────────────────────────
    // RETURN TO START
    // ─────────────────────────────────────────
    void DoReturn()
    {
        float dirX = startPosition.x - transform.position.x;

        if (dirX > 0.05f && !isFacingRight) TryFlip();
        else if (dirX < -0.05f && isFacingRight) TryFlip();

        rb.velocity = new Vector2(Mathf.Sign(dirX) * patrolSpeed, rb.velocity.y);

        if (canJump && isGrounded && jumpCooldown <= 0f && IsWallAhead())
            Jump();
    }

    // ─────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────
    void CheckGrounded()
    {
        if (groundCheck == null) return;
        if (groundLayer.value == 0)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius);
        else
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    bool IsWallAhead()
    {
        float dir = isFacingRight ? 1f : -1f;
        Vector2 origin = (Vector2)transform.position + Vector2.up * 0.1f;
        if (groundLayer.value == 0)
            return Physics2D.Raycast(origin, Vector2.right * dir, 0.4f);
        return Physics2D.Raycast(origin, Vector2.right * dir, 0.4f, groundLayer);
    }

    bool IsEdgeAhead()
    {
        float dir = isFacingRight ? 1f : -1f;
        Vector2 checkPos = (Vector2)transform.position + Vector2.right * dir * 0.4f + Vector2.down * 0.1f;
        if (groundLayer.value == 0)
            return !Physics2D.Raycast(checkPos, Vector2.down, 0.6f);
        return !Physics2D.Raycast(checkPos, Vector2.down, 0.6f, groundLayer);
    }

    void TryFlip()
    {
        if (flipCooldown > 0f) return;
        isFacingRight = !isFacingRight;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (isFacingRight ? 1f : -1f);
        transform.localScale = s;
        flipCooldown = FLIP_COOLDOWN_TIME;
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpCooldown = JUMP_COOLDOWN_TIME;
    }

    // ─────────────────────────────────────────
    // GIZMOS
    // ─────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Vector3 origin = Application.isPlaying ? (Vector3)startPosition : transform.position;

        // Detection & lose radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRadius);

        // Patrol range
        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin + Vector3.left * patrolDistance, origin + Vector3.right * patrolDistance);

        // Ground check
        if (groundCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
