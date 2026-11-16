using UnityEngine;

public class Collectible : MonoBehaviour
{
    private bool collected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player"))
            return;

        collected = true;

        Debug.Log("Fish Collected!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Collect();
        }

        Destroy(gameObject);
    }
}