using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class ShootingWeapon : MonoBehaviour
{
    public GameObject Weapon;
    public bool Shot;

    private bool m_in_payer_arm;
    private Transform m_cylinder_LP;
    private ParticleSystem m_fire_effect;
    private float m_angle = -60;
    private bool m_play_fire_anim = true;

    void Start()
    {
        if (Weapon != null)
        {
            m_cylinder_LP = Weapon.transform.Find("Cylinder_Poivot");
            Transform fire_transform = Weapon.transform.Find("Fire_Hit");
            m_fire_effect = fire_transform.GetComponent<ParticleSystem>();
        }



    }
    void Update()
    {

        if (Shot)
        {
            if (m_play_fire_anim)
            {
                m_fire_effect.Play();
                m_play_fire_anim = false;
            }
            Rotating();
        }
    }

    void Rotating()
    {
        if (m_angle < -2f)
        {
            if (m_angle * Time.deltaTime * 7 < m_angle)
                return;
            m_cylinder_LP.Rotate(m_angle * Time.deltaTime * 7, 0, 0, Space.Self);
            m_angle -= m_angle * Time.deltaTime * 7;
        }
        else
        {
            m_cylinder_LP.Rotate(m_angle, 0, 0, Space.Self);
            Shot = false;
            m_angle = -60;
            m_play_fire_anim = true;
        }
    }
}
//10.89173  10.923