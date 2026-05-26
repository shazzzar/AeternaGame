using UnityEngine;

public enum CrystalRarity { Common, Uncommon, Rare, Epic, Mythic, Ancient, Legendary }

public class Crystal : MonoBehaviour
{
    [Header("Item")]
    public Item item;

    public CrystalRarity rarity;
    public float baseMineTime = 2f;

    [Header("UI Reference")]
    public GameObject interactionPrompt;

    public float GetMiningDuration()
    {
        return rarity switch
        {
            CrystalRarity.Uncommon => baseMineTime * 1.5f,
            CrystalRarity.Rare => baseMineTime * 2f,
            CrystalRarity.Epic => baseMineTime * 4f,
            CrystalRarity.Mythic => baseMineTime * 5f,
            CrystalRarity.Ancient => baseMineTime * 6f,
            CrystalRarity.Legendary => baseMineTime * 8f,
            _ => baseMineTime
        };
    }

    public int GetDropAmount()
    {
        return rarity switch
        {
            CrystalRarity.Legendary => Random.Range(5, 11),
            CrystalRarity.Ancient => Random.Range(4, 9),
            CrystalRarity.Mythic => Random.Range(4, 7),
            CrystalRarity.Epic => Random.Range(3, 6),
            CrystalRarity.Rare => Random.Range(2, 5),
            CrystalRarity.Uncommon => Random.Range(2, 4),
            _ => Random.Range(1, 3)
        };
    }

    private void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    public void ShowPrompt(bool state)
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(state);
    }

    public void OnMined()
    {
        if (item == null)
        {
            Debug.LogWarning("Crystal sem item atribuído!");
            return;
        }

        int amount = GetDropAmount();

        if (PlayerStats.Instance != null && Random.value < PlayerStats.Instance.doubleMineralChance)
        {
            amount *= 2;
            Debug.Log("Double minerals! Chance was: " + PlayerStats.Instance.doubleMineralChance);
        }

        bool addedAny = InventoryManager.Instance.AddItemAmount(item, rarity, amount);

        if (addedAny)
        {
            Debug.Log("Crystal minerado!");
            Destroy(gameObject);
        }
    }
}
