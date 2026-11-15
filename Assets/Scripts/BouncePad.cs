
using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private float bounceForce = 12f;

    [Header("Optional Settings")]
    [SerializeField] private bool resetHorizontalVelocity = false;

    private void OnTriggerEnter(Collider other)
    {
        // Find PlayerController on the object or its parent
        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        Rigidbody playerRb =
            player.GetComponent<Rigidbody>();

        if (playerRb == null)
            return;

        // Launch Neko upward
        Vector3 velocity = playerRb.linearVelocity;

        if (resetHorizontalVelocity)
        {
            velocity.x = 0f;
            velocity.z = 0f;
        }

        velocity.y = bounceForce;
        playerRb.linearVelocity = velocity;

        Debug.Log("[BouncePad] Neko bounced!");
    }
}
