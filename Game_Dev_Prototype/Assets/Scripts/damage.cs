using System.Collections;
//using UnityEditorInternal;
using UnityEngine;

public class damage : MonoBehaviour
{

    enum damagetype { bullet, DOT, stationary }
    [SerializeField] damagetype type;
    [SerializeField] Rigidbody rigid;

    [SerializeField] int damageAmount;
    [SerializeField] float rateOfDamage;
    [SerializeField] int speed;
    [SerializeField] float destroyTime;
    [SerializeField] ParticleSystem hit;

    bool isDamaging;

    void Start()
    {
        if (type == damagetype.bullet)
        {
            rigid.linearVelocity = transform.forward * speed;
            Destroy(gameObject, destroyTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg != null && type != damagetype.DOT)
        {
            dmg.takeDamage(damageAmount);
        }

        if (type == damagetype.bullet)
        {
            if (hit != null)
            {
                Instantiate(hit, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }

        if(dmg != null && type != damagetype.stationary)
        {
            if (other.CompareTag("Player"))
            {
                dmg.takeDamage(damageAmount);
            }
        }

    }


    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg != null && type == damagetype.DOT && !isDamaging)
        {
            StartCoroutine(damageOther(dmg));
        }
    }

    IEnumerator damageOther(IDamage D)
    {
        isDamaging = true;
        D.takeDamage(damageAmount);
        yield return new WaitForSeconds(rateOfDamage);
        isDamaging = false;
    }
}

