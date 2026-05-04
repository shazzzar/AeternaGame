using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Item Data")]
    public Item item;
    public CrystalRarity rarity;
    public int amount;

    [Header("UI")]
    public Image slotImage;
    public TMP_Text amountText;

    private Canvas rootCanvas;
    private GameObject dragIcon;

    private Item dragStartItem;
    private CrystalRarity dragStartRarity;
    private int dragStartAmount;

    void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();

        if (slotImage == null)
        {
            Transform imageChild = transform.Find("Image");

            if (imageChild != null)
                slotImage = imageChild.GetComponent<Image>();
        }

        if (amountText == null)
        {
            Transform amountChild = transform.Find("AmountText");

            if (amountChild != null)
                amountText = amountChild.GetComponent<TMP_Text>();
        }

        if (amountText != null)
        {
            amountText.text = "";
            amountText.gameObject.SetActive(false);
        }

        UpdateSlotUI();
    }

    public bool IsEmpty()
    {
        return item == null || amount <= 0;
    }

    public bool CanStack(CrystalRarity newRarity)
    {
        return !IsEmpty()
            && rarity == newRarity
            && amount < InventoryManager.Instance.maxStackPerSlot;
    }

    public int AddToStack(int quantity)
    {
        int maxStack = InventoryManager.Instance.maxStackPerSlot;
        int freeSpace = maxStack - amount;
        int amountToAdd = Mathf.Min(freeSpace, quantity);

        amount += amountToAdd;

        UpdateSlotUI();

        return quantity - amountToAdd;
    }

    public void SetItem(Item newItem, CrystalRarity newRarity, int newAmount)
    {
        item = newItem;
        rarity = newRarity;
        amount = newItem == null ? 0 : newAmount;

        UpdateSlotUI();
    }

    public void ClearSlot()
    {
        item = null;
        amount = 0;

        UpdateSlotUI();
    }

    public void UpdateSlotUI()
    {
        if (slotImage == null)
        {
            Debug.LogError("SlotImage não atribuído no slot: " + gameObject.name, this);
            return;
        }

        if (!IsEmpty())
        {
            slotImage.sprite = item.item_image;
            slotImage.color = Color.white;
            slotImage.enabled = true;
        }
        else
        {
            slotImage.sprite = null;
            slotImage.color = new Color(1f, 1f, 1f, 0f);
            slotImage.enabled = true;
        }

        if (amountText == null)
        {
            Transform amountChild = transform.Find("AmountText");

            if (amountChild != null)
                amountText = amountChild.GetComponent<TMP_Text>();
        }

        if (amountText != null)
        {
            bool showAmount = !IsEmpty() && amount > 1;

            amountText.gameObject.SetActive(showAmount);
            amountText.text = showAmount ? amount.ToString() : "";
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsEmpty()) return;

        dragStartItem = item;
        dragStartRarity = rarity;
        dragStartAmount = amount;

        CreateDragIcon();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            dragIcon.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        InventorySlot targetSlot = GetSlotUnderMouse(eventData);

        bool moved = false;

        if (targetSlot != null && targetSlot != this)
        {
            moved = InventoryManager.Instance.MoveOrStackSlot(this, targetSlot);
        }

        // Segurança: se o drag falhar, restaura o item
        if (!moved && IsEmpty() && dragStartItem != null)
        {
            SetItem(dragStartItem, dragStartRarity, dragStartAmount);
        }

        if (dragIcon != null)
        {
            Destroy(dragIcon);
            dragIcon = null;
        }

        UpdateSlotUI();

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.UpdateInventoryUI();
        }
    }

    private InventorySlot GetSlotUnderMouse(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            InventorySlot slot = result.gameObject.GetComponentInParent<InventorySlot>();

            if (slot != null)
                return slot;
        }

        return null;
    }

    private void CreateDragIcon()
    {
        if (rootCanvas == null) return;
        if (item == null) return;
        if (item.item_image == null) return;
        if (slotImage == null) return;

        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(rootCanvas.transform, false);
        dragIcon.transform.SetAsLastSibling();

        Image img = dragIcon.AddComponent<Image>();
        img.sprite = item.item_image;
        img.color = Color.white;
        img.raycastTarget = false;

        RectTransform rect = dragIcon.GetComponent<RectTransform>();
        rect.sizeDelta = slotImage.rectTransform.sizeDelta;
        rect.position = Input.mousePosition;
    }
}