using UnityEngine;

public class BladeMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3 movementDirection = Vector3.right;
    [SerializeField] private float moveDistance = 4f;
    [SerializeField] private float moveSpeed = 2f;

    private Vector3 startPosition;
    private float elapsedTime;

    private void Start()
    {
        // Save the starting position
        startPosition = transform.position;
    }

    private void Update()
    {
        // Calculate movement distance
        elapsedTime += Time.deltaTime;

        float distance = Mathf.PingPong(
            elapsedTime * moveSpeed,
            moveDistance
        );

        // Move left and right continuously
        transform.position = startPosition +
            movementDirection.normalized * distance;
    }
}
