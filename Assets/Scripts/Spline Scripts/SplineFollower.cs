//using UnityEngine;
//using UnityEngine;
//using System.Collections.Generic;

//public class SplineFollower : MonoBehaviour
//{
//    [Header("Spline Configuration")]
//    public Spline currentSpline;
//    public bool startAutomatically = true;

//    [Header("Movement Settings")]
//    public float baseSpeed = 5.0f;
//    public float currentSpeed;
//    private float traveledDistance = 0f; // теперь мы двигаемся по длине, а не по t

//    [Range(0, 1)] public float t = 0f;
//    public bool isMoving = true;

//    [Header("VR Rotation Settings - Critical for Comfort")]
//    [Tooltip("Higher values = sharper turns, Lower = smoother. Recommended 2-3 for VR")]
//    public float rotationSmoothness = 2.5f;
//    [Tooltip("More accurate but slightly more expensive")]
//    public bool useExactDirection = true;
//    [Tooltip("Custom up vector for banking on turns")]
//    public bool useCustomUpVector = false;
//    public Vector3 customUpVector = Vector3.up;

//    [Header("Path Completion")]
//    public bool loopPath = true;
//    public bool stopAtEnd = false;

//    // Private variables
//    private Quaternion targetRotation;
//    private float splineLength = 0f;
//    private bool hasCompletedPath = false;

//    // Events for external scripts
//    public System.Action OnPathStarted;
//    public System.Action OnPathCompleted;
//    public System.Action<float> OnProgressChanged; // Parameter: progress 0-1

//    void Start()
//    {
//        currentSpeed = baseSpeed;
//        targetRotation = transform.rotation;

//        if (currentSpline != null)
//            splineLength = CalculateSplineLength();

//        if (startAutomatically)
//            StartMoving();
//    }

//    void Update()
//    {
//        if (!isMoving ||currentSpline == null || currentSpline.controlPoints.Count < 2 || hasCompletedPath)
//            return;

//        // двигаемся по расстоянию, а не по t
//        traveledDistance += currentSpeed * Time.deltaTime;

//        if (traveledDistance > splineLength)
//        {
//            if (loopPath)
//            {
//                traveledDistance %= splineLength;
//            }
//            else
//            {
//                HandlePathCompletion();
//                return;
//            }
//        }

//        t = traveledDistance / splineLength;
//        t = Mathf.Clamp01(t);

//        // Обновляем позицию
//        Vector3 newPosition = currentSpline.GetPointAt(t);
//        transform.position = newPosition;

//        // Обновляем поворот
//        UpdateRotation(t);

//        // События прогресса
//        OnProgressChanged?.Invoke(t);
//    }

//    private void UpdateRotation(float tValue)
//    {
//        float lookAhead = 0.01f; // насколько вперёд смотреть для плавного поворота
//        float futureT = Mathf.Clamp01(tValue + lookAhead);

//        Vector3 direction = useExactDirection ?
//            (currentSpline.GetPointAt(futureT) - currentSpline.GetPointAt(tValue)).normalized :
//            currentSpline.GetDirectionAt(tValue);

//        if (direction != Vector3.zero)
//        {
//            Vector3 upVector = useCustomUpVector ? customUpVector : Vector3.up;
//            targetRotation = Quaternion.LookRotation(direction, upVector);

//            // сглаживание поворота
//            transform.rotation = Quaternion.Slerp(
//                transform.rotation,
//                targetRotation,
//                rotationSmoothness * Time.deltaTime
//            );
//        }
//    }

//    private float CalculateSplineLength()
//    {
//        float length = 0f;
//        int segments = Mathf.Max(40, currentSpline.controlPoints.Count * 4);
//        Vector3 prev = currentSpline.GetPointAt(0f);
//        for (int i = 1; i <= segments; i++)
//        {
//            float t = i / (float)segments;
//            Vector3 p = currentSpline.GetPointAt(t);
//            length += Vector3.Distance(prev, p);
//            prev = p;
//        }
//        return length;
//    }

//    private void HandlePathCompletion()
//    {
//        if (loopPath)
//        {
//            traveledDistance = 0f;
//            t = 0f;
//        }
//        else if (stopAtEnd)
//        {
//            isMoving = false;
//            hasCompletedPath = true;
//            OnPathCompleted?.Invoke();
//        }
//        else
//        {
//            hasCompletedPath = true;
//            OnPathCompleted?.Invoke();
//        }
//    }

//        // Public methods for external control
//    public void StartMoving()
//    {
//        isMoving = true;
//        hasCompletedPath = false;
//        OnPathStarted?.Invoke();
//    }

//    public void StopMoving()
//    {
//        isMoving = false;
//    }

//    public void SetSpeed(float newSpeed)
//    {
//        currentSpeed = newSpeed;
//    }

//    public void SetProgress(float progress)
//    {
//        t = Mathf.Clamp01(progress);
//        traveledDistance = t * splineLength;
//    }

//    public void SwitchSpline(Spline newSpline, bool resetProgress = true)
//    {
//        currentSpline = newSpline;
//        if (resetProgress)
//        {
//            traveledDistance = 0f;
//            t = 0f;
//            hasCompletedPath = false;
//        }
//        splineLength = CalculateSplineLength();
//    }

//    public float GetProgress() => t;
//    public bool IsPathCompleted() => hasCompletedPath;

//    private void OnDrawGizmos()
//    {
//        if (currentSpline != null && isMoving)
//        {
//            Gizmos.color = Color.green;
//            Gizmos.DrawWireSphere(transform.position, 0.3f);

