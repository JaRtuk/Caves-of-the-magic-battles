// using UnityEngine;

// public class SplineFollower : MonoBehaviour
// {
//     public Spline currentSpline; // Текущий сплайн, по которому едет тележка
//     public float baseSpeed = 5.0f; // Базовая скорость
//     public float currentSpeed; // Текущая скорость (может меняться извне)
//     [Range(0, 1)] public float t = 0f; // Текущая позиция на сплайне (0 - начало, 1 - конец)

//     [Header("Rotation Settings")]
//     public float rotationSmoothness = 5.0f; // Плавность поворота (чем больше, тем резче)
//     public bool useExactDirection = true; // Использовать аналитическое направление

//     // Для плавного поворота
//     private Quaternion targetRotation;

//     void Start()
//     {
//         currentSpeed = baseSpeed;
//         targetRotation = transform.rotation;
//     }

//     void Update()
//     {
//         if (currentSpline == null || currentSpline.controlPoints.Count < 2) return;
//         // Вычисляем новую позицию на сплайне
//         // Вместо прибавления к t, мы прибавляем к пройденному расстоянию для более равномерной скорости
//         float distanceToMove = currentSpeed * Time.deltaTime;
//         // (Здесь нужно преобразовать distanceToMove в приращение t, учитывая длину сплайна.
//         // Для простоты примера используем упрощенный вариант)
//         t += distanceToMove / 100f; // 100f - условная "длина" сплайна. Лучше вычислить реальную длину.

//         // Зацикливаем или останавливаемся в конце пути
//         if (t >= 1.0f)
//         {
//             if (currentSpline.isLoop)
//             {
//                 t -= 1.0f;
//             }
//             else
//             {
//                 t = 1.0f;
//                 // Здесь можно вызвать событие окончания пути
//             }
//         }

//         // Устанавливаем позицию и поворот тележки
//         Vector3 newPosition = currentSpline.GetPointAt(t);
//         transform.position = newPosition;


//         UpdateRotation(t);
//     }

//     private void UpdateRotation(float tValue)
//     {
//         // Получаем направление движения
//         Vector3 direction = useExactDirection ? 
//             currentSpline.GetExactDirectionAt(tValue) : 
//             currentSpline.GetDirectionAt(tValue);

//         // Если направление валидно
//         if (direction != Vector3.zero)
//         {
//             // Вычисляем целевой поворот
//             targetRotation = Quaternion.LookRotation(direction);
            
//             // Плавно интерполируем к целевому повороту
//             transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothness * Time.deltaTime);
//         }
//     }

//     private float GetApproximateSplineLength()
//     {
//         float length = 0f;
//         int segments = 20; // Количество сегментов для вычисления длины
        
//         for (int i = 0; i < segments; i++)
//         {
//             float t1 = i / (float)segments;
//             float t2 = (i + 1) / (float)segments;
            
//             Vector3 p1 = currentSpline.GetPointAt(t1);
//             Vector3 p2 = currentSpline.GetPointAt(t2);
            
//             length += Vector3.Distance(p1, p2);
//         }
        
//         return length;
//     }

//     // Метод для смены пути на перекрестке
//     public void SwitchSpline(Spline newSpline, bool resetT = true)
//     {
//         currentSpline = newSpline;
//         if (resetT) t = 0f;
//     }

