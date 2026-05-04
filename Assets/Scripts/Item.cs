using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    [Header("Visual")]
    public Sprite item_image;

    [Header("Stats")]
    public int value;
    public float weight;

    [Header("Inventory")]
    public int slot_size;
}