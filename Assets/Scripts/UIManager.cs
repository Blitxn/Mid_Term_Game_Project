using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject timeUpPanel;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    [Header("Timer")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float gameDuration = 180f;

    private float remainingTime;
    private bool isPlaying = false;
    private bool gameEnded = false;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 0f;
        remainingTime = gameDuration;
    }

    private void Start()
    {
        mainMenuPanel.SetActive(true);
        gamePanel.SetActive(false);
        winPanel.SetActive(false);
        timeUpPanel.SetActive(false);

        startButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);

        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (!isPlaying || gameEnded)
            return;

        remainingTime -= Time.unscaledDeltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            UpdateTimerDisplay();
            TimeUp();
            return;
        }

        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void StartGame()
    {
        mainMenuPanel.SetActive(false);
        gamePanel.SetActive(true);

        isPlaying = true;
        Time.timeScale = 1f;
    }

    public void ShowWinPanel()
    {
        if (gameEnded)
            return;

        gameEnded = true;
        isPlaying = false;

        winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void TimeUp()
    {
        gameEnded = true;
        isPlaying = false;

        timeUpPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
