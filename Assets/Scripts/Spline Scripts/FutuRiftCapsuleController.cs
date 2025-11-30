using UnityEngine;
using System.Collections.Generic;
using Futurift;
using Futurift.DataSenders;
using Futurift.Options;

public class FutuRiftCapsuleController : MonoBehaviour
{
    [Header("FutuRift Connection")]
    [SerializeField] private string ipAddress = "127.0.0.1";
    [SerializeField] private int port = 6065;

    [Header("Motion Effects")]
    [Tooltip("Intensity of backward tilt during acceleration")]
    public float accelerationTiltIntensity = 15f;
    [Tooltip("Intensity of sideways tilt during turns")]
    public float turnTiltIntensity = 20f;
    [Tooltip("How quickly the capsule returns to neutral position")]
    public float tiltRecoverySpeed = 3f;
    [Tooltip("Max tilt angle in degrees")]
    public float maxTiltAngle = 25f;

    public VRBoardingSystem _vRBoardingSystem;

    // Motion tracking
    private Vector3 _lastPosition;
    private Vector3 _velocity;
    private Vector3 _lastVelocity;
    private Vector3 _acceleration;
    private Vector3 _lastForward;
    private float _angularVelocity;
    
    // Current tilt values
    private float _currentPitch = 0f;
    private float _currentRoll = 0f;

    private FutuRiftController _futuRiftController;

    private bool _playerInTrolley = false;

    void Awake()
    {
        InitializeFutuRift();
        _lastPosition = transform.position;
        _lastForward = transform.forward;
    }

    void Update()
    {
        _playerInTrolley = _vRBoardingSystem.IsBoarded();
        if(_playerInTrolley)
        {
            CalculateMotion();
            ApplyCapsuleTilting();
        }
    }


    private void InitializeFutuRift()
    {
        var udpOptions = new UdpOptions
        {
            ip = ipAddress,
            port = port
        };
        _futuRiftController = new FutuRiftController(new UdpPortSender(udpOptions));
    }

    public void CalculateMotion()
    {
        // Calculate linear velocity and acceleration
        _velocity = (transform.position - _lastPosition) / Time.deltaTime;
        _acceleration = (_velocity - _lastVelocity) / Time.deltaTime;

        // Calculate angular velocity (turn rate)
        float angleDifference = Vector3.SignedAngle(_lastForward, transform.forward, Vector3.up);
        _angularVelocity = angleDifference / Time.deltaTime;

        _lastPosition = transform.position;
        _lastVelocity = _velocity;
        _lastForward = transform.forward;
    }

    public void ApplyCapsuleTilting()
    {
        // Calculate acceleration tilt (pitch) - tilt BACK during acceleration
        float forwardAcceleration = Vector3.Dot(transform.forward, _acceleration);
        float targetPitch = -forwardAcceleration * accelerationTiltIntensity;

        // Calculate turn tilt (roll) - tilt sideways during turns
        float targetRoll = -_angularVelocity * turnTiltIntensity;

        // Apply limits
        targetPitch = Mathf.Clamp(targetPitch, -maxTiltAngle, maxTiltAngle);
        targetRoll = Mathf.Clamp(targetRoll, -maxTiltAngle, maxTiltAngle);

        // Smooth interpolation
        _currentPitch = Mathf.Lerp(_currentPitch, targetPitch, tiltRecoverySpeed * Time.deltaTime);
        _currentRoll = Mathf.Lerp(_currentRoll, targetRoll, tiltRecoverySpeed * Time.deltaTime);

        // Apply to FutuRift capsule
        if (_futuRiftController != null)
        {
            _futuRiftController.Pitch = _currentPitch;
            _futuRiftController.Roll = _currentRoll;
        }
    }

    // Public method to get motion data for debugging
    public MotionData GetMotionData()
    {
        return new MotionData
        {
            velocity = _velocity,
            acceleration = _acceleration,
            angularVelocity = _angularVelocity,
            currentPitch = _currentPitch,
            currentRoll = _currentRoll
        };
    }

    // Method to manually override tilts (for special effects)
    public void SetManualTilt(float pitch, float roll)
    {
        _currentPitch = Mathf.Clamp(pitch, -maxTiltAngle, maxTiltAngle);
        _currentRoll = Mathf.Clamp(roll, -maxTiltAngle, maxTiltAngle);
    }

    // Method to reset to neutral position
    public void ResetTilt()
    {
        _currentPitch = 0f;
        _currentRoll = 0f;
    }

    void OnEnable()
    {
        _futuRiftController?.Start();
    }

    void OnDisable()
    {
        _futuRiftController?.Stop();
    }
        
    public bool isInTrolley() 
    {
        return _playerInTrolley;
    }

    // Data structure for motion information
    [System.Serializable]
    public struct MotionData
    {
        public Vector3 velocity;
        public Vector3 acceleration;
        public float angularVelocity;
        public float currentPitch;
        public float currentRoll;
    }
}