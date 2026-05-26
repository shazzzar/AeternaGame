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
        InitializePanel();
    }

    public void InitializePanel()
    {
        if (panel == null)
        {
            // Try to find it if not assigned
            GameObject p = GameObject.Find("InventoryPanel");
            if (p != null) panel = p.GetComponent<RectTransform>();
        }

        if (panel == null) return;

        canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.gameObject.AddComponent<CanvasGroup>();

        visiblePos = new Vector2(0, 0); // Assuming right-aligned with pivot at (1, 0.5)
        hiddenPos = new Vector2(hiddenOffsetX, 0f);

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
        if (panel == null)
        {
            // If we lost the panel (scene transition), try to find it
            GameObject p = GameObject.Find("InventoryPanel");
            if (p != null) 
            {
                panel = p.GetComponent<RectTransform>();
                InitializePanel();
            }
            if (panel == null) return;
        }

        if (Input.GetKeyDown(KeyCode.I)) 
        {
            ToggleInventory(!open);
        }

        Vector2 targetPos = open ? visiblePos : hiddenPos;

        panel.anchoredPosition = Vector2.Lerp(
            panel.anchoredPosition,
            targetPos,
            Time.unscaledDeltaTime * speed
        );

        if (canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.Lerp(
                canvasGroup.alpha,
                open ? 1f : 0f,
                Time.unscaledDeltaTime * speed
            );
        }
    }

    public void ToggleInventory(bool state)
    {
        open = state;
        IsInventoryOpen = open;

        if (canvasGroup != null)
        {
            canvasGroup.interactable = open;
            canvasGroup.blocksRaycasts = open;
        }

        UpdateCursorState();
    }

    void UpdateCursorState()
    {
        bool isShopScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Round_Pause";

        if (open || isShopScene)
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