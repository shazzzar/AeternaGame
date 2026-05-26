using UnityEngine;
using UnityEngine.SceneManagement;

public class NextRoundBox : MonoBehaviour
{
    public float detectionRadius = 3f;
    public string gameplaySceneName = "SampleScene"; // nome da tua scene de jogo

    private bool playerInside = false;

    void Update()
    {
        playerInside = false;

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
            GoToNextRound();
        }
    }

    void GoToNextRound()
    {
        Debug.Log("A avançar para a próxima ronda...");

        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.NextRound();
        }
        else
        {
            // Fallback if RoundManager is missing
            SceneManager.LoadScene(gameplaySceneName);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
