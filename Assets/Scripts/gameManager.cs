using UnityEngine;
using UnityEngine.SceneManagement;



public class gameManager : MonoBehaviour
{
    public float delay = 2f;
    bool gameHasEnded = false;
        public void EndGame()
    {
        if (gameHasEnded == false) {

            Debug.Log("Game Over!");
            gameHasEnded = true;
            Invoke("Restart", delay);
        }

    }

    public GameObject gameLevelObjet;
    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    
    }

    public void CompleteLevel()
    {
        gameLevelObjet.SetActive(true);
    }
}
