using UnityEngine;

public class SlashProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float maxDistance = 3.5f;
    public int damage = 15;
    private float direction;
    private Vector3 startPos;

    public void Init(float dir)
    {
        direction = dir;
        startPos = transform.position;

        if (dir < 0)
        {
            Vector3 s = transform.localScale;
            s.x = -Mathf.Abs(s.x);
            transform.localScale = s;
        }

        Destroy(gameObject, 2f);
    }

    void Update()
    {
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, startPos) >= maxDistance)
            Destroy(gameObject);
    }

        void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Enemy"))
        {
            EnemyHealth eh = col.gameObject.GetComponent<EnemyHealth>();
            if (eh != null) eh.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (!col.gameObject.CompareTag("Player"))
            Destroy(gameObject);
    }
        void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth eh = other.GetComponent<EnemyHealth>();
            if (eh != null) eh.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (!other.CompareTag("Player"))
            Destroy(gameObject);
    }
}