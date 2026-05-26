using UnityEngine;

public class MiniMapPlayerIcon : MonoBehaviour
{
    public Transform player;
    public float minimapRotationOffset = 48.5f; // mete aqui o mesmo ângulo que rodaste no UI

    void Update()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, -player.eulerAngles.y + minimapRotationOffset);
    }
}
