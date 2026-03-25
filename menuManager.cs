using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class menuManager : MonoBehaviour
{
    public TMP_Text NamaText;

    public void LoadScene(string NamaScene)
    {
        SceneManager.LoadScene(NamaScene);
    }

    public void SetNamaPanel(string NamaPanel)
    {
        NamaText.SetText(NamaPanel);
    }

    public void exit(){
        Application.Quit();
    }
}
