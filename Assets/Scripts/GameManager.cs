using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("State")]
    [SerializeField] private int totalOrbs = 5;

    private int collected;

    void Awake()
    {
        Instance = this;

        GameObject[] orbs = GameObject.FindGameObjectsWithTag("Orb");
        totalOrbs = orbs.Length;
        Debug.Log($"[GameManager] Found {totalOrbs} orbs in scene.");
    }

    void Update()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit(); // handy quit for testing the demo scene
#endif
    }

    public void Collect()
    {
        collected++;
        Debug.Log($"[GameManager] Collected {collected}/{totalOrbs}");
    }

    void OnGUI()
    {
        // Immediate-mode UI: runs several times per frame, so keep it cheap.
        GUI.Label(new Rect(12, 10, 400, 30), $"Orbs collected: {collected} / {totalOrbs}", new GUIStyle(GUI.skin.label) { fontSize = 20 });

        if (totalOrbs > 0 && collected >= totalOrbs)
        {
            GUIStyle winStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 40,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            winStyle.normal.textColor = Color.yellow;

            float bannerWidth = 400f;
            float bannerHeight = 100f;
            float xPos = (Screen.width - bannerWidth) * 0.5f;
            float yPos = (Screen.height - bannerHeight) * 0.5f;

            GUI.Label(new Rect(xPos, yPos, bannerWidth, bannerHeight), "YOU WIN!", winStyle);
        }
    }
}
