using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] float lifetime = 3f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }
    private void OnTriggerEnter(Collider other)
    {
        // ignore player
        if (other.CompareTag("Player")) return;

        IDamage dmg = other.GetComponentInParent<IDamage>();
        if (dmg != null)
        {
            dmg.takeDamage(10); // or pull from stats later
        }

        Destroy(gameObject);
    }
}
