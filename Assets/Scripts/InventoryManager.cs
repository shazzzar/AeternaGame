using UnityEngine;
using TMPro;

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

    [Header("Stack")]
    public int maxStackPerSlot = 5;

    [Header("Weight")]
    public float maxWeight = 105f;

    void Awake()
    {
        Instance = this;
        RefreshSlots();
    }

    void Start()
    {
        RefreshSlots();
        UpdateInventoryUI();
    }

    public void RefreshSlots()
    {
        if (slotsParent != null)
        {
            slots = slotsParent.GetComponentsInChildren<InventorySlot>(true);
        }
    }

    public bool AddItem(Item item, CrystalRarity rarity)
    {
        return AddItemAmount(item, rarity, 1);
    }

    public bool AddItemAmount(Item item, CrystalRarity rarity, int quantity)
    {
        RefreshSlots();
        if (item == null) return false;

        int remaining = quantity;

        foreach (InventorySlot slot in slots)
        {
            if (slot == null) continue;

            if (!slot.IsEmpty() && slot.item == item && slot.rarity == rarity)
            {
                if (slot.amount < maxStackPerSlot)
                {
                    remaining = slot.AddToStack(remaining);
                    if (remaining <= 0) break;
                }
            }
        }

        if (remaining > 0)
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot == null) continue;

                if (slot.IsEmpty())
                {
                    int amountToPut = Mathf.Min(maxStackPerSlot, remaining);
                    slot.SetItem(item, rarity, amountToPut);
                    remaining -= amountToPut;
                    if (remaining <= 0) break;
                }
            }
        }

        UpdateInventoryUI();
        return remaining <= 0;
    }

    public void CompactStacksByRarity()
    {
        RefreshSlots();

        for (int i = 0; i < slots.Length; i++)
        {
            InventorySlot mainSlot = slots[i];
            if (mainSlot == null || mainSlot.IsEmpty()) continue;

            for (int j = i + 1; j < slots.Length; j++)
            {
                InventorySlot otherSlot = slots[j];
                if (otherSlot == null || otherSlot.IsEmpty()) continue;

                // ADICIONADO: Verificar se é o MESMO item (ScriptableObject)
                if (mainSlot.item == otherSlot.item &&
                    mainSlot.rarity == otherSlot.rarity &&
                    mainSlot.amount < maxStackPerSlot)
                {
                    int remaining = mainSlot.AddToStack(otherSlot.amount);

                    if (remaining <= 0)
                    {
                        otherSlot.ClearSlot();
                    }
                    else
                    {
                        // Se sobrou algo, o otherSlot fica com o resto e o mainSlot fica cheio
                        otherSlot.SetItem(otherSlot.item, otherSlot.rarity, remaining);
                    }
                }
            }
        }
    }

    public bool MoveOrStackSlot(InventorySlot fromSlot, InventorySlot toSlot)
    {
        RefreshSlots();

        if (fromSlot == null || toSlot == null) return false;
        if (fromSlot == toSlot) return false;
        if (fromSlot.IsEmpty()) return false;

        Item fromItem = fromSlot.item;
        CrystalRarity fromRarity = fromSlot.rarity;
        int fromAmount = fromSlot.amount;

        // 1. Slot vazio: move tudo
        if (toSlot.IsEmpty())
        {
            toSlot.SetItem(fromItem, fromRarity, fromAmount);
            fromSlot.ClearSlot();

            CompactStacksByRarity();
            UpdateInventoryUI();
            return true;
        }

        // 2. Mesma raridade: faz stack
        if (toSlot.rarity == fromRarity &&
            toSlot.amount < maxStackPerSlot)
        {
            int remaining = toSlot.AddToStack(fromAmount);

            if (remaining <= 0)
            {
                fromSlot.ClearSlot();
            }
            else
            {
                fromSlot.SetItem(fromItem, fromRarity, remaining);
            }

            CompactStacksByRarity();
            UpdateInventoryUI();
            return true;
        }

        // 3. Raridade diferente ou slot cheio: troca
        Item tempItem = toSlot.item;
        CrystalRarity tempRarity = toSlot.rarity;
        int tempAmount = toSlot.amount;

        toSlot.SetItem(fromItem, fromRarity, fromAmount);
        fromSlot.SetItem(tempItem, tempRarity, tempAmount);

        CompactStacksByRarity();
        UpdateInventoryUI();
        return true;
    }

    public void UpdateInventoryUI()
    {
        RefreshSlots();

        int totalMoney = 0;
        float totalWeight = 0f;
        int usedSlots = 0;
        int totalSlots = slots.Length;

        foreach (InventorySlot slot in slots)
        {
            if (slot != null && !slot.IsEmpty())
            {
                totalMoney += slot.item.value * slot.amount;
                totalMoney += slot.item.value * slot.amount;
                totalWeight += slot.item.weight * slot.amount;
                usedSlots++;
            }
        }

        if (moneyVariable != null)
            moneyVariable.text = totalMoney.ToString() + " €";

        if (weightVariable != null)
            weightVariable.text = totalWeight.ToString("0.#") + " / " + maxWeight.ToString("0.#") + " KG";

        if (slotsVariable != null)
            slotsVariable.text = usedSlots.ToString() + " / " + totalSlots.ToString();
    }

    public void UpdateMoney()
    {
        UpdateInventoryUI();
    }
}