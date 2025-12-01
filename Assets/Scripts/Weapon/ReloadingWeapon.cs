using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

public class ReloadingWeapon : MonoBehaviour
{
    public GameObject AmmoCylinder;
    public BoxCollider triggerCollider;

    private GameObject m_main_ogj;
    private bool m_is_busy = false;
    

    private void Start()
    {
        m_main_ogj = GameObject.Find("Cylinder_Reloadeble");
        CheckForObjectsInsideTrigger();

        InvokeRepeating(nameof(CheckForObjectsInsideTrigger), 3f, 3f);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Collide with " + other.name + " tag is " + other.tag);
        if (other.tag == "Ammo" && !m_is_busy)
        {
            other.transform.SetParent(m_main_ogj.transform);
            other.transform.localPosition = new Vector3(0.0338993f, 0.04899454f, 0.01215172f);
            other.transform.localRotation = Quaternion.Euler(0, 180, 0);
            other.transform.localScale = new Vector3(0.8216703f, 0.8216701f, 0.8216703f);
            other.transform.SetParent(AmmoCylinder.transform);
            other.GetComponent<XRGrabInteractable>().enabled = false;
            other.GetComponent<Rigidbody>().isKinematic = true;
            other.GetComponent<Rigidbody>().useGravity = false;

            m_is_busy = true;
        }
    }

    private void CheckForObjectsInsideTrigger()
    {
        Vector3 center = triggerCollider.transform.TransformPoint(triggerCollider.center);
        Vector3 halfExtents = triggerCollider.size * 0.5f;
        Vector3 scale = triggerCollider.transform.lossyScale;
        halfExtents = Vector3.Scale(halfExtents, scale);
        
        Quaternion orientation = triggerCollider.transform.rotation;
        
        Collider[] collidersInside = Physics.OverlapBox(center, halfExtents, orientation);
        
        foreach (Collider collider in collidersInside)
        {
            if (collider.CompareTag("Ammo"))
            {
                m_is_busy = true;
                return;
            }
        }
        m_is_busy = false;
    }

    void OnDestroy()
    {
        CancelInvoke(nameof(CheckForObjectsInsideTrigger));
    }
}
