using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    public static bool shopOpen;

    [Header("Prices")]
    public int weaponPrice = 50;
    public int healthUpgradePrice = 100;
    public int speedUpgradePrice = 150;
    public int damageUpgradePrice = 200;
    public int miningSpeedUpgradePrice = 100;
    public int doubleMineralUpgradePrice = 150;

    [Header("References")]
    public Item weaponItem; // The weapon item to give to the player

    [Header("Audio")]
    public AudioClip buySound;
    private AudioSource audioSource;

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void BuyWeapon()
    {
        Debug.Log("ShopManager: BuyWeapon called.");
        if (PlayerStats.Instance != null && PlayerStats.Instance.hasBoughtFirstWeapon)
        {
            Debug.Log("ShopManager: Player already has first weapon, upgrading double mineral instead.");
            BuyDoubleMineralChance();
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("ShopManager: InventoryManager.Instance is null!");
            return;
        }

        if (weaponItem == null)
        {
            Debug.LogError("ShopManager: weaponItem is NOT assigned in the inspector!");
            return;
        }

        Debug.Log($"ShopManager: Attempting to buy weapon. Price: {weaponPrice}. Current Money: {InventoryManager.Instance.currentMoney}");
        if (InventoryManager.Instance.currentMoney >= weaponPrice)
        {
            if (InventoryManager.Instance.AddItem(weaponItem, CrystalRarity.Common))
            {
                InventoryManager.Instance.currentMoney -= weaponPrice;
                if (PlayerStats.Instance != null) PlayerStats.Instance.hasBoughtFirstWeapon = true;
                InventoryManager.Instance.UpdateInventoryUI();
                PlayBuySound();
                Debug.Log("ShopManager: Weapon bought successfully!");
            }
            else
            {
                Debug.LogWarning("ShopManager: Could not add weapon. Inventory might be full or slots not found!");
            }
        }
        else
        {
            Debug.LogWarning($"ShopManager: Not enough money! Need {weaponPrice}, have {InventoryManager.Instance.currentMoney}");
        }
    }

    public void BuyDoubleMineralChance()
    {
        if (InventoryManager.Instance == null) return;

        if (InventoryManager.Instance.currentMoney >= doubleMineralUpgradePrice)
        {
            InventoryManager.Instance.currentMoney -= doubleMineralUpgradePrice;
            if (PlayerStats.Instance != null) PlayerStats.Instance.doubleMineralChance += 0.01f;
            InventoryManager.Instance.UpdateInventoryUI();
            PlayBuySound();
            Debug.Log("Double mineral chance upgraded!");
        }
        else
        {
            Debug.LogWarning($"ShopManager: Not enough money for double mineral! Need {doubleMineralUpgradePrice}");
        }
    }

    public void UpgradeMiningSpeed()
    {
        if (InventoryManager.Instance == null) return;

        if (InventoryManager.Instance.currentMoney >= miningSpeedUpgradePrice)
        {
            InventoryManager.Instance.currentMoney -= miningSpeedUpgradePrice;
            if (PlayerStats.Instance != null) PlayerStats.Instance.miningSpeedMultiplier += 0.2f;
            InventoryManager.Instance.UpdateInventoryUI();
            PlayBuySound();
            Debug.Log("Mining speed upgraded!");
        }
        else
        {
            Debug.LogWarning($"ShopManager: Not enough money for mining speed! Need {miningSpeedUpgradePrice}");
        }
    }

    public void UpgradeHealth()
    {
        if (InventoryManager.Instance == null) return;

        if (InventoryManager.Instance.currentMoney >= healthUpgradePrice)
        {
            InventoryManager.Instance.currentMoney -= healthUpgradePrice;
            
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.maxHealth += 10;
            }

            PlayerHealth health = Object.FindAnyObjectByType<PlayerHealth>();
            if (health != null)
            {
                health.maxHealth = PlayerStats.Instance != null ? PlayerStats.Instance.maxHealth : health.maxHealth + 10;
                health.currentHealth = health.maxHealth; // Full heal on upgrade
            }

            InventoryManager.Instance.UpdateInventoryUI();
            PlayBuySound();
            Debug.Log("Health upgraded!");
        }
        else
        {
            Debug.LogWarning($"ShopManager: Not enough money for health! Need {healthUpgradePrice}");
        }
    }

    public void UpgradeSpeed()
    {
        if (InventoryManager.Instance == null) return;

        if (InventoryManager.Instance.currentMoney >= speedUpgradePrice)
        {
            InventoryManager.Instance.currentMoney -= speedUpgradePrice;

            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.speed += 0.5f;
            }

            PlayerController player = Object.FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                player.speed = PlayerStats.Instance != null ? PlayerStats.Instance.speed : player.speed + 0.5f;
            }

            InventoryManager.Instance.UpdateInventoryUI();
            PlayBuySound();
            Debug.Log("Speed upgraded!");
        }
    }

    public void UpgradeDamage()
    {
        if (InventoryManager.Instance == null) return;

        if (InventoryManager.Instance.currentMoney >= damageUpgradePrice)
        {
            InventoryManager.Instance.currentMoney -= damageUpgradePrice;

            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.damage += 5f;
            }

            PlayerController player = Object.FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                player.damage = PlayerStats.Instance != null ? PlayerStats.Instance.damage : player.damage + 5f;
            }

            InventoryManager.Instance.UpdateInventoryUI();
            PlayBuySound();
            Debug.Log("Damage upgraded!");
        }
    }

    private void PlayBuySound()
    {
        if (buySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buySound);
        }
    }

    public void CloseShop()
    {
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.NextRound();
        }
    }
    }
