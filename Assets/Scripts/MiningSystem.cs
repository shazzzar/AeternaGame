using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiningSystem : MonoBehaviour
{
    public float interactionRange = 3f; // Distância para minerar
    public LayerMask crystalLayer; // Camada onde os seus cristais estão
    public Animator playerAnimator; // Seu componente Animator

    private Crystal currentCrystal;
    public bool isMining = false;
    public GameObject pickaxeModel;

    void Update()
    {
        DetectCrystals();

        if (Input.GetKeyDown(KeyCode.E) && currentCrystal != null && !isMining)
        {
            StartCoroutine(MineCrystal());
        }
    }

    void DetectCrystals()
    {
        // Encontra o cristal mais próximo
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange, crystalLayer);

        if (hits.Length > 0)
        {
            Crystal nearest = hits[0].GetComponent<Crystal>();
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

    IEnumerator MineCrystal()
    {
        isMining = true;

        if (pickaxeModel != null) pickaxeModel.SetActive(true);

        // 1. Calculate direction and target rotation
        Vector3 direction = (currentCrystal.transform.position - transform.position);
        direction.y = 0;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // 2. Start the animation
            if (playerAnimator != null) playerAnimator.SetBool("isMining", true);

            // 3. THE FIX: Force rotation during the transition
            // This prevents the animator from snapping the player to a default direction
            float forceDuration = 0.3f; // Adjust this to match your transition time
            float elapsed = 0;
            while (elapsed < forceDuration)
            {
                transform.rotation = targetRotation;
                elapsed += Time.deltaTime;
                yield return null; // Wait for next frame
            }

            // Final snap to ensure alignment
            transform.rotation = targetRotation;
        }

        // 4. Wait for the remainder of the mining duration
        float remainingTime = currentCrystal.GetMiningDuration() - 0.3f;
        if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);

        // 5. Logic for finishing
        if (currentCrystal != null)
        {
            currentCrystal.OnMined();
        }

        if (playerAnimator != null) playerAnimator.SetBool("isMining", false);

        yield return new WaitForSeconds(0.5f);

        if (pickaxeModel != null) pickaxeModel.SetActive(false);

        isMining = false;
    }

    // Apenas para visualizar o alcance no editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
