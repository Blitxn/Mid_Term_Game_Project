using UnityEngine;
using System.Collections;

public class Hazard : MonoBehaviour
{
    [Header("Hazard Settings")]
    [SerializeField] private float respawnDelay = 0.5f;

    [Header("Respawn Settings")]
    [SerializeField] private float playerRespawnDelay = 0f;

    private bool playerHit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (playerHit)
            return;

        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        playerHit = true;

        Debug.Log("[Hazard] Neko touched " + gameObject.name);

        StartCoroutine(RespawnPlayer(player));
    }

    private IEnumerator RespawnPlayer(PlayerController player)
    {
        if (playerRespawnDelay > 0f)
            yield return new WaitForSeconds(playerRespawnDelay);

        if (player != null)
            player.Respawn();

        yield return new WaitForSeconds(respawnDelay);

        playerHit = false;
    }
}
