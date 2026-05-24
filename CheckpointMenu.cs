using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CheckpointMenu : MonoBehaviour
{
    [Header("Coin Display")]
    public TextMeshProUGUI coinText;

    [Header("Stats Display")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI atkText;
    public TextMeshProUGUI spdText;

    [Header("Save Feedback")]
    public TextMeshProUGUI saveStatusText; // optional, assign jika ada label status

    [Header("Cost per Upgrade")]
    public int hpCost = 3;
    public int atkCost = 5;
    public int spdCost = 4;

    [Header("Stat Increase per Upgrade")]
    public int hpIncrease = 20;
    public float atkIncrease = 5f;
    public float spdIncrease = 0.5f;

    private PlayerController player;
    private Checkpoint checkpoint;

    void Awake()
    {
        player = FindObjectOfType<PlayerController>();
        
        // Pastikan cari Checkpoint di scene, bukan di UI
        checkpoint = FindObjectOfType<Checkpoint>();
        
        if (checkpoint == null)
            Debug.LogWarning("Checkpoint tidak ditemukan di scene!");
        else
            Debug.Log("Checkpoint ditemukan: " + checkpoint.gameObject.name + 
                    " pos: " + checkpoint.transform.position);
    }
    public void RefreshUI()
    {
        if (player == null) player = FindObjectOfType<PlayerController>();
        if (checkpoint == null) checkpoint = FindObjectOfType<Checkpoint>();

        if (CoinManager.Instance != null)
            coinText.text = "Coin: " + CoinManager.Instance.GetCoins();

        hpText.text = "HP: " + player.maxHP + "\n(+" + hpIncrease + " | " + hpCost + " coin)";
        atkText.text = "ATK Speed: " + player.attackSpeed + "\n(+" + atkIncrease + " | " + atkCost + " coin)";
        spdText.text = "Speed: " + player.walkSpeed + "\n(+" + spdIncrease + " | " + spdCost + " coin)";

        if (saveStatusText != null)
            saveStatusText.text = "";
    }

    // Hubungkan ke tombol Save di UI
    public void SaveGame()
    {
        if (checkpoint != null)
            checkpoint.SaveGame(); // delegasikan ke Checkpoint.cs
        else
            Debug.LogWarning("Checkpoint reference null di CheckpointMenu!");
    }

    public void UpgradeHP()
    {
        if (CoinManager.Instance.SpendCoins(hpCost))
        {
            player.maxHP += hpIncrease;
            player.HealFull();
            RefreshUI();
        }
    }

    public void UpgradeATK()
    {
        if (CoinManager.Instance.SpendCoins(atkCost))
        {
            player.attackSpeed += atkIncrease;
            RefreshUI();
        }
    }

    public void UpgradeSpeed()
    {
        if (CoinManager.Instance.SpendCoins(spdCost))
        {
            player.walkSpeed += spdIncrease;
            player.runSpeed += spdIncrease;
            RefreshUI();
        }
    }

    public void CloseMenu()
    {
        if (checkpoint != null)
            checkpoint.CloseMenu();
    }
}