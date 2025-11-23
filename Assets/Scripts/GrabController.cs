using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabController : MonoBehaviour
{
    private bool onGrab;
    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    
    [Header("Grab Settings")]
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 rotationOffset = Vector3.zero;
    public bool useCustomAttachPoint = false;
    public Transform customAttachPoint;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnUngrab);
    }

    public void OnGrab(SelectEnterEventArgs args)
    {
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        Transform hand = args.interactorObject.transform;
        Transform objectToGrab = args.interactableObject.transform;
        
        // Делаем объект дочерним к руке
        objectToGrab.SetParent(hand);
        
        // Сбрасываем позицию и поворот с учетом смещений
        if (useCustomAttachPoint && customAttachPoint != null)
        {
            // Используем кастомную точку захвата
            objectToGrab.position = hand.position;
            objectToGrab.localRotation = hand.rotation;
            
            // Компенсируем смещение кастомной точки
            Vector3 offset = objectToGrab.position - customAttachPoint.position;
            objectToGrab.position += offset;
            
            Quaternion rotOffset = Quaternion.Inverse(customAttachPoint.rotation) * objectToGrab.rotation;
            objectToGrab.rotation = hand.rotation * rotOffset;
        }
        else
        {
            // Используем стандартные смещения
            objectToGrab.localPosition = positionOffset;
            objectToGrab.localRotation = Quaternion.Euler(rotationOffset);
        }
        
        onGrab = true; 
    }

    public void OnUngrab(SelectExitEventArgs args)
    {
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        
        args.interactableObject.transform.SetParent(null);
        onGrab = false;
    }

    public bool IsGrab()
    {
        return onGrab;
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited.RemoveListener(OnUngrab);
        }
    }
}