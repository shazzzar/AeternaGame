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
    public int index;
    public bool isMaster = false;
    public InventorySlot masterSlot;
    
    public int currentWidth = 1;
    public int currentHeight = 1;
    public bool isRotated = false;

    [Header("UI")]
    public Image slotImage;
    public TMP_Text amountText;

    private Canvas rootCanvas;
    private GameObject dragIcon;

    private Item dragStartItem;
    private CrystalRarity dragStartRarity;
    private int dragStartAmount;
    private int dragStartWidth;
    private int dragStartHeight;
    private bool dragStartRotated;
    private Canvas slotCanvas;

    void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        slotCanvas = GetComponent<Canvas>();
        if (slotCanvas == null)
        {
            slotCanvas = gameObject.AddComponent<Canvas>();
        }
        
        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        if (masterSlot == null && isMaster) masterSlot = this;

        if (item != null && isMaster)
        {
            if (currentWidth == 1 && currentHeight == 1 && (item.width != 1 || item.height != 1))
            {
                currentWidth = item.width;
                currentHeight = item.height;
            }
        }

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
        return masterSlot == null || masterSlot.item == null || (masterSlot.isMaster && masterSlot.amount <= 0);
    }

    public int AddToStack(int quantity)
    {
        if (!isMaster) return quantity;
        int maxStack = InventoryManager.Instance.maxStackPerSlot;
        int freeSpace = maxStack - amount;
        int amountToAdd = Mathf.Min(freeSpace, quantity);

        amount += amountToAdd;
        UpdateSlotUI();

        return quantity - amountToAdd;
    }

    public void SetItem(Item newItem, CrystalRarity newRarity, int newAmount, int w = -1, int h = -1)
    {
        item = newItem;
        rarity = newRarity;
        amount = newItem == null ? 0 : newAmount;
        isMaster = newItem != null;
        masterSlot = isMaster ? this : null;
        
        if (newItem != null)
        {
            currentWidth = w != -1 ? w : newItem.width;
            currentHeight = h != -1 ? h : newItem.height;
        }
        else
        {
            currentWidth = 1;
            currentHeight = 1;
            isRotated = false;
        }

        UpdateSlotUI();
    }

    public void ClearSlot()
    {
        item = null;
        amount = 0;
        isMaster = false;
        masterSlot = null;
        rarity = CrystalRarity.Common;
        currentWidth = 1;
        currentHeight = 1;
        isRotated = false;

        UpdateSlotUI();
    }

    public void UpdateSlotUI()
    {
        if (slotImage == null) return;

        if (slotCanvas != null)
        {
            slotCanvas.overrideSorting = isMaster && item != null;
            slotCanvas.sortingOrder = slotCanvas.overrideSorting ? 10 : 0;
        }

        if (isMaster && item != null)
{
            slotImage.sprite = item.item_image;
            slotImage.color = Color.white;
            slotImage.enabled = true;
            
            RectTransform rect = slotImage.GetComponent<RectTransform>();
            float cellW = 110f; 
            float cellH = 110f;
            rect.sizeDelta = new Vector2(cellW * currentWidth, cellH * currentHeight);
            rect.pivot = new Vector2(0.5f / currentWidth, 0.5f / currentHeight);
            rect.anchoredPosition = Vector2.zero;
            
            rect.localRotation = isRotated ? Quaternion.Euler(0, 0, -90) : Quaternion.identity;
        }
        else
        {
            slotImage.sprite = null;
            slotImage.color = new Color(1f, 1f, 1f, 0f);
            slotImage.enabled = false;
        }

        if (amountText != null)
        {
            bool showAmount = isMaster && !IsEmpty() && amount > 1;
            amountText.gameObject.SetActive(showAmount);
            amountText.text = showAmount ? amount.ToString() : "";
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsEmpty()) return;

        InventorySlot master = masterSlot;
        if (master == null) return;
        
        dragStartItem = master.item;
        dragStartRarity = master.rarity;
        dragStartAmount = master.amount;
        dragStartWidth = master.currentWidth;
        dragStartHeight = master.currentHeight;
        bool dragStartRotated = master.isRotated;

        CreateDragIcon(dragStartRotated);
        InventoryManager.Instance.RemoveItem(master);
    }

    void Update()
    {
        if (dragIcon != null)
        {
            dragIcon.transform.position = Input.mousePosition;

            if (Input.GetMouseButtonDown(1)) 
            {
                int temp = dragStartWidth;
                dragStartWidth = dragStartHeight;
                dragStartHeight = temp;

                RectTransform rect = dragIcon.GetComponent<RectTransform>();
                float currentAngle = rect.localRotation.eulerAngles.z;
                rect.localRotation = Quaternion.Euler(0, 0, currentAngle - 90);

                UpdateDragIconSize();
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        InventorySlot targetSlot = GetSlotUnderMouse(eventData);
        bool placed = false;

        if (targetSlot != null && dragStartItem != null)
        {
            if (InventoryManager.Instance.CanPlaceAt(targetSlot.index, dragStartWidth, dragStartHeight))
            {
                bool finalRotated = (dragStartWidth != dragStartItem.width);
                if (dragStartItem.width == dragStartItem.height && dragIcon != null)
                {
                    float angle = dragIcon.GetComponent<RectTransform>().localRotation.eulerAngles.z;
                    finalRotated = (Mathf.Abs(angle % 180) > 45); 
                }

                InventoryManager.Instance.PlaceItemAt(targetSlot.index, dragStartItem, dragStartRarity, dragStartAmount, dragStartWidth, dragStartHeight, finalRotated);
                placed = true;
            }
            else if (dragStartWidth == 1 && dragStartHeight == 1)
            {
                InventorySlot targetMaster = targetSlot.masterSlot;
                if (targetMaster != null && targetMaster.item != null && 
                    (targetMaster.item == dragStartItem || InventoryManager.NormalizeItemName(targetMaster.item.name) == InventoryManager.NormalizeItemName(dragStartItem.name)))
                {
                    int remaining = targetMaster.AddToStack(dragStartAmount);
                    if (remaining <= 0) 
                    {
                        placed = true;
                    }
                    else
                    {
                        InventoryManager.Instance.AddItemAmount(dragStartItem, dragStartRarity, remaining, -1, -1, false);
                        placed = true;
                    }
                }
            }
        }

        if (!placed && dragStartItem != null)
        {
            bool wasRotated = (dragStartWidth != dragStartItem.width);
            InventoryManager.Instance.AddItemAmount(dragStartItem, dragStartRarity, dragStartAmount, dragStartWidth, dragStartHeight, wasRotated);
        }

        if (dragIcon != null)
        {
            Destroy(dragIcon);
            dragIcon = null;
        }

        dragStartItem = null;
        dragStartAmount = 0;
        dragStartWidth = 1;
        dragStartHeight = 1;

        InventoryManager.Instance.UpdateInventoryUI();
    }

    private void OnEnable()
    {
        UpdateSlotUI();
    }

    private InventorySlot GetSlotUnderMouse(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (RaycastResult result in results)
        {
            InventorySlot slot = result.gameObject.GetComponentInParent<InventorySlot>();
            if (slot != null) return slot;
        }
        return null;
    }

    private void CreateDragIcon(bool initiallyRotated)
    {
        if (rootCanvas == null || dragStartItem == null) return;
        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(rootCanvas.transform, false);
        dragIcon.transform.SetAsLastSibling();
        Image img = dragIcon.AddComponent<Image>();
        img.sprite = dragStartItem.item_image;
        img.color = new Color(1, 1, 1, 0.7f);
        img.raycastTarget = false;
        
        RectTransform rect = dragIcon.GetComponent<RectTransform>();
        rect.localRotation = initiallyRotated ? Quaternion.Euler(0, 0, -90) : Quaternion.identity;

        UpdateDragIconSize();
    }

    private void UpdateDragIconSize()
    {
        if (dragIcon == null) return;
        RectTransform rect = dragIcon.GetComponent<RectTransform>();
        float cellW = 110f;
        rect.sizeDelta = new Vector2(cellW * dragStartWidth, cellW * dragStartHeight);
    }
}