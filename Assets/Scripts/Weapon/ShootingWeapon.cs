using Bhaptics.SDK2;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShootingWeapon : MonoBehaviour
{
    [Header("Weapon References")]
    public GameObject           Weapon;
    public GrabController       grabController;
    public ReloadingWeapon      reloadingWeapon;
    [Header("Input Actions")]
    public InputActionReference shootAction;
    public InputActionReference chargeAction;


    private Transform      m_cylinder_LP;
    private Transform      m_hammer_LP;
    private Transform      Bullet;
    private ParticleSystem m_fire_effect;
    private BoxCollider    m_shootAria;

    private bool m_hammer_on_idle   = true;
    private bool m_play_fire_anim   = true;
    private bool m_shot             = false;
    private bool m_is_hammer_charge = false;
    private bool m_in_payer_arm     = false;
    private bool m_bullet_in_aria   = false;
    private bool m_bullet_is_ready  = false;

    private float m_cylinder_rotation_angle = -60;
    private float m_hammer_rotation_angle   = 46;

    private void Awake()
    {
        shootAction.action.Enable();
        shootAction.action.performed += ToggleShoot;
        InputSystem.onDeviceChange += OnDeviceChange;

        chargeAction.action.Enable();
        chargeAction.action.performed += ToggleCharge;
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void Start()
    {
        if (Weapon != null)
        {
            m_cylinder_LP = Weapon.transform.Find("Cylinder_Reloadeble").Find("Cylinder_Pivot");
            m_hammer_LP = Weapon.transform.Find("HammerPivot");
            m_shootAria = Weapon.transform.Find("Area_for_bullet").GetComponent<BoxCollider>();
            CheckForObjectsInsideTrigger();

            Transform fire_transform = Weapon.transform.Find("Fire_Hit");
            m_fire_effect = fire_transform.GetComponent<ParticleSystem>();
        }
    }

    private void Update()
    {
        m_in_payer_arm = grabController.IsGrab();

        if (m_is_hammer_charge && m_hammer_on_idle)
        {
            HammerCharge();
        }

        if (m_shot && m_is_hammer_charge)
        {
            if (m_play_fire_anim && m_bullet_in_aria && m_bullet_is_ready)
            {
                m_fire_effect.Play();
                m_play_fire_anim = false;

                BulletShoot();

                m_bullet_in_aria = false;
                m_bullet_is_ready = false;
            }
            HammerHit();
            RotateBaraban();
        }
    }

    // private void ToggleShoot(InputAction.CallbackContext context)
    // {
    //     if (m_in_payer_arm && m_is_hammer_charge)
    //         m_shot = true;
    //     else if (!m_is_hammer_charge && m_in_payer_arm)
    //         RotateBaraban();
    // }

    private void ToggleShoot(InputAction.CallbackContext context)
    {
        if (m_in_payer_arm && m_is_hammer_charge)
        {
            m_shot = true;

            BhapticsLibrary.Play("pistol_recoil");
        }
        else if (!m_is_hammer_charge && m_in_payer_arm)
        {
            //RotateBaraban();
        }
    }

    private void ToggleCharge(InputAction.CallbackContext context)
    { 
        if (m_in_payer_arm)
            m_is_hammer_charge = true;
    }
    
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Disconnected:
                shootAction.action.Disable();
                shootAction.action.performed -= ToggleShoot;
                chargeAction.action.Disable();
                chargeAction.action.performed -= ToggleCharge;
                break;
            case InputDeviceChange.Reconnected:
                shootAction.action.Enable();
                shootAction.action.performed += ToggleShoot;
                chargeAction.action.Enable();
                chargeAction.action.performed += ToggleCharge;
                break;
        }
    }

    private void BulletShoot()
    {
        if (Bullet != null)
        {
            Vector3 localDir = new Vector3(1, 0, 0);
            Vector3 worldDir = transform.TransformDirection(localDir);
            Bullet.SetParent(null);
            Bullet.GetComponent<Bullet>().StartShoot(worldDir);
            Bullet = null;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ammo" && other.GetComponent<Bullet>() != null)
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
    
    private void RotateBaraban()
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
            m_shot = false;
            m_is_hammer_charge = false;


            m_cylinder_rotation_angle = -60;
            m_play_fire_anim = true;

            if (m_bullet_in_aria)
                m_bullet_is_ready = true;
        }
    }

    private void OnDestroy()
    {
        shootAction.action.Disable();
        shootAction.action.performed -= ToggleShoot;
        // InputSystem.onDeviceChange -= OnDeviceChange;
        chargeAction.action.Disable();
        chargeAction.action.performed -= ToggleShoot;
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void CheckForObjectsInsideTrigger()
    {
        Vector3 center = m_shootAria.transform.TransformPoint(m_shootAria.center);
        Vector3 halfExtents = m_shootAria.size * 0.5f;
        Vector3 scale = m_shootAria.transform.lossyScale;
        halfExtents = Vector3.Scale(halfExtents, scale);
        
        Quaternion orientation = m_shootAria.transform.rotation;
        
        Collider[] collidersInside = Physics.OverlapBox(center, halfExtents, orientation);
        
        foreach (Collider collider in collidersInside)
        {
            if (collider.CompareTag("Ammo"))
            {
                m_bullet_in_aria  = true;
                m_bullet_is_ready = true;
            }
        }
    }
}