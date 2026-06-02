using UnityEngine;
using System.Collections;

public class Trees_controller : MonoBehaviour
{
    public GameObject line1;
    public GameObject line2;

    public float activeTime = 10f;
    public float inactiveTime = 5f;

    void Start()
    {
        StartCoroutine(Lines());
    }

    IEnumerator Lines()
    {
        while (true)
        {
            yield return new WaitForSeconds(inactiveTime);

            // Vertical ON
            line1.SetActive(true);
            line2.SetActive(false);

            yield return new WaitForSeconds(activeTime);

            // Ambas OFF
            line1.SetActive(false);
            line2.SetActive(false);

            yield return new WaitForSeconds(inactiveTime);

            // Horizontal ON
            line1.SetActive(false);
            line2.SetActive(true);

            yield return new WaitForSeconds(activeTime);

            // Ambas OFF
            line1.SetActive(false);
            line2.SetActive(false);

            yield return new WaitForSeconds(inactiveTime);

            // Vertical ON
            line1.SetActive(true);
            line2.SetActive(true);

            yield return new WaitForSeconds(inactiveTime);
        }
    }
}