using UnityEngine;
using UnityEngine.SceneManagement;

public class Credits : MonoBehaviour
{
    public void Quit()
    {
        Debug.Log("Quitting");
        Application.Quit();
    }

    public void EndlessGame()
    {
        SceneManager.LoadScene("Endless");
    }
    public void GoToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}

