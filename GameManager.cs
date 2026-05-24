using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("GameManager");
                    _instance = obj.AddComponent<GameManager>();
                    Debug.Log("GameManager dibuat otomatis.");
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            // Subscribe ke event scene loaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Dipanggil otomatis setiap kali scene selesai di-load
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);

        // Hanya load player data di scene gameplay, bukan di menu
        if (scene.name == "Act1" || scene.name == "Act2")
        {
            if (Save.HasSave() && Save.HasValidPosition())
            {
                // Delay sedikit agar semua object di scene siap
                Invoke(nameof(DoLoadPlayer), 0.1f);
            }
        }
    }

    // Start tidak perlu load lagi, sudah handle di OnSceneLoaded
    void Start() { }

    public void LoadPlayerFromSave()
    {
        if (!Save.HasSave()) return;
        Invoke(nameof(DoLoadPlayer), 0.1f);
    }

        void DoLoadPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found in scene!");
            return;
        }

        if (Save.HasValidPosition())
        {
            Vector3 savedPosition = Save.GetSavedPosition();

            // Validasi posisi wajar
            bool posisiWajar = savedPosition.x > -500f && savedPosition.x < 500f &&
                            savedPosition.y > -100f && savedPosition.y < 100f;

            if (posisiWajar)
            {
                player.transform.position = savedPosition;
                Debug.Log("Player loaded at: " + savedPosition);

                // ========== TAMBAHKAN INI ==========
                // Sesuaikan posisi awan dengan player setelah teleport
                CloudScrolling cloud = FindObjectOfType<CloudScrolling>();
                if (cloud != null)
                {
                    cloud.AlignToPlayer();
                    Debug.Log("CloudScrolling di-align ke posisi player setelah load.");
                }
                else
                {
                    Debug.LogWarning("CloudScrolling tidak ditemukan di scene.");
                }
                // ===================================
            }
            else
            {
                Debug.LogWarning("Posisi tersimpan tidak wajar, diabaikan: " + savedPosition);
                Save.DeleteSave();
            }
        }

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            int savedHP = Save.GetSavedHealth();

            if (savedHP <= 0 || savedHP > playerController.maxHP)
                playerController.currentHP = playerController.maxHP;
            else
                playerController.currentHP = savedHP;

            if (playerController.healthBar != null)
                playerController.healthBar.size = (float)playerController.currentHP / playerController.maxHP;

            Debug.Log("HP loaded: " + playerController.currentHP);
        }
    }

    public void SavePlayerData(Vector3 playerPosition, int health, int score)
    {
        Save saveSystem = gameObject.GetComponent<Save>();
        if (saveSystem == null)
            saveSystem = gameObject.AddComponent<Save>();

        saveSystem.health = health;
        saveSystem.score = score;
        saveSystem.position = playerPosition;
        saveSystem.SavePosition();
        Debug.Log($"Saved - Pos: {playerPosition}, HP: {health}");
    }
}