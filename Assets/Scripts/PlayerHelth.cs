using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHalth : MonoBehaviour
{
    public GameObject Player;

    private float m_player_helth;

    void Start()
    {
        m_player_helth = 100f;
        InvokeRepeating(nameof(Heal), 1f, 1f);
    }

    void Update()
    {
        Debug.Log(m_player_helth);
        if (m_player_helth <= 0)
            SceneManager.LoadScene(1);
    }

    // void OnTriggerEnter(Collider other)
    // {
    //     if (other.tag == "EnemyAmmo")
    //     {
    //         m_player_helth -= 15;
    //         Destroy(other.gameObject);
    //     }
    // }

    public void TakeDamage(float damage)
    {
        m_player_helth -= damage;        
    }

    public void Heal(float r_heal_value)
    {
        if (m_player_helth > 0 && m_player_helth < 100)
            m_player_helth += r_heal_value;
    }

    private void Heal()
    {
        if (m_player_helth > 0 && m_player_helth < 100)
            m_player_helth += 1;
    }
}
