using UnityEngine;

public class Flyerprompt : MonoBehaviour
{


    public GameObject flyerPaperUI;

    public GameObject pressFPrompt;

    public playerController playerScript;

    private bool canRead = false;

    private bool isReading = false;

    // Update is called once per frame
    void Update()
    {
        if (canRead && Input.GetKeyDown(KeyCode.F))
        {
            if (!isReading)
            {
                SeeRent();
            }
            else
            {
                CloseRent();
            }
        }
    }

    void SeeRent()
    {
        flyerPaperUI.SetActive(true);

        pressFPrompt.SetActive(false);

        playerScript.enabled = false;

        isReading = true;
    }

    public void CloseRent()
    {
        flyerPaperUI.SetActive(false);

        playerScript.enabled = true;

        isReading = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canRead = true;

            pressFPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canRead = false;

            pressFPrompt.SetActive(false);

            CloseRent();
        }
    }
}
