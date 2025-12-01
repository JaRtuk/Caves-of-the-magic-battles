using UnityEngine;
using System.Collections.Generic;
using Bhaptics.SDK2;

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

    [Header("VR Rotation Settings")]
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

    // Reference to capsule controller
    [Header("Capsule Controller")]
    public FutuRiftCapsuleController capsuleController;

    // Private variables
    private Quaternion targetRotation;
    private float splineLength = 0f;
    private bool hasCompletedPath = false;

    private float hapticTimer = 0f;
    private float hapticInterval = 0.22f;

    // Events
    public System.Action OnPathStarted;
    public System.Action OnPathCompleted;
    public System.Action<float> OnProgressChanged;

    void Start()
    {
        currentSpeed = baseSpeed;
        targetRotation = transform.rotation;

        if (currentSpline != null)
            splineLength = CalculateSplineLength();

        // Auto-find capsule controller if not assigned
        if (capsuleController == null)
            capsuleController = GetComponent<FutuRiftCapsuleController>();

        if (startAutomatically)
            StartMoving();
    }

    void Update()
    {
        if (!isMoving || currentSpline == null || currentSpline.controlPoints.Count < 2 || hasCompletedPath)
            return;

        // Move along distance
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

        // Update position
        Vector3 newPosition = currentSpline.GetPointAt(t);
        transform.position = newPosition;

        // Update rotation
        UpdateRotation(t);

        if (currentSpeed > 0.05f && capsuleController != null && capsuleController.isInTrolley())
        {
            hapticTimer += Time.deltaTime;

            if (hapticTimer >= hapticInterval)
            {
                BhapticsLibrary.Play("ride_in_trolley");
                hapticTimer = 0f;
            }
        }
        else
        {
            hapticTimer = 0f;
        }

        OnProgressChanged?.Invoke(t);
    }

    private void UpdateRotation(float tValue)
    {
        float lookAhead = 0.01f;
        float futureT = Mathf.Clamp01(tValue + lookAhead);

        Vector3 direction = useExactDirection ?
            (currentSpline.GetPointAt(futureT) - currentSpline.GetPointAt(tValue)).normalized :
            currentSpline.GetDirectionAt(tValue);

        if (direction != Vector3.zero)
        {
            Vector3 upVector = useCustomUpVector ? customUpVector : Vector3.up;
            targetRotation = Quaternion.LookRotation(direction, upVector);

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

    // Public methods
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
}