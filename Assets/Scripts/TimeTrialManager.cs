/* Ethan Gapic-Kott, 000923124 */

using UnityEngine;
using TMPro;

public class TimeTrialManager : MonoBehaviour
{
    [Header("Timer Settings")]
    public float startTime = 30f;
    private float timer;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TextMeshProUGUI endScoreText;
    [SerializeField] private TextMeshProUGUI endLinesText;

    [Header("Score Settings")]
    private int score = 0;
    private int linesCleared = 0;
    public int forkBonusTime = 3; // 3 Seconds added when Fork placed
    public int forkScore = 100;   // Score per Fork placed
    public int lineClearScore = 1000; // Points per line cleared

    private bool isGameOver = false;

    private void Start()
    {
        timer = startTime;
        UpdateTimerUI();
        UpdateScoreUI();
        if (endPanel != null)
            endPanel.SetActive(false);
    }

    private void Update()
    {
        if (isGameOver) return;

        timer -= Time.deltaTime;
        UpdateTimerUI();

        if (timer <= 0f)
        {
            timer = 0f;
            GameOver();
        }
    }

    // Called whenever a Fork piece locks in place
    public void OnForkPlaced()
    {
        if (isGameOver) return;

        timer += forkBonusTime; // Add bonus time
        score += forkScore;     // Add score
        UpdateTimerUI();
        UpdateScoreUI();
    }

    // Called by Board when lines are cleared
    public void OnLinesCleared(int lines)
    {
        if (isGameOver) return;

        linesCleared += lines;
        score += lines * lineClearScore;
        UpdateScoreUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = $"Time: {Mathf.CeilToInt(timer)}";
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    private void GameOver()
    {
        isGameOver = true;

        if (endPanel != null)
        {
            endPanel.SetActive(true);
            if (endScoreText != null)
                endScoreText.text = $"Score: {score}";
            if (endLinesText != null)
                endLinesText.text = $"Lines Cleared: {linesCleared}";
        }
    }
}
