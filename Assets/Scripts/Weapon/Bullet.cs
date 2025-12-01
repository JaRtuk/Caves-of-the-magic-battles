using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Transform bullet; 
    private bool startShoot = false;
    private bool hasDealtDamage = false;
    private bool canDestroy = false;
    private Vector3 m_shoot_dir;

    public void StartShoot(Vector3 shootDir)
    {
        m_shoot_dir = shootDir;
        startShoot = true;
        InvokeRepeating(nameof(CanDestroy), 2f, 10000f);
    }

    private void CanDestroy()
    {
        canDestroy = true;
    }

    void Update()
    {
        if (startShoot)
        {
            bullet.localPosition += m_shoot_dir * Time.deltaTime * 7;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (!startShoot) return;
        if (hasDealtDamage) return;
        hasDealtDamage = true;

        if (other.CompareTag("Enemy"))
        {
            var hp = other.GetComponent<EnemyArcher>();
            if (hp != null) hp.TakeDamage(50);
        }

        if (canDestroy)
            Destroy(bullet.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!startShoot) return;
        if (hasDealtDamage) return;
        hasDealtDamage = true;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            var hp = collision.gameObject.GetComponent<EnemyArcher>();
            if (hp != null) hp.TakeDamage(50);
        }

        if (canDestroy) 
            Destroy(bullet.gameObject);
    }
}
