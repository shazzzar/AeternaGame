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

        // Incrementar ronda
        RoundManager.Instance.currentRound++;

        // Despausar o jogo
        Time.timeScale = 1f;

        // Bloquear cursor novamente
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Carregar a scene do jogo
        SceneManager.LoadScene(gameplaySceneName);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
