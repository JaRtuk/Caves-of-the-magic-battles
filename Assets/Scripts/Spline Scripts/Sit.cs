using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public class VRBoardingSystem : MonoBehaviour
{
    [Header("XR References")]
    public GameObject xrOrigin; 
    
    [Header("Telega References")]
    public SplineFollower telega; 
    public Transform seatPoint;
    
    public GameObject boardButton;
    public GameObject startDoor;

    private CharacterController xrCharacterController;
    private bool isBoarded = false;
    private bool isOpenDoor = false;

    void Start()
    {
        // Находим компоненты если не установлены
        if (xrOrigin == null)
            xrOrigin = GameObject.Find("XR Origin");
            
        if (xrOrigin != null)
            xrCharacterController = xrOrigin.GetComponent<CharacterController>();

        // Настраиваем кнопку
        // if (boardButton != null)
        //     boardButton.onClick.AddListener(ToggleBoarding);
    }

    void Update()
    {
        if (isBoarded && telega != null && seatPoint != null)
        {
            xrOrigin.transform.position = seatPoint.position;

            if (!isOpenDoor)
            {
                startDoor.transform.position += new Vector3(0, 0.35f, 0) * Time.deltaTime; 
                if (startDoor.transform.position.y > 7.3f)
                {
                    isOpenDoor = true;         
                    telega.StartMoving();          
                }
            }
        }

    }

    public void ToggleBoarding()
    {
        // if (isBoarded)
        // {
        //     ExitTelega();
        // }
        // else
        {
            BoardTelega();
        }
        // telega.StartMoving();
    }

    public void BoardTelega()
    {
        if (telega == null || seatPoint == null || xrOrigin == null)
        {
            Debug.LogError("Не все ссылки установлены в VRBoardingSystem");
            return;
        }


        if (xrCharacterController != null)
            xrCharacterController.enabled = false;


        xrOrigin.transform.position = seatPoint.position;
        xrOrigin.transform.rotation = seatPoint.rotation;


        xrOrigin.transform.SetParent(telega.transform, true);

        Destroy(boardButton);

        isBoarded = true;
        
        Debug.Log("Игрок сел в тележку");
    }

    // public void ExitTelega()
    // {
    //     if (xrOrigin == null) return;

    //     // Возвращаем игрока в независимую иерархию
    //     xrOrigin.transform.SetParent(null, true);

    //     // Включаем CharacterController
    //     if (xrCharacterController != null)
    //         xrCharacterController.enabled = true;

    //     isBoarded = false;
        
    //     Debug.Log("Игрок вышел из тележки");
    // }

    // // Метод для принудительного выхода (например, при завершении поездки)
    // public void ForceExit()
    // {
    //     ExitTelega();
    // }

    public bool IsBoarded()
    {
        return isBoarded;
    }
}