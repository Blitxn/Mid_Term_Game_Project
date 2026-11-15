using UnityEngine;
using System.Collections;

public class RollingObstacle : MonoBehaviour
{
    [Header("Movement Points")]
    [SerializeField] private Transform topPoint;
    [SerializeField] private Transform bottomPoint;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float pauseAtTop = 1f;
    [SerializeField] private float pauseAtBottom = 0.5f;

    [Header("Rolling Settings")]
    [SerializeField] private Vector3 rotationAxis = Vector3.right;
    [SerializeField] private float rotationSpeed = 360f;

    private bool movingDown = true;
    private bool isMoving = false;

    private void Start()
    {
        if (topPoint == null || bottomPoint == null)
        {
            Debug.LogError(
                "Missing points on GameObject: " + gameObject.name
            );
        }

        transform.position = topPoint.position;
        StartCoroutine(RollingLoop());
    }

    private void Update()
    {
        if (!isMoving)
            return;

        Vector3 target = movingDown
            ? bottomPoint.position
            : topPoint.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        float direction = movingDown ? 1f : -1f;

        transform.Rotate(
            rotationAxis.normalized,
            rotationSpeed * direction * Time.deltaTime,
            Space.Self
        );
    }

    private IEnumerator RollingLoop()
    {
        while (true)
        {
            isMoving = false;

            if (movingDown)
                yield return new WaitForSeconds(pauseAtTop);
            else
                yield return new WaitForSeconds(pauseAtBottom);

            isMoving = true;

            Vector3 target = movingDown
                ? bottomPoint.position
                : topPoint.position;

            while (Vector3.Distance(transform.position, target) > 0.01f)
                yield return null;

            transform.position = target;
            movingDown = !movingDown;
        }
    }
}
