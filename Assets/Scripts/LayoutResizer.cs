using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class ResponsiveGrid : MonoBehaviour
{
    public int columns = 7;
    public int rows = 3;
    public float spacing = 5f;

    private GridLayoutGroup grid;
    private RectTransform rectTransform;

    void Start()
    {
        grid = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
        UpdateGrid();
    }

    void Update()
    {
        UpdateGrid();
    }

    void UpdateGrid()
    {
        if (rectTransform == null || grid == null) return;
        float parentWidth = rectTransform.rect.width - grid.padding.left - grid.padding.right;
        float parentHeight = rectTransform.rect.height - grid.padding.top - grid.padding.bottom;
        float totalSpacingWidth = spacing * (columns - 1);
        float totalSpacingHeight = spacing * (rows - 1);
        float cellWidth = (parentWidth - totalSpacingWidth) / columns;
        float cellHeight = (parentHeight - totalSpacingHeight) / rows;
        float size = Mathf.Min(cellWidth, cellHeight);
        grid.cellSize = new Vector2(size, size);
        grid.spacing = new Vector2(spacing, spacing);
    }
}