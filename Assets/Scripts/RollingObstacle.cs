using UnityEngine;
using System.Collections;

public class RollingObstacle : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float travelDistance = 5f;
    [SerializeField] private bool moveTowardPositiveZ = true;
    [SerializeField] private bool invertSpinDirection = false;
    [SerializeField] private bool stopAtMapEdge = true;

    [Header("Roller Grounding")]
    [SerializeField] private float radius = 0.5f;
    [Tooltip("Negative lowers onto the ground/ramp; positive raises it.")]
    [SerializeField] private float heightOffset = 0f;
    [SerializeField] private float groundSnapSpeed = 20f;

    [Header("Ground & Boundary Detection")]
    [SerializeField] private LayerMask groundLayer = ~0;
    [Tooltip("Optional wall/boundary layer so the roller turns back before leaving the map.")]
    [SerializeField] private LayerMask wallLayer = 0;
    [SerializeField] private float raycastHeight = 10f;
    [SerializeField] private float wallCheckDistance = 0.75f;

    [Header("Rolling Rotation")]
    [Tooltip("Local axis of the roller mesh that acts as its axle (e.g. (1,0,0), (0,1,0), or (0,0,1)).")]
    [SerializeField] private Vector3 localAxle = Vector3.right;

    [Header("Timing")]
    [SerializeField] private float startDelay = 0f;
    [SerializeField] private float pauseAtEnd = 1f;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 smoothedGroundPoint;
    private bool initializedGround;

    private void Start()
    {
        startPosition = transform.position;
        float direction = moveTowardPositiveZ ? 1f : -1f;
        Vector3 desiredEnd = startPosition + new Vector3(0f, 0f, direction * travelDistance);

        // Clamp endPosition so it doesn't extend past where ground exists or into walls
        endPosition = ClampPathEndpoint(startPosition, desiredEnd);

        if (SampleGround(startPosition, out float startGroundY))
        {
            smoothedGroundPoint = new Vector3(startPosition.x, startGroundY, startPosition.z);
            initializedGround = true;
            transform.position = new Vector3(
                smoothedGroundPoint.x,
                smoothedGroundPoint.y + radius + heightOffset,
                smoothedGroundPoint.z
            );
        }
        else
        {
            smoothedGroundPoint = new Vector3(startPosition.x, startPosition.y - radius, startPosition.z);
        }

        StartCoroutine(RollingLoop());
    }

    private Vector3 ClampPathEndpoint(Vector3 from, Vector3 to)
    {
        Vector3 delta = to - from;
        float totalDist = delta.magnitude;
        if (totalDist <= 0.001f)
            return from;

        Vector3 dir = delta.normalized;
        float safeDist = totalDist;

        // 1. Check walls if wallLayer is assigned
        if (wallLayer.value != 0)
        {
            if (Physics.Raycast(transform.position, dir, out RaycastHit wallHit, totalDist + wallCheckDistance, wallLayer, QueryTriggerInteraction.Ignore))
            {
                safeDist = Mathf.Min(safeDist, Mathf.Max(0f, wallHit.distance - wallCheckDistance));
            }
        }

        // 2. Step along path to ensure ground exists and doesn't drop off the map
        if (stopAtMapEdge)
        {
            int steps = Mathf.Max(1, Mathf.CeilToInt(safeDist / 0.25f));
            float stepSize = safeDist / steps;

            for (int i = 1; i <= steps; i++)
            {
                Vector3 testPos = from + dir * (stepSize * i);
                if (!SampleGround(testPos, out _))
                {
                    safeDist = Mathf.Max(0f, stepSize * (i - 1));
                    break;
                }
            }
        }

        return from + dir * safeDist;
    }

    private IEnumerator RollingLoop()
    {
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        while (true)
        {
            yield return RollTo(startPosition, endPosition);
            if (pauseAtEnd > 0f)
                yield return new WaitForSeconds(pauseAtEnd);

            yield return RollTo(endPosition, startPosition);
            if (pauseAtEnd > 0f)
                yield return new WaitForSeconds(pauseAtEnd);
        }
    }

    private IEnumerator RollTo(Vector3 from, Vector3 to)
    {
        float totalDistance = Vector3.Distance(from, to);
        float speed = Mathf.Max(moveSpeed, 0.01f);
        float duration = Mathf.Max(totalDistance / speed, 0.001f);

        float timer = 0f;
        Vector3 previousPosition = transform.position;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            Vector3 pathTarget = Vector3.Lerp(from, to, t);

            // Prevent stepping into a wall mid-roll
            if (wallLayer.value != 0)
            {
                Vector3 stepVec = pathTarget - transform.position;
                float stepLen = stepVec.magnitude;
                if (stepLen > 0.0001f && Physics.Raycast(transform.position, stepVec.normalized, out RaycastHit hit, stepLen + wallCheckDistance, wallLayer, QueryTriggerInteraction.Ignore))
                {
                    pathTarget = transform.position + stepVec.normalized * Mathf.Max(0f, hit.distance - wallCheckDistance);
                }
            }

            StepRoller(pathTarget, previousPosition, Time.deltaTime);
            previousPosition = transform.position;

            yield return null;
        }

        StepRoller(to, previousPosition, Time.deltaTime);
    }

    private bool SampleGround(Vector3 horizontalTarget, out float groundY)
    {
        Vector3 rayOrigin = new Vector3(horizontalTarget.x, transform.position.y + raycastHeight, horizontalTarget.z);
        float rayDistance = raycastHeight * 3f;

        RaycastHit[] hits = Physics.RaycastAll(
            rayOrigin,
            Vector3.down,
            rayDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        bool foundGround = false;
        groundY = transform.position.y - radius;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform == transform || hits[i].transform.IsChildOf(transform))
                continue;

            if (hits[i].distance < closestDistance)
            {
                closestDistance = hits[i].distance;
                groundY = hits[i].point.y;
                foundGround = true;
            }
        }

        return foundGround;
    }

    private void StepRoller(Vector3 pathTarget, Vector3 previousPosition, float dt)
    {
        bool hitFound = SampleGround(pathTarget, out float rawGroundY);

        if (!hitFound && stopAtMapEdge)
        {
            // Stay at current ground height and don't walk off the edge
            pathTarget = new Vector3(previousPosition.x, previousPosition.y, previousPosition.z);
            rawGroundY = smoothedGroundPoint.y;
        }
        else if (!hitFound)
        {
            Debug.LogWarning("Ground not detected. Check Ground Layer.", this);
            rawGroundY = smoothedGroundPoint.y;
        }

        float blend = 1f - Mathf.Exp(-Mathf.Max(groundSnapSpeed, 1f) * Mathf.Max(dt, 0.0001f));
        Vector3 desiredGroundPoint = new Vector3(pathTarget.x, rawGroundY, pathTarget.z);

        if (!initializedGround && hitFound)
        {
            smoothedGroundPoint = desiredGroundPoint;
            initializedGround = true;
        }
        else
        {
            smoothedGroundPoint = Vector3.Lerp(smoothedGroundPoint, desiredGroundPoint, blend);
        }

        Vector3 finalPosition = new Vector3(
            smoothedGroundPoint.x,
            smoothedGroundPoint.y + radius + heightOffset,
            smoothedGroundPoint.z
        );

        Vector3 displacement = finalPosition - previousPosition;
        float movedDistance = displacement.magnitude;

        float desiredSign = moveTowardPositiveZ ? 1f : -1f;
        float sign = Vector3.Dot(displacement, new Vector3(0f, 0f, desiredSign)) >= 0f ? 1f : -1f;

        transform.position = finalPosition;

        float angleDeg = (movedDistance / Mathf.Max(radius, 0.01f)) * Mathf.Rad2Deg * sign;
        if (invertSpinDirection)
            angleDeg = -angleDeg;

        Vector3 spinAxis = localAxle.sqrMagnitude > 0.0001f ? localAxle.normalized : Vector3.right;
        transform.Rotate(spinAxis, angleDeg, Space.Self);
    }
}
