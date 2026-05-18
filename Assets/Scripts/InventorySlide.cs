using UnityEngine;

public class InventorySlide : MonoBehaviour
{
    public static bool IsInventoryOpen = false;

    [Header("Panel")]
    public RectTransform panel;

    [Header("Slide")]
    public float speed = 10f;
    public float hiddenOffsetX = 900f;

    private Vector2 visiblePos;
    private Vector2 hiddenPos;
    private bool open = false;

    private CanvasGroup canvasGroup;
    private PlayerController playerController;

    void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();

        if (panel == null)
        {
            Debug.LogError("InventorySlide: Panel n�o atribu�do!");
            return;
        }

        canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.gameObject.AddComponent<CanvasGroup>();

        visiblePos = panel.anchoredPosition;

        hiddenPos = visiblePos + new Vector2(hiddenOffsetX, 0f);

        open = false;
        IsInventoryOpen = false;

        panel.gameObject.SetActive(true);
        panel.anchoredPosition = hiddenPos;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        UpdateCursorState();
    }

    void Update()
    {
        if (panel == null) return;

        if (Input.GetKeyDown(KeyCode.I) && Time.timeScale > 0) 
        {
            ToggleInventory(!open);
        }

        Vector2 targetPos = open ? visiblePos : hiddenPos;

        panel.anchoredPosition = Vector2.Lerp(
            panel.anchoredPosition,
            targetPos,
            Time.unscaledDeltaTime * speed
        );

        canvasGroup.alpha = Mathf.Lerp(
            canvasGroup.alpha,
            open ? 1f : 0f,
            Time.unscaledDeltaTime * speed
        );
    }

    public void ToggleInventory(bool state)
    {
        open = state;
        IsInventoryOpen = open;

        canvasGroup.interactable = open;
        canvasGroup.blocksRaycasts = open;

        UpdateCursorState();
    }

    void UpdateCursorState()
{
        if (open)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            if (playerController != null)
                playerController.UpdateCursorState();
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}