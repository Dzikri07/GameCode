using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public int maxHP = 100;
    private int currentHP;
    private SpriteRenderer sr;
    private Color originalColor;

    void Start()
    {
        currentHP = maxHP;
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        StartCoroutine(HitFlash());

        if (currentHP <= 0)
            Die();
    }

    IEnumerator HitFlash()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        sr.color = originalColor;
    }

    void Die()
    {
        // nanti isi animasi mati
        Destroy(gameObject);
    }
}