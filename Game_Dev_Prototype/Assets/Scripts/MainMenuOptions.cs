using UnityEngine;

public class MainMenuOptions : MonoBehaviour
{
    [SerializeField] GameObject optionsMenu;

    public void OpenOptions()
    {
        optionsMenu.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsMenu.SetActive(false);
    }
}