//     // Метод для изменения скорости (например, при спуске или подъеме)
//     public void SetSpeed(float newSpeed)
//     {
//         currentSpeed = newSpeed;
//     }
// }


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
    [Range(0, 1)] public float t = 0f;
    public bool isMoving = true;
    
    [Header("VR Rotation Settings - Critical for Comfort")]
    [Tooltip("Higher values = sharper turns, Lower = smoother. Recommended 2-3 for VR")]
    public float rotationSmoothness = 2.5f;
    [Tooltip("More accurate but slightly more expensive")]
    public bool useExactDirection = true;
    [Tooltip("Custom up vector for banking on turns")]
    public bool useCustomUpVector = false;
    public Vector3 customUpVector = Vector3.up;
    
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
    public System.Action<float> OnProgressChanged; // Parameter: progress 0-1
    
    void Start()
    {
        currentSpeed = baseSpeed;
        targetRotation = transform.rotation;
        
        if (currentSpline != null)
        {
            splineLength = CalculateSplineLength();
        }
        
        if (startAutomatically)
        {
            StartMoving();
        }
    }

    void Update()
    {
        if (!isMoving || currentSpline == null || currentSpline.controlPoints.Count < 2 || hasCompletedPath)
            return;

        // Calculate movement based on spline length for consistent speed
        float distanceToMove = currentSpeed * Time.deltaTime;
        float tIncrement = distanceToMove / splineLength;
        t += tIncrement;
        
        // Clamp t between 0 and 1
        t = Mathf.Clamp01(t);
        
        // Update position
        Vector3 newPosition = currentSpline.GetPointAt(t);
        transform.position = newPosition;
        
        // Update rotation smoothly for VR comfort
        UpdateRotation(t);
        
        // Trigger progress event
        OnProgressChanged?.Invoke(t);
        
        // Check if path is completed
        if (t >= 0.999f)
        {
            HandlePathCompletion();
        }
    }
    
    private void UpdateRotation(float tValue)
    {
        Vector3 direction = useExactDirection ? 
            currentSpline.GetExactDirectionAt(tValue) : 
            currentSpline.GetDirectionAt(tValue);

        if (direction != Vector3.zero)
        {
            Vector3 upVector = useCustomUpVector ? customUpVector : CalculateDynamicUpVector(tValue);
            
            targetRotation = Quaternion.LookRotation(direction, upVector);
            
            // Use Slerp for smooth rotation - CRITICAL FOR VR COMFORT
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 
                rotationSmoothness * Time.deltaTime);
        }
    }
    
    private Vector3 CalculateDynamicUpVector(float tValue)
    {
        // Simple implementation - you can enhance this for banking effects
        if (useCustomUpVector)
            return customUpVector;
            
        // Basic up vector that tries to stay upright but can be modified
        return Vector3.up;
    }
    
    private float CalculateSplineLength()
    {
        float length = 0f;
        int segments = Mathf.Max(20, currentSpline.controlPoints.Count * 2);
        
        for (int i = 0; i < segments; i++)
        {
            float t1 = i / (float)segments;
            float t2 = (i + 1) / (float)segments;
            
            Vector3 p1 = currentSpline.GetPointAt(t1);
            Vector3 p2 = currentSpline.GetPointAt(t2);
            
            length += Vector3.Distance(p1, p2);
        }
        
        return length;
    }
    
    private void HandlePathCompletion()
    {
        if (loopPath)
        {
            t = 0f;
            // Optionally reset speed or other parameters here
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
    
    // Public methods for external control
    public void StartMoving()
    {
        isMoving = true;
        hasCompletedPath = false;
        OnPathStarted?.Invoke();
    }
    
    public void StopMoving()
    {
        isMoving = false;
    }
    
    public void SetSpeed(float newSpeed)
    {
        currentSpeed = newSpeed;
    }
    
    public void SetProgress(float progress)
    {
        t = Mathf.Clamp01(progress);
    }
    
    public void SwitchSpline(Spline newSpline, bool resetProgress = true)
    {
        currentSpline = newSpline;
        if (resetProgress)
        {
            t = 0f;
            hasCompletedPath = false;
        }
        splineLength = CalculateSplineLength();
    }
    
    public float GetProgress()
    {
        return t;
    }
    
    public bool IsPathCompleted()
    {
        return hasCompletedPath;
    }
    
    // Debug visualization
    private void OnDrawGizmos()
    {
        if (currentSpline != null && isMoving)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
            
            // Draw direction indicator
            Vector3 direction = transform.forward * 1f;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + direction);
        }
    }
}


// using UnityEngine;

// public class SplineFollower : MonoBehaviour
// {
//     [Header("Spline Configuration")]
//     public Spline currentSpline;

//     [Header("VR Movement Settings")]
//     public float speed = 3.0f;
//     [Range(0, 1)] public float t = 0f;
//     public bool isMoving = true;

