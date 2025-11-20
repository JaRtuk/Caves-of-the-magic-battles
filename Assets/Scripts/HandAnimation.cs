using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class HandAnimation : MonoBehaviour
{
    [SerializeField]
    InputActionProperty m_TriggerAction;
    
    [SerializeField]
    InputActionProperty m_GripAction;
    
    [SerializeField] 
    Animator animator;

    private void OnEnable()
    {
        m_TriggerAction.action.Enable();
        m_GripAction.action.Enable();
    }

    private void OnDisable()
    {
        m_TriggerAction.action.Disable();
        m_GripAction.action.Disable();
    }

    private void Update()
    {
        float triggerValue = m_TriggerAction.action?.ReadValue<float>() ?? 0f;
        float gripValue = m_GripAction.action?.ReadValue<float>() ?? 0f;

        animator.SetFloat("Trigger", triggerValue);
        animator.SetFloat("Grip", gripValue);
    }
}