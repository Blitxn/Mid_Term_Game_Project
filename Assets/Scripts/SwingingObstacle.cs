using UnityEngine;

public class SwingingObstacle : MonoBehaviour
{
    [Header("Swing Settings")]
    [SerializeField] private float swingAngle = 60f;
    [SerializeField] private float swingSpeed = 2f;

    private Quaternion startRotation;

    private void Start()
    {
        startRotation = transform.localRotation;
    }

    private void Update()
    {
        float angle = Mathf.Sin(Time.time * swingSpeed) * swingAngle;

        transform.localRotation =
            startRotation * Quaternion.Euler(0f, 0f, angle);
    }
}
