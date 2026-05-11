using UnityEngine;

public class FlyingEnemyAI : MonoBehaviour
{
    [Header("Patrol Circular")]
    public float radius = 2f;
    public float orbitSpeed = 2f;

    [Header("Chase Settings")]
    public float chaseSpeed = 4f;
    public float detectionRadius = 5f;
    public float loseRadius = 7f;

    [Header("Combat")]
    public int damage = 10;
    public float damageCooldown = 1f;
    private float lastDamageTime = -99f;

    [Header("Bounce")]
    public float bounceDistance = 2f;   // seberapa jauh mundur setelah nabrak
    public float bounceSpeed = 6f;      // kecepatan mundur
    private bool isBouncing = false;
    private Vector3 bounceTarget;

    private Transform player;
    private Vector3 centerPoint;
    private float angle = 0f;

    private enum State { Patrolling, Chasing, Returning }
    private State currentState = State.Patrolling;

    void Start()
    {
        centerPoint = transform.position;
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.gravityScale = 0f;
    }

    void Update()
    {
        if (player == null) return;

        // Selama bounce, enemy mundur dulu — skip AI state
        if (isBouncing)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, bounceTarget, bounceSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, bounceTarget) < 0.1f)
            {
                isBouncing = false;
                currentState = State.Chasing; // langsung kejar lagi setelah bounce
            }
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        float distToCenter = Vector2.Distance(transform.position, centerPoint);

        switch (currentState)
        {
            case State.Patrolling:
                DoOrbit();
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
                if (distToCenter < 0.3f)
                    currentState = State.Patrolling;
                if (distToPlayer <= detectionRadius)
                    currentState = State.Chasing;
                break;
        }
    }

    void DoOrbit()
    {
        angle += orbitSpeed * Time.deltaTime;
        float x = centerPoint.x + Mathf.Cos(angle) * radius;
        float y = centerPoint.y + Mathf.Sin(angle) * radius;
        transform.position = new Vector3(x, y, transform.position.z);

        Vector3 s = transform.localScale;
        s.x = Mathf.Cos(angle) > 0 ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
        transform.localScale = s;
    }

    void DoChase()
    {
        transform.position = Vector3.MoveTowards(
            transform.position, player.position, chaseSpeed * Time.deltaTime);

        Vector3 s = transform.localScale;
        s.x = player.position.x > transform.position.x
            ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
        transform.localScale = s;
    }

    void DoReturn()
    {
        transform.position = Vector3.MoveTowards(
            transform.position, centerPoint, chaseSpeed * Time.deltaTime);
    }

    // ─── Ganti OnCollisionEnter2D → OnTriggerEnter2D + OnTriggerStay2D ───
    // karena enemy bergerak via transform (bukan physics), collision ga ke-trigger

    void OnTriggerEnter2D(Collider2D col)
    {
        TryDamagePlayer(col.gameObject);
    }

    void OnTriggerStay2D(Collider2D col)
    {
        // Handle juga kalau player diam dan enemy yang bergerak masuk
        TryDamagePlayer(col.gameObject);
    }

    void TryDamagePlayer(GameObject obj)
    {
        if (!obj.CompareTag("Player")) return;
        if (Time.time - lastDamageTime < damageCooldown) return;

        lastDamageTime = Time.time;
        obj.GetComponent<PlayerController>()?.TakeDamage(damage, transform.position);

        // Bounce: mundur ke arah berlawanan dari player, sedikit ke atas
        Vector3 bounceDir = (transform.position - obj.transform.position).normalized;
        bounceDir.y = Mathf.Abs(bounceDir.y) + 0.5f; // paksa ke atas
        bounceTarget = transform.position + bounceDir.normalized * bounceDistance;
        isBouncing = true;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 origin = Application.isPlaying ? centerPoint : transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(origin, radius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRadius);
    }
}