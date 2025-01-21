using UnityEngine;

public class BulletTime : MonoBehaviour
{
    public float slowDownFactor = 0.5f; // Facteur de ralentissement
    public float duration = 2f; // Durée du ralenti

    private bool isActive = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !isActive) // Active le ralenti avec F
        {
            StartCoroutine(ActivateBulletTime());
        }
    }

    private System.Collections.IEnumerator ActivateBulletTime()
    {
        isActive = true;

        // Ralentit le temps
        Time.timeScale = slowDownFactor;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(duration);

        // Remet le temps à la normale
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        isActive = false;
    }
}
