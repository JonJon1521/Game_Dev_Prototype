using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] GameObject model;
    [SerializeField] GameObject button;

    bool canOpen;

    void Update()
    {
        if (Input.GetButtonDown("Interact") && canOpen)
        {
            model.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canOpen = true;
            button.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            model.SetActive(true);
            canOpen = false;
            button.SetActive(false);
        }
    }
}

