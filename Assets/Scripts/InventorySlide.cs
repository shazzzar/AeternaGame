using UnityEngine;

public class InventorySlide : MonoBehaviour
{
    public RectTransform panel;
    public float speed = 10f;

    private Vector2 hiddenPos;
    private Vector2 visiblePos;
    private bool open = false;

    void Start()
    {
        float panelWidth = panel.rect.width;

        visiblePos = new Vector2(0, 0);
        hiddenPos = new Vector2(panelWidth, 0);

        panel.anchoredPosition = hiddenPos;
        SetCursorState(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            open = !open;
            SetCursorState(open); 
        }

        if (!open)
        {
            hiddenPos.x = panel.rect.width;
        }

        panel.anchoredPosition = Vector2.Lerp(
            panel.anchoredPosition,
            open ? visiblePos : hiddenPos,
            Time.deltaTime * speed
        );
    }

    void SetCursorState(bool isOpen)
    {
        if (isOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}