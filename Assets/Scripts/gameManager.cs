using UnityEngine;
using UnityEngine.SceneManagement;



public class gameManager : MonoBehaviour
{
    public float delay = 2f;
    public bool gameHasEnded = false;

    public void EndGame()
    {
        if (gameHasEnded == false) {

            Debug.Log("Game Over!");
            gameHasEnded = true;
            Invoke("goToMain", delay);
        }

    }

    public GameObject gameLevelObjet;
    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }

    private void goToMain()
    {
        SceneManager.LoadScene("Menu");
    }
    public void CompleteLevel()
    {
        gameLevelObjet.SetActive(true);
    }
   


}
