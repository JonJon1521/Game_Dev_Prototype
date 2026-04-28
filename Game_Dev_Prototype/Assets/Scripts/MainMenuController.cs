using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
   

    public void PlayGame()
    {
        SceneManager.LoadScene(1); // will load the game after hitting play
    }

    public void CloseGame()
    {
        Application.Quit();

        Debug.Log("Game Exiting");
    }
}
