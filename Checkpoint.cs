using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public static Vector3 respawnPoint;
    private static bool checkpointActivated = false;
    private bool playerNearby = false;

    [Header("Posisi awal player")]
    public Transform defaultSpawn;

    [Header("UI")]
    public GameObject checkpointMenu;

    private static Checkpoint currentActive;

    void Start()
    {
        if (!checkpointActivated && defaultSpawn != null)
            respawnPoint = defaultSpawn.position;

        if (checkpointMenu != null)
            checkpointMenu.SetActive(false);
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.F))
        {
            if (checkpointMenu != null && checkpointMenu.activeSelf)
                CloseMenu();
            else if (checkpointMenu != null)
                OpenMenu();
        }

        // Gunakan unscaledDeltaTime agar Escape tetap jalan saat timeScale = 0
        if (Input.GetKeyDown(KeyCode.Escape) && checkpointMenu != null && checkpointMenu.activeSelf)
            CloseMenu();
    }

    void OpenMenu()
    {
        if (checkpointMenu == null)
        {
            Debug.LogWarning("checkpointMenu belum di-assign di Inspector!", this);
            return;
        }

        respawnPoint = transform.position;
        checkpointActivated = true;
        currentActive = this;

        // Set respawn point dulu, save dilakukan lewat tombol Save di menu
        checkpointMenu.SetActive(true);
        Time.timeScale = 0f;

        CheckpointMenu menu = checkpointMenu.GetComponent<CheckpointMenu>();
        if (menu != null)
            menu.RefreshUI();
        else
            Debug.LogWarning("Komponen CheckpointMenu tidak ditemukan!", this);

        Debug.Log("Checkpoint aktif!");
    }

    // Dipanggil dari tombol Save di CheckpointMenu
        public void SaveGame()
    {
        if (GameManager.Instance == null) return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController == null) return;

        // transform.position di sini = posisi Checkpoint object di scene (benar)
        Vector3 checkpointPosition = transform.position;

        Debug.Log($"Saving at checkpoint position: {checkpointPosition}");

        GameManager.Instance.SavePlayerData(
            checkpointPosition,
            playerController.currentHP,
            0
        );
    }

    public void CloseMenu()
    {
        if (checkpointMenu == null) return;
        checkpointMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public bool IsMenuOpen()
    {
        return checkpointMenu != null && checkpointMenu.activeSelf;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNearby = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            CloseMenu();
        }
    }
}