using System.Collections;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    [SerializeField] int sens;

    [SerializeField] int lockVertMin, lockVertMax;

    [SerializeField] bool invertY;

    [SerializeField] Transform player;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    float camRotX;

    Vector3 origLocalPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        player = transform.parent;

        origLocalPos = transform.localPosition; //stores the 'center' position of the camera

        Cursor.visible = false;

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sens * Time.deltaTime;

        float mouseY = Input.GetAxisRaw("Mouse Y") * sens * Time.deltaTime;

        if(invertY)
        {
            camRotX += mouseY;
        }
        else
        {
            camRotX -= mouseY;
        }

        camRotX = Mathf.Clamp(camRotX, lockVertMin, lockVertMax);

        transform.localRotation = Quaternion.Euler(camRotX, 0, 0);

        player.transform.Rotate(Vector3.up * mouseX);
    }

    public void shack(float dur, float mag) // dur = duration, mag = magnitud
    {
        StartCoroutine(ProcessShack(dur, mag)); // start the Coroutine
    }

    private IEnumerator ProcessShack(float dur,float mag)
    {
        float elapsed = 0.0f; // track how much time has passed since the shack started

        // keep shacking till the elapsed time meets duration time

        while(elapsed < dur)
        {
            float x = Random.Range(-1f, -1f) * mag; // pick a random x offset for the magnitud ( the intensaty of the hit)

            float y = Random.Range(-1f, -1f) * mag; // paick a random y offet for magnitud

            transform.localPosition = new Vector3(origLocalPos.x + x, origLocalPos.y + y, origLocalPos.z); // move the camera to a new random spot keeping the z distance

            elapsed += Time.deltaTime; // add the time that this tookto complet

            yield return null; // wait for the very next frame before running the loop agian
        }

        transform.localPosition = origLocalPos; // when timer is done set back to original position;

    }
}
