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
            Debug.LogError("InventorySlide: Panel não atribuído!");
            return;
        }

        canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.gameObject.AddComponent<CanvasGroup>();

        // A posição onde meteste o painel no Editor é a posição aberta
        visiblePos = panel.anchoredPosition;

        // Posição escondida para a direita
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

        if (Input.GetKeyDown(KeyCode.I))
        {
            open = !open;
            IsInventoryOpen = open;

            canvasGroup.interactable = open;
            canvasGroup.blocksRaycasts = open;

            UpdateCursorState();
        }

        Vector2 targetPos = open ? visiblePos : hiddenPos;

        panel.anchoredPosition = Vector2.Lerp(
            panel.anchoredPosition,
            targetPos,
            Time.deltaTime * speed
        );

        // Fade opcional
        canvasGroup.alpha = Mathf.Lerp(
            canvasGroup.alpha,
            open ? 1f : 0f,
            Time.deltaTime * speed
        );
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