//            Vector3 direction = transform.forward * 1f;
//            Gizmos.color = Color.blue;
//            Gizmos.DrawLine(transform.position, transform.position + direction);
//        }
//    }
//}


using UnityEngine.UIElements;

using UnityEngine;
using UnityEngine;
using System.Collections.Generic;

public class SplineFollower : MonoBehaviour
{
    [Header("Spline Configuration")]
    public Spline currentSpline;
    public bool startAutomatically = true;

    [Header("Movement Settings")]
    public float baseSpeed = 5.0f;
    public float currentSpeed;
    private float traveledDistance = 0f;

    [Range(0, 1)] public float t = 0f;
    public bool isMoving = true;

    [Header("VR Rotation Settings - Critical for Comfort")]
    [Tooltip("Higher values = sharper turns, Lower = smoother. Recommended 2-3 for VR")]
    public float rotationSmoothness = 2.5f;
    [Tooltip("More accurate but slightly more expensive")]
    public bool useExactDirection = true;

    [Header("Banking Settings")]
    [Tooltip("Multiplier for how strongly the cart banks into turns.")]
    public float bankingStrength = 2.0f;
    [Tooltip("Maximum bank angle in degrees.")]
    public float maxBankAngle = 25f;

    [Header("Path Completion")]
    public bool loopPath = true;
    public bool stopAtEnd = false;

    // Private variables
    private Quaternion targetRotation;
    private float splineLength = 0f;
    private bool hasCompletedPath = false;

    // Events for external scripts
    public System.Action OnPathStarted;
    public System.Action OnPathCompleted;
    public System.Action<float> OnProgressChanged;

    void Start()
    {
        currentSpeed = baseSpeed;
        targetRotation = transform.rotation;

        if (currentSpline != null)
            splineLength = CalculateSplineLength();

        if (startAutomatically)
            StartMoving();
    }

    void Update()
    {
        if (!isMoving || currentSpline == null || currentSpline.controlPoints.Count < 2 || hasCompletedPath)
            return;

        // Двигаемся по длине
        traveledDistance += currentSpeed * Time.deltaTime;

        if (traveledDistance > splineLength)
        {
            if (loopPath)
            {
                traveledDistance %= splineLength;
            }
            else
            {
                HandlePathCompletion();
                return;
            }
        }

        t = traveledDistance / splineLength;
        t = Mathf.Clamp01(t);

        // Позиция
        Vector3 newPosition = currentSpline.GetPointAt(t);
        transform.position = newPosition;

        // Поворот с банкингом
        UpdateRotationWithBanking(t);

        // Прогресс
        OnProgressChanged?.Invoke(t);
    }

    private void UpdateRotationWithBanking(float tValue)
    {
        float lookAhead = 0.01f;
        float futureT = Mathf.Clamp01(tValue + lookAhead);

        Vector3 currentPoint = currentSpline.GetPointAt(tValue);
        Vector3 futurePoint = currentSpline.GetPointAt(futureT);
        Vector3 direction = (futurePoint - currentPoint).normalized;

        if (direction != Vector3.zero)
        {
            // Обычный up-вектор
            Vector3 upVector = Vector3.up;

            // Считаем кривизну (как сильно направление меняется)
            Vector3 futureDirection = (currentSpline.GetPointAt(Mathf.Clamp01(futureT + lookAhead)) - futurePoint).normalized;
            float turnAmount = Vector3.SignedAngle(direction, futureDirection, Vector3.up);

            // Рассчитываем угол наклона (банкинг)
            float bankAngle = Mathf.Clamp(-turnAmount * bankingStrength, -maxBankAngle, maxBankAngle);

            // Делаем поворот с учётом банкинга
            targetRotation = Quaternion.LookRotation(direction, upVector) * Quaternion.Euler(0, 0, bankAngle);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSmoothness * Time.deltaTime
            );
        }
    }

    private float CalculateSplineLength()
{
        float length = 0f;
        int segments = Mathf.Max(40, currentSpline.controlPoints.Count * 4);
        Vector3 prev = currentSpline.GetPointAt(0f);
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 p = currentSpline.GetPointAt(t);
            length += Vector3.Distance(prev, p);
            prev = p;
        }
        return length;
    }

    private void HandlePathCompletion()
    {
        if (loopPath)
        {
            traveledDistance = 0f;
            t = 0f;
        }
        else if (stopAtEnd)
        {
            isMoving = false;
            hasCompletedPath = true;
            OnPathCompleted?.Invoke();
        }
        else
        {
            hasCompletedPath = true;
            OnPathCompleted?.Invoke();
        }
    }

    // Public API
    public void StartMoving()
    {
        isMoving = true;
        hasCompletedPath = false;
        OnPathStarted?.Invoke();
    }

    public void StopMoving() => isMoving = false;

    public void SetSpeed(float newSpeed) => currentSpeed = newSpeed;

    public void SetProgress(float progress)
    {
        t = Mathf.Clamp01(progress);
        traveledDistance = t * splineLength;
    }

    public void SwitchSpline(Spline newSpline, bool resetProgress = true)
    {
        currentSpline = newSpline;
        if (resetProgress)
        {
            traveledDistance = 0f;
            t = 0f;
            hasCompletedPath = false;
        }
        splineLength = CalculateSplineLength();
    }

    public float GetProgress() => t;
    public bool IsPathCompleted() => hasCompletedPath;

    private void OnDrawGizmos()
    {
        if (currentSpline != null && isMoving)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.3f);

            Vector3 direction = transform.forward * 1f;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + direction);
        }
    }
}
