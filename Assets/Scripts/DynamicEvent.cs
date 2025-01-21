using UnityEngine;

public class DynamicEvent : MonoBehaviour
{
    public float triggerRadius = 10f;
    public GameObject eventPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Instantiate(eventPrefab, transform.position, Quaternion.identity);
            Debug.Log("Événement déclenché !");
        }
    }
}
