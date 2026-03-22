using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonFunctions : MonoBehaviour
{
   public void resume()
    {
        gameManager.instance.stateUnpaused();
    }

    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        gameManager.instance.stateUnpaused();
    }

    public void quit()
    {
#if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying = false;

#else 

Application.Quit();

#endif
    }
}
