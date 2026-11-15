using UnityEngine;

public class Orb : MonoBehaviour
{
    [Header("Click palette")]
    [SerializeField] private Color[] clickColors = { Color.cyan, Color.magenta, Color.yellow };

    [Header("Bob & spin")]
    [SerializeField] private float bobHeight = 0.15f;
    [SerializeField] private float spinSpeed = 90f;

    private Vector3 basePosition;
    private Vector3 baseScale;
    private int clickIndex;

    void Awake()
    {
        basePosition = transform.position;
        baseScale = transform.localScale;
    }

    void Update()
    {
        // Bob up and down using Time.time (seconds since Play started).
        float bob = Mathf.Sin(Time.time * 3f) * bobHeight;
        transform.position = basePosition + Vector3.up * bob;
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    private void OnMouseDown()
    {
        // Click the orb's collider → cycle its color. Uses GetComponent<T>().
        if (clickColors != null && clickColors.Length > 0)
        {
            clickIndex = (clickIndex + 1) % clickColors.Length;
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = clickColors[clickIndex];
            }
            Debug.Log($"[Orb] Clicked → {clickColors[clickIndex]}");
        }
    }

}
