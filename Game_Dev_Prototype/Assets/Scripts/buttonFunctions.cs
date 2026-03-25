using UnityEngine;
using UnityEngine.SceneManagement;



public class ButtonFunctions : MonoBehaviour
{
    public void resume()
    {
        gamemanager.instance.stateUnpaused();

    }

    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gamemanager.instance.stateUnpaused();
    }

    public void quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();

#endif
    }
    public void PlayerSpawn()
    {
        gamemanager.instance.playerScript.spawnPlayer();
        gamemanager.instance.stateUnpaused();
    }
}

