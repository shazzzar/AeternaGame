using UnityEngine;
using TMPro;

public enum ShopItemType { HP, MiningSpeed, Weapon }

public class PhysicalShopItem : MonoBehaviour
{
    public ShopItemType itemType;
    public float detectionRadius = 5f; // Increased radius
    public GameObject labelParent;
    public TMP_Text labelText;

    private bool playerInside = false;
    private Transform mainCamera;

    void Start()
    {
        if (Camera.main != null) mainCamera = Camera.main.transform;
        if (labelParent != null) labelParent.SetActive(false);
    }

    void Update()
    {
        CheckPlayer();
        
        if (playerInside)
        {
            UpdateLabel();
            if (labelParent != null)
            {
                labelParent.SetActive(true);
                if (mainCamera != null)
                {
                    labelParent.transform.LookAt(labelParent.transform.position + mainCamera.forward);
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log($"PhysicalShopItem: [E] pressed near {gameObject.name} (Type: {itemType})");
                BuyItem();
            }
}
        else
        {
            if (labelParent != null) labelParent.SetActive(false);
        }
    }

    void CheckPlayer()
    {
        playerInside = false;
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                playerInside = true;
                break;
            }
        }
    }

    void UpdateLabel()
    {
        if (labelText == null) return;

        string text = "";
        int price = 0;

        if (ShopManager.Instance == null) return;

        switch (itemType)
        {
            case ShopItemType.HP:
                text = "Buy HP";
                price = ShopManager.Instance.healthUpgradePrice;
                break;
            case ShopItemType.MiningSpeed:
                text = "Buy Mining Speed";
                price = ShopManager.Instance.miningSpeedUpgradePrice;
                break;
            case ShopItemType.Weapon:
                if (PlayerStats.Instance != null && PlayerStats.Instance.hasBoughtFirstWeapon)
                {
                    text = "Buy Chance for Double Mineral (+1%)";
                    price = ShopManager.Instance.doubleMineralUpgradePrice;
                }
                else
                {
                    text = "Buy Weapon";
                    price = ShopManager.Instance.weaponPrice;
                }
                break;
        }

        labelText.text = $"{text}\nPrice: {price} €\n[E] to Buy";
    }

    void BuyItem()
    {
        if (ShopManager.Instance == null) return;

        switch (itemType)
        {
            case ShopItemType.HP:
                ShopManager.Instance.UpgradeHealth();
                break;
            case ShopItemType.MiningSpeed:
                ShopManager.Instance.UpgradeMiningSpeed();
                break;
            case ShopItemType.Weapon:
                ShopManager.Instance.BuyWeapon();
                break;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
