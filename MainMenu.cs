using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
        public void NewGame()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Start New Game");
        SceneManager.LoadScene("Intro");
    }

    public void ContinueGame()
    {
        if (Save.HasSave())
        {
            Debug.Log("Loading saved game from scene: " + Save.GetLastScene());
            SceneManager.LoadScene(Save.GetLastScene());
        }
        else
        {
            Debug.Log("No save found, starting new game");
            NewGame();
        }
    }

    public void Settings()
    {   
        SceneManager.LoadScene("costumize");
        Debug.Log("Open Settings");
        // nanti kita buat
    }

    public void ExitGame()
    {
        Debug.Log("Exit Game");
        Application.Quit();
    }


}