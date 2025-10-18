using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

public class ShootingWeapon : MonoBehaviour
{
    public GameObject Weapon;
    public bool Shot = false;
    public bool IsHammerCharge = false;

    private bool m_in_payer_arm = false;

    private bool m_play_fire_anim = true;
    private ParticleSystem m_fire_effect;

    private Transform m_cylinder_LP;
    private float m_cylinder_rotation_angle = -60;

    private Transform m_hammer_LP;
    private float m_hammer_rotation_angle = 46;
    private bool m_hammer_on_idle = true;

    private bool m_bullet_in_aria = false;
    private bool m_bullet_is_ready = false;

    private Transform Bullet;

    void Start()
    {
        if (Weapon != null)
        {
            m_cylinder_LP = Weapon.transform.Find("Cylinder_Poivot");
            m_hammer_LP = Weapon.transform.Find("HammerPivot");

            Transform fire_transform = Weapon.transform.Find("Fire_Hit");
            m_fire_effect = fire_transform.GetComponent<ParticleSystem>();
        }
    }
    void Update()
    {
        if (IsHammerCharge && m_hammer_on_idle)
        {
            HammerCharge();
        }

        if (Shot && IsHammerCharge)
        {
            if (m_play_fire_anim && m_bullet_in_aria && m_bullet_is_ready)
            {
                m_fire_effect.Play();
                m_play_fire_anim = false;

                BulletShoot();
            }
            HammerHit();
            RotateBaranan();
        }
    }

    void BulletShoot()
    {
        if (Bullet != null)
        {
            Destroy(Bullet.parent.gameObject);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.name == "Main_bullet2_Low")
        {
            m_bullet_in_aria = true;
            Bullet = other.transform;
        }
    }

    private void HammerHit()
    {
        if (m_hammer_LP.rotation.eulerAngles.z > 1 && !m_hammer_on_idle)
        {
            if (m_hammer_LP.rotation.eulerAngles.z - m_hammer_rotation_angle * Time.deltaTime * 11 <= 0)
                m_hammer_LP.Rotate(0, 0, -m_hammer_LP.rotation.eulerAngles.z, Space.Self);
            else
                m_hammer_LP.Rotate(0, 0, -m_hammer_rotation_angle * Time.deltaTime * 11, Space.Self);
        }
        else
        {
            if (m_hammer_LP.rotation.eulerAngles.z != 0)
                m_hammer_LP.localRotation = Quaternion.Euler(0, 0, 0);

            m_hammer_on_idle = true;
            return;
        }
    }

    private void HammerCharge()
    {
        if (m_hammer_LP.rotation.eulerAngles.z < m_hammer_rotation_angle)
        {
            m_hammer_LP.Rotate(0, 0, m_hammer_rotation_angle * Time.deltaTime * 7, Space.Self);
        }
        else
        {
            if (m_hammer_LP.rotation.eulerAngles.z != m_hammer_rotation_angle)
                m_hammer_LP.Rotate(0,0, -(m_hammer_LP.rotation.eulerAngles.z - m_hammer_rotation_angle));

            m_hammer_on_idle = false;
            return;
        }
    }
    
    private void RotateBaranan()
    {
        if (m_cylinder_rotation_angle < -2f)
        {
            if (m_cylinder_rotation_angle * Time.deltaTime * 7 < m_cylinder_rotation_angle)
                return;
            m_cylinder_LP.Rotate(m_cylinder_rotation_angle * Time.deltaTime * 7, 0, 0, Space.Self);
            m_cylinder_rotation_angle -= m_cylinder_rotation_angle * Time.deltaTime * 7;
        }
        else
        {
            m_cylinder_LP.Rotate(m_cylinder_rotation_angle, 0, 0, Space.Self);
            Shot = false;
            IsHammerCharge = false;
            m_cylinder_rotation_angle = -60;
            m_play_fire_anim = true;

            if (m_bullet_is_ready)
            {
                m_bullet_in_aria = false;
                m_bullet_is_ready = false;
            }
            else if (m_bullet_in_aria)
                m_bullet_is_ready = true;

        }
    }
}