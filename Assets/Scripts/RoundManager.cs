using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;

    [Header("Round Settings")]
    public float roundDuration = 600f; // 10 minutes
    public int currentRound = 1;
    public int maxRounds = 5;
    public bool isShopPhase = false;

    [Header("UI")]
    public TMP_Text timerText;
    public TMP_Text roundText;
    public GameObject shopPanel;
    public GameObject winPanel;
    public GameObject hudParent; // Optional: Assign a parent for all HUD elements

    [Header("Spawning")]
    public GameObject[] enemyPrefabs;
    public GameObject[] mineralPrefabs;
    public float spawnMargin = 10f; // Stay away from borders
    public float earlyRoundRadius = 50f; // Radius around player for early rounds

    private float timer;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private List<GameObject> activeMinerals = new List<GameObject>();
    private Terrain terrain;
    private Transform playerTransform;
    private Vector3 playerInitialPosition;
    private Quaternion playerInitialRotation;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Transfer references to the persistent instance
            Instance.timerText = this.timerText;
            Instance.roundText = this.roundText;
            Instance.shopPanel = this.shopPanel;
            Instance.winPanel = this.winPanel;
            Instance.hudParent = this.hudParent;
            
            // Also sync current state if needed (though usually Instance is the source of truth)
            
            Destroy(gameObject);
            return;
        }

        terrain = Terrain.activeTerrain;
        FindPlayer();
        if (playerTransform != null)
        {
            playerInitialPosition = playerTransform.position;
            playerInitialRotation = playerTransform.rotation;
        }
    }

    void FindPlayer()
    {
        PlayerController pc = Object.FindAnyObjectByType<PlayerController>();
        if (pc != null) playerTransform = pc.transform;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SampleScene" && Instance == this)
        {
            StartRound();
        }
    }

    void Start()
    {
        // Initial start
        if (Instance == this && SceneManager.GetActiveScene().name == "SampleScene")
        {
            StartRound();
        }
    }

    void Update()
    {
        if (!isShopPhase && currentRound <= maxRounds)
        {
            UpdateTimer();
        }
    }

    void UpdateTimer()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            EndRound();
        }
        UpdateTimerUI();
    }

    void StartRound()
    {
        if (currentRound > maxRounds)
        {
            WinGame();
            return;
        }

        if (playerTransform == null) FindPlayer();
        
        // Respawn player
        if (playerTransform != null)
        {
            // Reset position and rotation
            playerTransform.position = playerInitialPosition;
            playerTransform.rotation = playerInitialRotation;
            
            // Also heal the player if they have health component
            PlayerHealth health = playerTransform.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.currentHealth = health.maxHealth;
            }
        }

        timer = roundDuration;
        isShopPhase = false;
        
        if (shopPanel != null) shopPanel.SetActive(false);

        // Re-enable inventory slide component if it was hidden
        InventorySlide invSlide = Object.FindAnyObjectByType<InventorySlide>(FindObjectsInactive.Include);
        if (invSlide != null)
        {
            invSlide.gameObject.SetActive(true);
        }

        if (hudParent != null) hudParent.SetActive(true);
        else
        {
            // Fallback: Show specific UI elements if hudParent is not set
            if (timerText != null) timerText.gameObject.SetActive(true);
            if (roundText != null) roundText.gameObject.SetActive(true);
            
            // Find Slider
            GameObject slider = GameObject.Find("Slider");
            if (slider != null) slider.SetActive(true);
        }
        
        // Resume simulation
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        UpdateRoundUI();
        SpawnRoundEntities();
    }

    void EndRound()
    {
        isShopPhase = true;
        
        // Simulation can continue at normal speed in the pause scene
        Time.timeScale = 1;
        
        // Sell non-weapon items and save state
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RefreshSlots(); // Ensure slots are updated
            InventoryManager.Instance.SellAllNonWeapons();
            InventoryManager.Instance.SaveInventoryState();
            Debug.Log($"EndRound: Money after selling: {InventoryManager.Instance.currentMoney}");
        }

        // Instead of opening the local shop panel, load the Round_Pause scene
        SceneManager.LoadScene("Round_Pause");

        // Close and hide inventory
        InventorySlide invSlide = Object.FindAnyObjectByType<InventorySlide>();
        if (invSlide != null)
        {
            invSlide.ToggleInventory(false);
            // invSlide.gameObject.SetActive(false); // DO NOT DISABLE THE GAMEOBJECT, or it won't handle input in the next scene
        }

        // Hide HUD
        if (hudParent != null) hudParent.SetActive(false);
        else
        {
            if (timerText != null) timerText.gameObject.SetActive(false);
            if (roundText != null) roundText.gameObject.SetActive(false);
            
            GameObject slider = GameObject.Find("Slider");
            if (slider != null) slider.SetActive(false);
        }
        
        // Clear remaining entities
        ClearEntities();

        Debug.Log("Round " + currentRound + " ended. Loading Round_Pause scene.");
    }


    public void NextRound()
    {
        currentRound++;
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SaveInventoryState();
        }
        SceneManager.LoadScene("SampleScene");
    }

    void SpawnRoundEntities()
    {
        if (terrain == null) terrain = Terrain.activeTerrain;
        if (terrain == null) return;

        Vector3 terrainPos = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;

        List<Vector3> rareMineralSpots = new List<Vector3>();

        // 1. Spawn Minerals First
        if (mineralPrefabs != null && mineralPrefabs.Length > 0)
        {
            // Large grid for spreading out huge assets
            int gridSide = 12; 
            float cellWidth = terrainSize.x / gridSide;
            float cellHeight = terrainSize.z / gridSide;
            
            int totalGroups = currentRound * 4; 
            List<int> cells = new List<int>();
            for(int i=0; i<gridSide*gridSide; i++) cells.Add(i);

            for (int g = 0; g < totalGroups && cells.Count > 0; g++)
            {
                int idx = Random.Range(0, cells.Count);
                int cell = cells[idx];
                cells.RemoveAt(idx);

                int gx = cell % gridSide;
                int gz = cell / gridSide;

                Vector3 groupCenter = terrainPos + new Vector3(
                    gx * cellWidth + cellWidth * 0.5f,
                    0,
                    gz * cellHeight + cellHeight * 0.5f
                );
                
                // Add jitter
                groupCenter += new Vector3(Random.Range(-cellWidth * 0.3f, cellWidth * 0.3f), 0, Random.Range(-cellHeight * 0.3f, cellHeight * 0.3f));

                // Round 1 minerals are closer to player
                if (currentRound <= 1 && playerTransform != null)
                {
                    Vector2 circle = Random.insideUnitCircle * earlyRoundRadius;
                    groupCenter = playerTransform.position + new Vector3(circle.x, 0, circle.y);
                }
                
                groupCenter.y = terrain.SampleHeight(groupCenter) + terrainPos.y;

                int groupSize = Random.Range(3, 7); 
                GameObject prefab = GetWeightedMineralPrefab();
                if (prefab != null)
                {
                    // radius 50f for huge assets clumping prevention
                    SpawnGroup(prefab, groupCenter, groupSize, 50f, activeMinerals, false, rareMineralSpots);
                }
            }
        }

        // 2. Spawn Enemies
        if (currentRound > 1 && enemyPrefabs != null && enemyPrefabs.Length > 0)
        {
            int enemyGroups = currentRound + 2;
            
            for (int g = 0; g < enemyGroups; g++)
            {
                Vector3 groupCenter;
                // Prefer spawning near rare minerals from Round 2
                if (rareMineralSpots.Count > 0)
                {
                    int rIdx = Random.Range(0, rareMineralSpots.Count);
                    groupCenter = rareMineralSpots[rIdx];
                    rareMineralSpots.RemoveAt(rIdx); 
                }
                else
                {
                    groupCenter = GetRandomTerrainPosition(terrainPos, terrainSize);
                }

                int size = Random.Range(3, 6 + currentRound);
                SpawnGroup(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)], groupCenter, size, 15f, activeEnemies, true, null);
            }
        }
    }

    Vector3 GetRandomTerrainPosition(Vector3 pos, Vector3 size)
    {
        float x = Random.Range(pos.x + spawnMargin, pos.x + size.x - spawnMargin);
        float z = Random.Range(pos.z + spawnMargin, pos.z + size.z - spawnMargin);
        float y = terrain.SampleHeight(new Vector3(x, 0, z)) + pos.y;
        return new Vector3(x, y, z);
    }

    void SpawnGroup(GameObject prefab, Vector3 center, int count, float radius, List<GameObject> activeList, bool isEnemy, List<Vector3> raritySpots)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = Vector3.zero;
            bool validPos = false;
            int attempts = 0;

            // Huge assets (~30 units), so distance check is critical
            float minDistance = isEnemy ? 6.0f : 40.0f;

            while (!validPos && attempts < 30)
            {
                attempts++;
                Vector2 offset = Random.insideUnitCircle * radius;
                
                // Minerals avoid cluster center
                if (!isEnemy && offset.magnitude < radius * 0.35f)
                {
                    offset = offset.normalized * (radius * 0.4f + Random.Range(0, radius * 0.2f));
                }

                spawnPos = center + new Vector3(offset.x, 0, offset.y);
                spawnPos.y = terrain.SampleHeight(spawnPos) + terrain.transform.position.y;

                validPos = true;
                
                // Absolute check against all existing minerals
                foreach (var active in activeMinerals)
                {
                    if (active != null && Vector3.Distance(active.transform.position, spawnPos) < minDistance)
                    {
                        validPos = false;
                        break;
                    }
                }
                
                if (validPos && isEnemy)
                {
                    foreach (var active in activeEnemies)
                    {
                        if (active != null && Vector3.Distance(active.transform.position, spawnPos) < minDistance)
                        {
                            validPos = false;
                            break;
                        }
                    }
                }
            }

            if (validPos)
            {
                GameObject obj = Instantiate(prefab, spawnPos, Quaternion.Euler(0, Random.Range(0, 360), 0));
                
                if (isEnemy)
                {
                    float scale = Random.Range(0.7f, 1.8f);
                    obj.transform.localScale = Vector3.one * scale;
                }
                else if (raritySpots != null)
                {
                    Crystal c = obj.GetComponent<Crystal>();
                    if (c != null && (int)c.rarity >= (int)CrystalRarity.Rare)
                    {
                        raritySpots.Add(spawnPos);
                    }
                }

                activeList.Add(obj);
            }
        }
    }

    GameObject GetWeightedMineralPrefab()
    {
        if (mineralPrefabs == null || mineralPrefabs.Length == 0) return null;

        float totalWeight = 0;
        List<float> weights = new List<float>();

        foreach (var p in mineralPrefabs)
        {
            if (p == null) continue;
            Crystal c = p.GetComponent<Crystal>();
            float w = 20f; 

            if (c != null)
            {
                w = c.rarity switch
                {
                    CrystalRarity.Common => 150f,
                    CrystalRarity.Uncommon => 80f,
                    CrystalRarity.Rare => 30f,
                    CrystalRarity.Epic => 10f, 
                    CrystalRarity.Mythic => 5f, 
                    CrystalRarity.Ancient => 2f,
                    CrystalRarity.Legendary => 1f,
                    _ => 20f
                };
            }
            
            if (p.name.Contains("Emerald") || p.name.Contains("Ruby"))
            {
                w += 5f; 
            }

            weights.Add(w);
            totalWeight += w;
        }

        if (totalWeight <= 0) return mineralPrefabs[0];

        float randomValue = Random.Range(0, totalWeight);
        float currentSum = 0;

        for (int i = 0; i < weights.Count; i++)
        {
            currentSum += weights[i];
            if (randomValue <= currentSum) return mineralPrefabs[i];
        }

        return mineralPrefabs[0];
    }

    void ClearEntities()
    {
        foreach (var e in activeEnemies) if (e != null) Destroy(e);
        foreach (var m in activeMinerals) if (m != null) Destroy(m);
        activeEnemies.Clear();
        activeMinerals.Clear();
    }

    void WinGame()
    {
        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("You Won!");
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timer / 60f);
            int seconds = Mathf.FloorToInt(timer % 60f);
            timerText.text = string.Format("{0}:{1:00}", minutes, seconds);
        }
    }

    void UpdateRoundUI()
    {
        if (roundText != null)
        {
            roundText.text = "Round: " + currentRound + " / " + maxRounds;
        }
    }
}