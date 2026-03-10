using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiningSystem : MonoBehaviour
{
    public float interactionRange = 3f; 
    public LayerMask crystalLayer; 
    public Animator playerAnimator; 

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

        Vector3 direction = (currentCrystal.transform.position - transform.position);
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

        float remainingTime = currentCrystal.GetMiningDuration() - 0.3f;
        if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);

        if (currentCrystal != null)
        {
            Sprite crystalItem = currentCrystal.itemIcon; // Pega o ícone do cristal

            // Tenta adicionar ao inventário
            bool success = InventoryManager.Instance.AddItem(crystalItem);

            if (success)
            {
                currentCrystal.OnMined(); // Só destrói o cristal se houve espaço no inventário
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
