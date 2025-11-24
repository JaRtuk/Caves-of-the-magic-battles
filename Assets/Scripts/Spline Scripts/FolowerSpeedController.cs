using UnityEngine;
using System.Collections;

public class FolowerSpeedController : MonoBehaviour
{
    public float speedChange;
    public bool addSpeed;

    [Header("Capsule Effects")]
    public bool applyTiltEffect = true;
    public float temporaryTiltIntensity = 10f;
    public float tiltDuration = 1.0f;

    void OnTriggerEnter(Collider other)
    {
        SplineFollower follower = other.GetComponent<SplineFollower>();
        FutuRiftCapsuleController capsule = other.GetComponent<FutuRiftCapsuleController>();

        if (follower != null)
        {
            float oldSpeed = follower.currentSpeed;
            
            if (addSpeed)
                follower.SetSpeed(follower.currentSpeed + speedChange);
            else
                follower.SetSpeed(follower.currentSpeed - speedChange);

            // Apply temporary tilt effect during speed change
            if (applyTiltEffect && capsule != null)
            {
                float tiltDirection = addSpeed ? -1f : 1f; // Backward tilt for acceleration, forward for deceleration
                StartCoroutine(ApplyTemporaryTilt(capsule, tiltDirection));
            }
        }
    }

    private IEnumerator ApplyTemporaryTilt(FutuRiftCapsuleController capsule, float direction)
    {
        float elapsed = 0f;
        
        while (elapsed < tiltDuration)
        {
            float progress = elapsed / tiltDuration;
            float currentTilt = Mathf.Lerp(direction * temporaryTiltIntensity, 0f, progress);
            capsule.SetManualTilt(currentTilt, capsule.GetMotionData().currentRoll);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we return to normal
        capsule.SetManualTilt(0f, capsule.GetMotionData().currentRoll);
    }
}