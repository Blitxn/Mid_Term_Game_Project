using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("State")]
    [SerializeField] private int totalOrbs = 5;

    private int collected;

    private void Awake()
    {
        Instance = this;

        GameObject[] orbs = GameObject.FindGameObjectsWithTag("Orb");
        totalOrbs = orbs.Length;

        Debug.Log($"[GameManager] Found {totalOrbs} orbs in scene.");
    }

    private void Start()
    {
        // Display the initial orb count.
        UIManager.Instance.UpdateOrbCounter(collected, totalOrbs);
    }

    public void Collect()
    {
        collected++;

        Debug.Log("STEP 3: Collected " + collected + "/" + totalOrbs);

        if (UIManager.Instance != null)
        {
            Debug.Log("STEP 4: UIManager found!");

            UIManager.Instance.UpdateOrbCounter(collected, totalOrbs);
        }
        else
        {
            Debug.LogError("ERROR: UIManager Instance is NULL!");
        }

        if (totalOrbs > 0 && collected >= totalOrbs)
        {
            UIManager.Instance.ShowWinPanel();
        }
    }
}
