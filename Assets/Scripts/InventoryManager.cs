using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Slots")]
    public Transform slotsParent;
    public InventorySlot[] slots;

    [Header("UI")]
    public TMP_Text moneyVariable;
    public TMP_Text weightVariable;
    public TMP_Text slotsVariable;

    [Header("Grid Settings")]
    public int columns = 7;
    public int rows = 5;

    [Header("Currency")]
    public int currentMoney = 0;

    [Header("Stack")]
    public int maxStackPerSlot = 5;

    [Header("Weight")]
    public float maxWeight = 105f;

    [System.Serializable]
    private class SavedItemData
    {
        public int index;
        public Item item;
        public CrystalRarity rarity;
        public int amount;
        public int w;
        public int h;
        public bool rot;
    }
    private List<SavedItemData> _savedInventoryState = new List<SavedItemData>();

    public void SaveInventoryState()
    {
        // Don't clear if we don't have slots to save from (prevents data loss)
        if (slots == null || slots.Length == 0) 
        {
            Debug.LogWarning("InventoryManager: Attempted to save state but no slots are active. Skipping save to prevent data loss.");
            return;
        }

        _savedInventoryState.Clear();
        foreach (InventorySlot slot in slots)
        {
            if (slot != null && slot.item != null && slot.isMaster)
            {
                _savedInventoryState.Add(new SavedItemData
                {
                    index = slot.index,
                    item = slot.item,
                    rarity = slot.rarity,
                    amount = slot.amount,
                    w = slot.currentWidth,
                    h = slot.currentHeight,
                    rot = slot.isRotated
                });
            }
        }
        Debug.Log($"InventoryManager: Saved {_savedInventoryState.Count} items. Current Money: {currentMoney}");
    }

    public void LoadInventoryState()
    {
        RefreshSlots();
        if (slots != null)
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot != null) slot.ClearSlot();
            }
        }

        foreach (var data in _savedInventoryState)
        {
            if (CanPlaceAt(data.index, data.w, data.h))
            {
                PlaceItemAt(data.index, data.item, data.rarity, data.amount, data.w, data.h, data.rot);
            }
            else
            {
                AddItemAmount(data.item, data.rarity, data.amount, data.w, data.h, data.rot);
            }
        }
        UpdateInventoryUI();
        Debug.Log($"InventoryManager: Restored {_savedInventoryState.Count} items. Current Money: {currentMoney}");
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null); 
            DontDestroyOnLoad(gameObject);
            Debug.Log("InventoryManager: Initialized persistent instance.");
        }
        else
        {
            Debug.Log("InventoryManager: Destroying duplicate instance.");
            Destroy(gameObject);
            return;
        }

        RefreshSlots();
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (Instance == this)
        {
            Debug.Log($"InventoryManager: OnSceneLoaded triggered for scene {scene.name}");
            
            // Re-find references in the new scene
            FindUIReferences();
            
            if (slotsParent != null)
            {
                LoadInventoryState();
            }
            else
            {
                Debug.LogWarning("InventoryManager: Could not find InventoryPanel/SlotContainer in this scene.");
            }
        }
    }

    private void FindUIReferences()
    {
        // Search through all canvases for the InventoryPanel
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        RectTransform foundPanel = null;

        foreach (Canvas canvas in canvases)
        {
            foreach (RectTransform rt in canvas.GetComponentsInChildren<RectTransform>(true))
            {
                if (rt.name == "InventoryPanel")
                {
                    foundPanel = rt;
                    break;
                }
            }
            if (foundPanel != null) break;
        }

        if (foundPanel != null)
        {
            Debug.Log($"InventoryManager: Found UI Panel: {foundPanel.name}");
            slotsParent = foundPanel.transform.Find("SlotContainer");
            moneyVariable = foundPanel.transform.Find("MoneyVariable")?.GetComponent<TMP_Text>();
            
            // Try different names for weight/slots labels
            weightVariable = foundPanel.transform.Find("Weight")?.GetComponent<TMP_Text>();
            slotsVariable = foundPanel.transform.Find("SlotsVariable")?.GetComponent<TMP_Text>();
            if (slotsVariable == null) slotsVariable = foundPanel.transform.Find("Title")?.GetComponent<TMP_Text>();

            InventorySlide slide = GetComponent<InventorySlide>();
            if (slide != null)
            {
                slide.panel = foundPanel;
                slide.InitializePanel();
            }
            
            RefreshSlots();
        }
    }

    private bool _initialItemsSetup = false;
    void Start()
    {
        if (!_initialItemsSetup)
        {
            RefreshSlots();
            SetupInitialItems();
            UpdateInventoryUI();
            _initialItemsSetup = true;
        }
    }

    private void ReLinkSubSlots()
    {
        if (slots == null) return;
        foreach (InventorySlot slot in slots)
        {
            if (slot != null && slot.isMaster && slot.item != null)
            {
                PlaceItemAt(slot.index, slot.item, slot.rarity, slot.amount, slot.currentWidth, slot.currentHeight, slot.isRotated);
            }
        }
        UpdateInventoryUI();
    }

    private class InitialItemData
    {
        public int index;
        public Item item;
        public CrystalRarity rarity;
        public int amount;
        public int w;
        public int h;
        public bool rot;
    }

    private void SetupInitialItems()
    {
        List<InitialItemData> initialItems = new List<InitialItemData>();
        foreach (InventorySlot slot in slots)
        {
            if (slot != null && slot.item != null && (slot.isMaster || slot.masterSlot == null))
            {
                initialItems.Add(new InitialItemData
                {
                    index = slot.index,
                    item = slot.item,
                    rarity = slot.rarity,
                    amount = slot.amount,
                    w = slot.currentWidth > 0 ? slot.currentWidth : slot.item.width,
                    h = slot.currentHeight > 0 ? slot.currentHeight : slot.item.height,
                    rot = slot.isRotated
                });
            }
        }

        foreach (InventorySlot slot in slots) if (slot != null) slot.ClearSlot();

        foreach (var data in initialItems)
        {
            if (CanPlaceAt(data.index, data.w, data.h))
            {
                PlaceItemAt(data.index, data.item, data.rarity, data.amount, data.w, data.h, data.rot);
            }
        }
    }

    public void RefreshSlots()
    {
        if (slotsParent != null)
        {
            slots = slotsParent.GetComponentsInChildren<InventorySlot>(true);
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].index = i;
            }
            Debug.Log($"InventoryManager: Refreshed {slots.Length} slots on {slotsParent.name}.");
        }
        else
        {
            slots = new InventorySlot[0];
            Debug.LogWarning("InventoryManager: RefreshSlots called but slotsParent is null.");
        }
    }

    public bool HasWeapon()
    {
        if (slots == null) return false;
        foreach (InventorySlot slot in slots)
        {
            if (slot != null && !slot.IsEmpty() && slot.isMaster && slot.item != null && slot.item.isWeapon) return true;
        }
        return false;
    }

    public bool AddItem(Item item, CrystalRarity rarity)
    {
        return AddItemAmount(item, rarity, 1);
    }

    public bool AddItemAmount(Item item, CrystalRarity rarity, int quantity, int forcedW = -1, int forcedH = -1, bool rotated = false)
    {
        if (item == null) return false;
        
        RefreshSlots();
        if (slots == null || slots.Length == 0) 
        {
            Debug.LogWarning("InventoryManager: AddItemAmount failed - no active slots!");
            return false;
        }

        int remaining = quantity;
        string normalizedTargetName = NormalizeItemName(item.name);
        
        int w = forcedW != -1 ? forcedW : item.width;
        int h = forcedH != -1 ? forcedH : item.height;

        if (w == 1 && h == 1)
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot == null) continue;
                if (!slot.IsEmpty() && slot.isMaster && slot.item != null && 
                    (slot.item == item || NormalizeItemName(slot.item.name) == normalizedTargetName))
                {
                    if (slot.amount < maxStackPerSlot)
                    {
                        remaining = slot.AddToStack(remaining);
                        if (remaining <= 0) break;
                    }
                }
            }
        }

        while (remaining > 0)
        {
            int foundIndex = FindEmptySpace(w, h);
            if (foundIndex != -1)
            {
                int amountToPut = (w == 1 && h == 1) ? Mathf.Min(maxStackPerSlot, remaining) : 1;
                PlaceItemAt(foundIndex, item, rarity, amountToPut, w, h, rotated);
                remaining -= amountToPut;
                if (w > 1 || h > 1) break; 
            }
            else 
            {
                Debug.LogWarning($"InventoryManager: No space for {item.name}");
                break;
            }
        }

        UpdateInventoryUI();
        return remaining < quantity;
    }

    public static string NormalizeItemName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "";
        string normalized = name.Replace("(Clone)", "");
        normalized = normalized.Replace("Variant", "");
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\d", ""); 
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"[_\-\s]", "");
        return normalized.Trim().ToLower();
    }

    public int FindEmptySpace(int w, int h)
    {
        for (int r = 0; r <= rows - h; r++)
        {
            for (int c = 0; c <= columns - w; c++)
            {
                int index = r * columns + c;
                if (CanPlaceAt(index, w, h)) return index;
            }
        }
        return -1;
    }

    public bool CanPlaceAt(int index, int w, int h, InventorySlot ignoreMaster = null)
    {
        int startR = index / columns;
        int startC = index % columns;
        if (startC + w > columns || startR + h > rows) return false;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int checkIndex = (startR + y) * columns + (startC + x);
                if (checkIndex >= slots.Length) return false;
                
                InventorySlot slot = slots[checkIndex];
                if (!slot.IsEmpty())
                {
                    if (ignoreMaster != null && slot.masterSlot == ignoreMaster) continue;
                    return false;
                }
            }
        }
        return true;
    }

    public void PlaceItemAt(int index, Item item, CrystalRarity rarity, int amount, int w = -1, int h = -1, bool rotated = false)
    {
        int startR = index / columns;
        int startC = index % columns;
        InventorySlot master = slots[index];

        int itemW = w != -1 ? w : item.width;
        int itemH = h != -1 ? h : item.height;

        master.isRotated = rotated;
        master.SetItem(item, rarity, amount, itemW, itemH);
        master.isMaster = true;
        master.masterSlot = master;

        for (int y = 0; y < itemH; y++)
        {
            for (int x = 0; x < itemW; x++)
            {
                if (x == 0 && y == 0) continue;
                int currentC = startC + x;
                int currentR = startR + y;
                if (currentC < columns && currentR < rows)
                {
                    int subIndex = currentR * columns + currentC;
                    if (subIndex < slots.Length)
                    {
                        InventorySlot subSlot = slots[subIndex];
                        subSlot.masterSlot = master;
                        subSlot.item = item;
                        subSlot.rarity = rarity;
                        subSlot.isMaster = false;
                        subSlot.currentWidth = itemW;
                        subSlot.currentHeight = itemH;
                        subSlot.isRotated = rotated;
                        subSlot.UpdateSlotUI();
                    }
                }
            }
        }
    }

    public void RemoveItem(InventorySlot master)
    {
        if (master == null || !master.isMaster || master.item == null) return;
        int startR = master.index / columns;
        int startC = master.index % columns;
        int w = master.currentWidth;
        int h = master.currentHeight;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int currentC = startC + x;
                int currentR = startR + y;
                if (currentC < columns && currentR < rows)
                {
                    int subIndex = currentR * columns + currentC;
                    if (subIndex < slots.Length) slots[subIndex].ClearSlot();
                }
            }
        }
    }

    public bool MoveOrStackSlot(InventorySlot fromSlot, InventorySlot toSlot)
    {
        if (fromSlot == null || toSlot == null) return false;
        InventorySlot master = fromSlot.masterSlot;
        if (master == null || master.item == null) return false;

        Item item = master.item;
        CrystalRarity rarity = master.rarity;
        int amount = master.amount;
        int w = master.currentWidth;
        int h = master.currentHeight;
        bool rotated = master.isRotated;

        if (CanPlaceAt(toSlot.index, w, h, master))
        {
            RemoveItem(master);
            PlaceItemAt(toSlot.index, item, rarity, amount, w, h, rotated);
            UpdateInventoryUI();
            return true;
        }

        InventorySlot targetMaster = toSlot.masterSlot;
        if (targetMaster != null && targetMaster != master && targetMaster.item != null && 
            (targetMaster.item == item || NormalizeItemName(targetMaster.item.name) == NormalizeItemName(item.name)) && w == 1 && h == 1)
        {
            int remaining = targetMaster.AddToStack(amount);
            if (remaining <= 0) RemoveItem(master);
            else { master.amount = remaining; master.UpdateSlotUI(); }
            UpdateInventoryUI();
            return true;
        }
        return false;
    }

    public void UpdateInventoryUI()
    {
        // Sync with PlayerStats
        if (PlayerStats.Instance != null)
        {
            // If we have no money in manager but stats has money (e.g. after scene load), restore it
            if (currentMoney == 0 && PlayerStats.Instance.money > 0) 
            {
                currentMoney = PlayerStats.Instance.money;
            }
            else 
            {
                // Otherwise manager is the source of truth (selling items updates manager first)
                PlayerStats.Instance.money = currentMoney;
            }
        }

        RefreshSlots();
        int totalInventoryValue = 0;
float totalWeight = 0f;
        int usedSlots = 0;
        int totalSlots = slots != null ? slots.Length : 0;

        if (slots != null)
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot != null)
                {
                    if (slot.isMaster && !slot.IsEmpty() && slot.item != null)
                    {
                        totalInventoryValue += slot.item.value * slot.amount;
                        totalWeight += slot.item.weight * slot.amount;
                    }
                    if (!slot.IsEmpty()) usedSlots++;
                }
            }
        }

        if (moneyVariable != null) moneyVariable.text = currentMoney.ToString() + " €";
        if (weightVariable != null) weightVariable.text = totalWeight.ToString("0.#") + " / " + maxWeight.ToString("0.#") + " KG";
        if (slotsVariable != null) slotsVariable.text = usedSlots.ToString() + " / " + totalSlots.ToString();
    }

    public void SellAllNonWeapons()
    {
        RefreshSlots();
        if (slots != null)
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot != null && slot.isMaster && !slot.IsEmpty() && slot.item != null && !slot.item.isWeapon)
                {
                    currentMoney += slot.item.value * slot.amount;
                    RemoveItem(slot);
                }
            }
        }
        UpdateInventoryUI();
    }

    public void UpdateMoney() { UpdateInventoryUI(); }
}
