// --- START OF FILE EndlessGameController.cs ---
using UnityEngine;
using UnityEngine.SceneManagement; // For scene reloading if you choose that for restart

public class EndlessGameController : MonoBehaviour
{
    public static EndlessGameController Instance; // Singleton

    public PlayerSidewaysMovement playerMovement;
    public MovingPlatformManager platformManager;
    public EndlessScoreManager scoreManager;

    public GameObject gameOverUI; // Assign a UI panel for Game Over screen
    public float restartDelay = 2f;

    public bool isGameOver = false;
    private Vector3 playerInitialSpawnPosition;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Try to find components if not assigned
        if (playerMovement == null) playerMovement = FindObjectOfType<PlayerSidewaysMovement>();
        if (platformManager == null) platformManager = FindObjectOfType<MovingPlatformManager>();
        if (scoreManager == null) scoreManager = FindObjectOfType<EndlessScoreManager>();

        if (playerMovement == null || platformManager == null || scoreManager == null)
        {
            Debug.LogError("One or more essential managers not found by EndlessGameController!");
            this.enabled = false;
            return;
        }

        if (gameOverUI != null) gameOverUI.SetActive(false);
        playerInitialSpawnPosition = playerMovement.transform.position; // Store initial player pos
        StartGame();
    }

    public void StartGame()
    {
        isGameOver = false;
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
            playerMovement.ResetPlayer(playerInitialSpawnPosition);
        }
        if (platformManager != null)
        {
            platformManager.gameObject.SetActive(true); // Ensure it's active
            platformManager.ResetWorld(playerInitialSpawnPosition);
        }
        if (scoreManager != null)
        {
            scoreManager.ResetScore();
            scoreManager.StartScoring();
        }
        if (gameOverUI != null) gameOverUI.SetActive(false);

        Time.timeScale = 1f; // Ensure game is not paused
        Debug.Log("Endless Game Started!");
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("Endless Game Over! Final Score: " + (scoreManager ? scoreManager.GetFinalScore().ToString() : "N/A"));

        if (playerMovement != null) playerMovement.enabled = false; // Disable player input
        if (scoreManager != null) scoreManager.StopScoring();
        // Optionally slow down world speed or stop platforms
        // if (platformManager != null) platformManager.worldSpeed = 0; // Or platformManager.enabled = false;

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            // You can update a Text field in gameOverUI with the final score here
        }

        Invoke("PrepareRestart", restartDelay);
    }

    void PrepareRestart()
    {
        // Here you decide how to restart. Reloading the scene is simplest.
        // For a UI button to restart, it would call StartGame() or a scene load method.
        Debug.Log("Preparing to restart or go to menu...");
        // Example: SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // Or, if you have a restart button on gameOverUI, it would call StartGame().
    }

    // Call this from a UI Button on your Game Over screen
    public void RestartButton()
    {
        StartGame();
    }

    public void MainMenuButton()
    {
        // Assuming your main menu is scene 0 or named "MainMenu"
        // SceneManager.LoadScene("MainMenu");
        SceneManager.LoadScene(0); // Adjust scene index/name as needed
    }
}
// --- END OF FILE EndlessGameController.cs ---