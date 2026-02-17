// using UnityEngine;
// using UnityEngine.XR.Interaction.Toolkit;
// using UnityEngine.UI;

// public class VRBoardingSystem : MonoBehaviour
// {
//     [Header("XR References")]
//     public GameObject xrOrigin; 
//     public GameObject CameraOfset;
    
//     [Header("Telega References")]
//     public SplineFollower telega; 
//     public Transform seatPoint;
    
//     public GameObject boardButton;
//     public GameObject startDoor;

//     private CharacterController xrCharacterController;
//     private bool isBoarded = false;
//     private bool isOpenDoor = false;

//     void Start()
//     {
//         // Находим компоненты если не установлены
//         if (xrOrigin == null)
//             xrOrigin = GameObject.Find("XR Origin");
            
//         if (xrOrigin != null)
//             xrCharacterController = xrOrigin.GetComponent<CharacterController>();

//         // Настраиваем кнопку
//         // if (boardButton != null)
//         //     boardButton.onClick.AddListener(ToggleBoarding);
//     }

//     void Update()
//     {
//         if (isBoarded && telega != null && seatPoint != null)
//         {
//             xrOrigin.transform.position = seatPoint.position;

//             if (!isOpenDoor)
//             {
//                 startDoor.transform.position += new Vector3(0, 0.35f, 0) * Time.deltaTime; 
//                 if (startDoor.transform.position.y > 7.3f)
//                 {
//                     isOpenDoor = true;         
//                     telega.StartMoving();          
//                 }
//             }
//         }

//     }

//     public void ToggleBoarding()
//     {
//         // if (isBoarded)
//         // {
//         //     ExitTelega();
//         // }
//         // else
//         {
//             BoardTelega();
//         }
//         // telega.StartMoving();
//     }


//     public void BoardTelega()
//     {
//         if (telega == null || seatPoint == null || xrOrigin == null)
//         {
//             Debug.LogError("Не все ссылки установлены в VRBoardingSystem");
//             return;
//         }


//         if (xrCharacterController != null)
//             xrCharacterController.enabled = false;

//         xrOrigin.transform.position = CameraOfset.transform.position;

//         xrOrigin.transform.position = seatPoint.position;
//         xrOrigin.transform.rotation = seatPoint.rotation;

//         xrOrigin.transform.SetParent(telega.transform, true);

//         Destroy(boardButton);

//         isBoarded = true;
        
//         Debug.Log("Игрок сел в тележку");
//     }

//     public bool IsBoarded()
//     {
//         return isBoarded;
//     }
// }



using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public class VRBoardingSystem : MonoBehaviour
{
    [Header("XR References")]
    public GameObject xrOrigin; 
    public GameObject CameraOfset;
    
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
        if (xrOrigin == null)
            xrOrigin = GameObject.Find("XR Origin");
            
        if (xrOrigin != null)
            xrCharacterController = xrOrigin.GetComponent<CharacterController>();
    }

    void LateUpdate() // Используем LateUpdate для коррекции после обновления трекинга
    {
        if (isBoarded && telega != null && seatPoint != null)
        {
            // Находим камеру в иерархии XR Origin
            Transform camera = xrOrigin.GetComponentInChildren<Camera>().transform;
            
            // Вычисляем текущее смещение камеры относительно XR Origin
            Vector3 cameraOffsetFromOrigin = camera.position - xrOrigin.transform.position;
            
            // Устанавливаем позицию XR Origin так, чтобы камера оказалась в точке seatPoint
            xrOrigin.transform.position = seatPoint.position - cameraOffsetFromOrigin;

            // Логика открытия двери и запуска движения (остаётся без изменений)
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
            Debug.LogError("Не все ссылки установлены");
            return;
        }

        if (xrCharacterController != null)
            xrCharacterController.enabled = false;

        // ВАЖНО: считаем разницу между камерой и XR Origin
        Transform camera = xrOrigin.GetComponentInChildren<Camera>().transform;
        Vector3 cameraOffset = xrOrigin.transform.position - camera.position;

        // Ставим XR Origin так, чтобы камера оказалась в seatPoint
        xrOrigin.transform.position = seatPoint.position + cameraOffset;
        xrOrigin.transform.rotation = seatPoint.rotation;

        xrOrigin.transform.SetParent(telega.transform, true);

        Destroy(boardButton);

        isBoarded = true;

        Debug.Log("Игрок сел в тележку");
    }

    public bool IsBoarded()
    {
        return isBoarded;
    }
}