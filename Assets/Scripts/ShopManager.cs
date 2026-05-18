using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("Prices")]
    public int weaponPrice = 50;
    public int healthUpgradePrice = 100;
public int speedUpgradePrice = 150;
    public int damageUpgradePrice = 200;

    [Header("References")]
    public Item weaponItem; // The weapon item to give to the player

    void Awake()
    {
        Instance = this;
    }

    public void BuyWeapon()
    {
        if (InventoryManager.Instance.currentMoney >= weaponPrice)
        {
            if (InventoryManager.Instance.AddItem(weaponItem, CrystalRarity.Common))
            {
                InventoryManager.Instance.currentMoney -= weaponPrice;
                InventoryManager.Instance.UpdateInventoryUI();
                Debug.Log("Weapon bought!");
            }
            else
            {
                Debug.Log("Inventory full!");
            }
        }
        else
        {
            Debug.Log("Not enough money!");
        }
    }

    public void UpgradeHealth()
    {
        if (InventoryManager.Instance.currentMoney >= healthUpgradePrice)
        {
            PlayerHealth health = Object.FindAnyObjectByType<PlayerHealth>();
            if (health != null)
            {
                InventoryManager.Instance.currentMoney -= healthUpgradePrice;
                health.maxHealth += 10;
                health.currentHealth += 10; // Also heal a bit
                InventoryManager.Instance.UpdateInventoryUI();
                Debug.Log("Health upgraded!");
            }
        }
    }

    public void UpgradeSpeed()
    {
        if (InventoryManager.Instance.currentMoney >= speedUpgradePrice)
        {
            PlayerController player = Object.FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                InventoryManager.Instance.currentMoney -= speedUpgradePrice;
                player.speed += 0.5f;
                InventoryManager.Instance.UpdateInventoryUI();
                Debug.Log("Speed upgraded!");
            }
        }
    }

    public void UpgradeDamage()
    {
        if (InventoryManager.Instance.currentMoney >= damageUpgradePrice)
        {
            PlayerController player = Object.FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                InventoryManager.Instance.currentMoney -= damageUpgradePrice;
                player.damage += 5f;
                InventoryManager.Instance.UpdateInventoryUI();
                Debug.Log("Damage upgraded!");
            }
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
