using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawn : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemy;
    public Transform spawnPosition;

    [Header("Spawn Settings")]
    public float spawnerdelay = 2f;
    // jarak waktu untuk memulai spawn pertama kali
    public float spawninterval = 5f;
    // jarak waktu untuk spawn selanjutnya

    public Transform player;

    void Start()
    {
        // memulai fungsi Spawner setelah spawnerdelay detik, dan mengulang setiap spawninterval detik
        InvokeRepeating("Spawner", spawnerdelay, spawninterval);

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }
    void Spawner()
    {
        // membuat objek musuh baru di posisi spawnPosition dengan rotasi default
        
        Vector2 spawnPos = new Vector2(player.position.x+10 + spawnPosition.position.x, spawnPosition.position.y+10);
        Instantiate(enemy, spawnPos, Quaternion.identity);
    }
    

}
