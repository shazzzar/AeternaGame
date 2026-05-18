using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Updated with 3 more rarities as requested
public enum CrystalRarity { Common, Uncommon, Rare, Epic, Mythic, Ancient, Legendary }

public class Crystal : MonoBehaviour
{
    public Sprite itemIcon;
    public string crystalName;
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
            _ => baseMineTime // Default for Common
        };
    }

    public int GetDropAmount()
    {
        // Legendary drops between 5-10
        // Others scaled down from there
        return rarity switch
        {
            CrystalRarity.Legendary => Random.Range(5, 11),
            CrystalRarity.Ancient => Random.Range(4, 9),
            CrystalRarity.Mythic => Random.Range(4, 7),
            CrystalRarity.Epic => Random.Range(3, 6),
            CrystalRarity.Rare => Random.Range(2, 5),
            CrystalRarity.Uncommon => Random.Range(2, 4),
            _ => Random.Range(1, 3) // Common
        };
    }

    private void Start()
    {
        if (interactionPrompt) interactionPrompt.SetActive(false);
    }

    public void ShowPrompt(bool state)
    {
        if (interactionPrompt) interactionPrompt.SetActive(state);
    }

    public void OnMined()
    {
        Debug.Log(crystalName + " minerado!");
        Destroy(gameObject);
    }
}