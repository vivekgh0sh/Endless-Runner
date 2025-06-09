// --- START OF FILE EndlessScoreManager.cs ---
using UnityEngine;
using TMPro; // Or UnityEngine.UI if using legacy Text

public class EndlessScoreManager : MonoBehaviour
{
    public static EndlessScoreManager Instance; // Singleton for easy access

    public TextMeshProUGUI scoreText; // Assign your UI Text element
    public MovingPlatformManager platformManager; // Assign your MovingPlatformManager
    public float scoreMultiplier = 1f; // Adjust to change how fast score increases

    private float currentScore = 0f;
    private bool isScoring = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (scoreText == null)
        {
            Debug.LogError("Score Text not assigned in EndlessScoreManager!");
            this.enabled = false;
            return;
        }
        if (platformManager == null)
        {
            // Try to find it if not assigned
            platformManager = FindObjectOfType<MovingPlatformManager>();
            if (platformManager == null)
            {
                Debug.LogError("MovingPlatformManager not found by EndlessScoreManager!");
                this.enabled = false;
                return;
            }
        }
        ResetScore();
        StartScoring(); // Start scoring when the game begins
    }

    void Update()
    {
        if (isScoring && platformManager != null && platformManager.worldSpeed > 0)
        {
            currentScore += platformManager.worldSpeed * scoreMultiplier * Time.deltaTime;
            scoreText.text = "Score: " + Mathf.FloorToInt(currentScore).ToString();
        }
    }

    public void AddScore(float amount)
    {
        if (isScoring)
        {
            currentScore += amount;
        }
    }

    public void StartScoring()
    {
        isScoring = true;
    }

    public void StopScoring()
    {
        isScoring = false;
    }

    public void ResetScore()
    {
        currentScore = 0f;
        if (scoreText != null) scoreText.text = "Score: 0";
    }

    public int GetFinalScore()
    {
        return Mathf.FloorToInt(currentScore);
    }
}
// --- END OF FILE EndlessScoreManager.cs ---