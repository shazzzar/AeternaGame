using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public Image[] slots; 

    void Awake() { Instance = this; }

    public bool AddItem(Sprite icon)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].sprite == null || !slots[i].gameObject.activeSelf)
            {
                slots[i].sprite = icon;
                slots[i].gameObject.SetActive(true);
                slots[i].color = Color.white;
                return true;
            }
        }
        Debug.Log("Inventário Cheio!");
        return false;
    }
}