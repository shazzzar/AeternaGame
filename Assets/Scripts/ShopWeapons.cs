using UnityEngine;

public class ShopWeapons : MonoBehaviour
{
    public float detectionRadius = 3f;
    public GameObject shopPanel;

    private bool playerInside = false;

    void Update()
    {
        playerInside = false;

        // detetar jogador por proximidade usando tag
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                playerInside = true;
                break;
            }
        }

        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            OpenShop();
        }
    }

    void OpenShop()
    {
        ShopManager.shopOpen = true;
        shopPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f; // PAUSAR
    }

    public void CloseShop()
    {
        ShopManager.shopOpen = false;
        shopPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f; // DESPAUSAR
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
