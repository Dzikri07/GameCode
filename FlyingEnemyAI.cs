using UnityEngine;

public class FlyingEnemyAI : MonoBehaviour
{
    [Header("Patrol Circular")]
    public float radius = 2f;        // radius putaran
    public float orbitSpeed = 2f;    // kecepatan putaran

    [Header("Chase Settings")]
    public float chaseSpeed = 4f;
    public float detectionRadius = 5f;
    public float loseRadius = 7f;

    private Transform player;
    private Vector3 centerPoint;     // titik tengah putaran
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

        // flip sprite sesuai arah gerak
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