//     [Header("VR Rotation Settings - Critical for Comfort")]
//     [Tooltip("Smoothness of rotation (higher = sharper)")]
//     public float rotationSmoothness = 2.0f;

//     [Header("Rail Alignment")]
//     [Tooltip("How much to align with rail normal (0 = no alignment, 1 = full alignment)")]
//     public float railAlignmentStrength = 1.0f;

//     void Update()
//     {
//         if (!isMoving || currentSpline == null) return;

//         // Movement
//         t += speed * Time.deltaTime / 100f;
//         if (t >= 1f) t = 0f;

//         // Position
//         transform.position = currentSpline.GetPointAt(t);

//         // ROTATION - PERPENDICULAR TO RAILS
//         UpdateRailAlignment(t);
//     }

//     private void UpdateRailAlignment(float tValue)
//     {
//         // Get the direction (tangent) of the spline at this point
//         Vector3 tangent = currentSpline.GetExactDirectionAt(tValue);

//         if (tangent == Vector3.zero) return;

//         // Calculate normal vector perpendicular to the rails
//         // For simple cases, we can use cross product with world up vector
//         Vector3 normal = Vector3.Cross(tangent, Vector3.up).normalized;

//         // If normal is zero (when tangent is parallel to up), use a different approach
//         if (normal == Vector3.zero)
//         {
//             normal = Vector3.Cross(tangent, Vector3.forward).normalized;
//         }

//         // Calculate binormal (the other perpendicular vector)
//         Vector3 binormal = Vector3.Cross(tangent, normal).normalized;

//         // Create rotation that aligns with the rail coordinate system
//         Quaternion targetRotation = Quaternion.LookRotation(tangent, binormal);

//         // Smooth rotation for VR comfort
//         transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
//             rotationSmoothness * Time.deltaTime);
//     }

//     // Enhanced method for better rail alignment (more advanced)
//     private void UpdateAdvancedRailAlignment(float tValue)
//     {
//         Vector3 tangent = currentSpline.GetExactDirectionAt(tValue);
//         if (tangent == Vector3.zero) return;

//         // Sample nearby points to calculate a more accurate normal
//         float sampleOffset = 0.05f;
//         Vector3 pointBefore = currentSpline.GetPointAt(Mathf.Clamp01(tValue - sampleOffset));
//         Vector3 pointAfter = currentSpline.GetPointAt(Mathf.Clamp01(tValue + sampleOffset));

//         Vector3 segmentDirection = (pointAfter - pointBefore).normalized;

//         // Calculate normal using cross products
//         Vector3 worldUp = Vector3.up;
//         Vector3 normal = Vector3.Cross(segmentDirection, worldUp).normalized;

//         // If normal is degenerate, try alternative axes
//         if (normal.magnitude < 0.1f)
//         {
//             normal = Vector3.Cross(segmentDirection, Vector3.forward).normalized;
//         }

//         // Recalculate binormal to ensure orthogonality
//         Vector3 binormal = Vector3.Cross(tangent, normal).normalized;
//         normal = Vector3.Cross(binormal, tangent).normalized; // Ensure orthogonality

//         Quaternion targetRotation = Quaternion.LookRotation(tangent, normal);

//         // Apply rotation with smoothing
//         transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
//             rotationSmoothness * Time.deltaTime);
//     }

//     public void SetSpeed(float newSpeed) => speed = newSpeed;
//     public void Stop() => isMoving = false;
//     public void StartMoving() => isMoving = true;

//     // Visual debug in scene view
//     private void OnDrawGizmosSelected()
//     {
//         if (currentSpline != null && isMoving)
//         {
//             // Draw forward direction (red)
//             Gizmos.color = Color.red;
//             Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);

//             // Draw right direction (green) - perpendicular to rails
//             Gizmos.color = Color.green;
//             Gizmos.DrawLine(transform.position, transform.position + transform.right * 1f);

//             // Draw up direction (blue) - perpendicular to rails
//             Gizmos.color = Color.blue;
//             Gizmos.DrawLine(transform.position, transform.position + transform.up * 1f);
//         }
//     }
// }


