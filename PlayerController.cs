using System.Collections;
using UnityEngine;
using UnityEngine.UI; // ← tambahkan ini

public class PlayerController : MonoBehaviour
{
    public GameObject slashPrefab;
    public Transform spawnPoint;

    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpForce = 10f;

    [Header("Double Jump")]
    public int maxJumps = 2;
    private int jumpCount;

    [Header("Stats")]
    public int maxHP = 100;
    public Scrollbar healthBar;
    private int currentHP;

    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;
    public float invincibleDuration = 1f;

    private bool isKnockedBack = false;
    private bool isInvincible = false;

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
        healthBar.size = (float)currentHP / maxHP; // huruf kecil 's'
    }

    void Update()
    {
        if (isDead) return;

        // Skip input saat kena knockback
        if (!isKnockedBack)
        {
            HandleMovement();
            HandleJump();
            HandleAttack();
        }

        CheckFallDeath();
    }

    void CheckFallDeath()
    {
        if (transform.position.y < -10f)
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        transform.position = Checkpoint.respawnPoint;
        rb.velocity = Vector2.zero;
        currentHP = maxHP;
        isDead = false;
        isKnockedBack = false;
        isInvincible = false;
        anim.SetBool("isDead", false);
        anim.SetBool("isJumping", false);
        healthBar.size = 1f; // ← reset health bar
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

    // ─────────────────────────────────────────────
    // Panggil dari Enemy script:
    //   player.TakeDamage(10, transform.position);
    // ─────────────────────────────────────────────
    public void TakeDamage(int damage, Vector2 enemyPosition)
    {
        if (isDead || isInvincible) return;

        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        
        healthBar.size = (float)currentHP / maxHP; 
        StartCoroutine(KnockbackCoroutine(enemyPosition));
        StartCoroutine(InvincibleCoroutine());

        if (currentHP <= 0)
            Die();
    }
    // Overload tanpa posisi musuh (knockback ke kiri default)
    public void TakeDamage(int damage)
    {
        // Knockback ke arah berlawanan dari hadap player
        Vector2 fallbackEnemyPos = (Vector2)transform.position + new Vector2(transform.localScale.x, 0);
        TakeDamage(damage, fallbackEnemyPos);
    }

    IEnumerator KnockbackCoroutine(Vector2 enemyPosition)
    {
        isKnockedBack = true;

        // Arah menjauh dari musuh, sedikit ke atas
        Vector2 knockbackDir = ((Vector2)transform.position - enemyPosition).normalized;
        knockbackDir.y = 0.5f;

        rb.velocity = Vector2.zero;
        rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);
        isKnockedBack = false;
    }

    IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;

        // Efek kedip-kedip selama invincible
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float elapsed = 0f;
        while (elapsed < invincibleDuration)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        sr.enabled = true;

        isInvincible = false;
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

    void SpawnSlash()
    {
        float dir = transform.localScale.x > 0 ? 1f : -1f;
        GameObject slash = Instantiate(slashPrefab, spawnPoint.position, Quaternion.identity);
        slash.GetComponent<SlashProjectile>().Init(dir);
    }
}