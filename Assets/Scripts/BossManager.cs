using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Drives the Boss round flow:
///  1. The boss enemy stays hidden until the trigger mineral is mined.
///  2. Mining the mineral (which destroys the Crystal) activates the boss.
///  3. When the boss is defeated (its GameObject is destroyed by EnemyAI),
///     the screen fades to black and the Main Menu scene is loaded.
/// </summary>
public class BossManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Crystal the player must mine to trigger the boss. It is destroyed when mined.")]
    public Crystal triggerCrystal;

    [Tooltip("The boss enemy. Hidden on start, activated when the mineral is mined, watched for defeat.")]
    public GameObject bossEnemy;

    [Tooltip("Full-screen black CanvasGroup used for the fade-out.")]
    public CanvasGroup fadeCanvasGroup;

    [Header("Center Trees")]
    [Tooltip("Trees in the center of the arena that disappear when the mineral is mined.")]
    public GameObject[] centerTrees;

    [Header("Boss Health Bar")]
    [Tooltip("CanvasGroup wrapping the giant boss HP bar. Hidden until the boss appears.")]
    public CanvasGroup bossHealthBarGroup;

    [Tooltip("Filled Image representing the boss's remaining health (fillAmount 0..1).")]
    public UnityEngine.UI.Image bossHealthFill;

    [Header("Settings")]
    public string menuSceneName = "MainMenu";
    public float fadeDuration = 2f;

    private bool _hadCrystal;
    private bool _bossSpawned;
    private bool _bossDefeated;

    private EnemyAI _bossAI;
    private float _bossMaxHealth = 1f;

    private void Start()
    {
        _hadCrystal = triggerCrystal != null;

        if (bossEnemy != null)
            bossEnemy.SetActive(false);

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        // Boss HP bar stays hidden until the boss appears.
        if (bossHealthBarGroup != null)
            bossHealthBarGroup.alpha = 0f;
    }

    private void Update()
    {
        // Phase 1: wait for the mineral to be mined (Crystal.OnMined destroys it -> reference becomes null).
        if (!_bossSpawned)
        {
            if (_hadCrystal && triggerCrystal == null)
            {
                SpawnBoss();
            }
            return;
        }

        // Phase 2: boss is alive - keep the HP bar in sync.
        if (!_bossDefeated && bossEnemy != null)
        {
            UpdateHealthBar();
            return;
        }

        // Phase 3: boss defeated (EnemyAI destroys the GameObject at 0 health).
        if (!_bossDefeated && bossEnemy == null)
        {
            _bossDefeated = true;

            if (bossHealthFill != null) bossHealthFill.fillAmount = 0f;
            if (bossHealthBarGroup != null) bossHealthBarGroup.alpha = 0f;

            StartCoroutine(FadeAndLoadMenu());
        }
    }

    private void SpawnBoss()
    {
        _bossSpawned = true;

        // Clear the trees in the center of the arena.
        if (centerTrees != null)
        {
            foreach (var tree in centerTrees)
                if (tree != null) tree.SetActive(false);
        }

        if (bossEnemy != null)
        {
            bossEnemy.SetActive(true);
            Debug.Log("Boss mineral mined - the boss has appeared!");

            // Capture the boss's max health for the HP bar.
            _bossAI = bossEnemy.GetComponent<EnemyAI>();
            if (_bossAI != null && _bossAI.health > 0f)
                _bossMaxHealth = _bossAI.health;

            if (bossHealthBarGroup != null) bossHealthBarGroup.alpha = 1f;
            if (bossHealthFill != null) bossHealthFill.fillAmount = 1f;
        }
        else
        {
            // No boss assigned: nothing to fight, end immediately.
            _bossDefeated = true;
            StartCoroutine(FadeAndLoadMenu());
        }
    }

    private void UpdateHealthBar()
    {
        if (bossHealthFill == null || _bossAI == null) return;
        bossHealthFill.fillAmount = Mathf.Clamp01(_bossAI.health / _bossMaxHealth);
    }

    private IEnumerator FadeAndLoadMenu()
    {
        Debug.Log("Boss defeated - fading to black and returning to the main menu.");

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;

            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, fadeDuration);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }

        // Restore time scale and cursor in case anything left them altered.
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(menuSceneName);
    }
}
