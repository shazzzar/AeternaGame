using System.Collections;
using UnityEngine;

public class MiningSystem : MonoBehaviour
{
    public float interactionRange = 3f;
    public LayerMask crystalLayer;
    public Animator playerAnimator;

    private Crystal currentCrystal;
    public bool isMining = false;
    public GameObject pickaxeModel;
    
    [Header("Audio")]
    public AudioClip mineFinishedSound;
    public AudioClip mineSwingSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
{
        DetectCrystals();

        if (Input.GetKeyDown(KeyCode.E) && currentCrystal != null && !isMining)
            StartCoroutine(MineCrystal());
    }

    void DetectCrystals()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange, crystalLayer);

        if (hits.Length > 0)
        {
            Crystal nearest = hits[0].GetComponent<Crystal>();
            if (nearest == null) return;

            if (currentCrystal != nearest)
            {
                if (currentCrystal != null) currentCrystal.ShowPrompt(false);
                currentCrystal = nearest;
                currentCrystal.ShowPrompt(true);
            }
        }
        else
        {
            if (currentCrystal != null)
            {
                currentCrystal.ShowPrompt(false);
                currentCrystal = null;
            }
        }
    }

    public void PlayMiningSwingSound()
    {
        if (mineSwingSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(mineSwingSound);
        }
    }

    IEnumerator MineCrystal()
    {
        isMining = true;

        Crystal crystalToMine = currentCrystal;

        if (pickaxeModel != null) pickaxeModel.SetActive(true);

        if (crystalToMine != null)
        {
            Vector3 direction = crystalToMine.transform.position - transform.position;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                if (playerAnimator != null) playerAnimator.SetBool("isMining", true);

                float forceDuration = 0.3f;
                float elapsed = 0;

                while (elapsed < forceDuration)
                {
                    transform.rotation = targetRotation;
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                transform.rotation = targetRotation;
            }

            float baseDuration = crystalToMine.GetMiningDuration();
            float multiplier = (PlayerStats.Instance != null) ? PlayerStats.Instance.miningSpeedMultiplier : 1f;
            float actualDuration = baseDuration / multiplier;

            // The sound is now handled by the Animation Event calling PlayMiningSwingSound()
            // We just wait for the mining to complete
            yield return new WaitForSeconds(actualDuration);

            if (crystalToMine != null)
            {
                crystalToMine.OnMined();
                if (mineFinishedSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(mineFinishedSound);
                }
            }
            }

        if (playerAnimator != null) playerAnimator.SetBool("isMining", false);

        yield return new WaitForSeconds(0.5f);

        if (pickaxeModel != null) pickaxeModel.SetActive(false);

        isMining = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
