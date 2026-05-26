using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Mining Stats")]
    public float miningSpeedMultiplier = 1f;
    public float doubleMineralChance = 0f; // 0 to 1 (0% to 100%)

    [Header("Player Stats")]
    public float maxHealth = 100f;
    public float speed = 5f;
    public float damage = 10f;

    [Header("Currency")]
    public int money = 0;

    [Header("Progression")]
    public bool hasBoughtFirstWeapon = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
