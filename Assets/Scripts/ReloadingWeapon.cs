using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

public class ReloadingWeapon : MonoBehaviour
{
    public GameObject AmmoCylinder;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ammo")
        {
            other.transform.SetParent(AmmoCylinder.transform);
            other.transform.localPosition = new Vector3(-0.09919406f, -0.00622413f, 0.004062738f);
            other.transform.localRotation = Quaternion.Euler(-90, 180, 0);
            other.transform.localScale = new Vector3(0.410835f,0.410835f,0.410835f);
            other.GetComponent<XRGrabInteractable>().enabled = false;
            other.GetComponent<Rigidbody>().isKinematic = true;
            other.GetComponent<Rigidbody>().useGravity = false;
        }
    }
}
