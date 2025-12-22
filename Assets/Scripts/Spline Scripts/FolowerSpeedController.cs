using UnityEngine;
using System.Collections;

public class FolowerSpeedController : MonoBehaviour
{
    public SplineFollower follower;

    public float speedChange;
    public bool addSpeed;

    [Header("Capsule Effects")]
    public bool applyTiltEffect = true;
    public float temporaryTiltIntensity = 10f;
    public float tiltDuration = 1.0f;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != 6)
        {
            Debug.Log("Wrooooooooooong");
            return;
        }
        FutuRiftCapsuleController capsule = follower.GetComponent<FutuRiftCapsuleController>();

        if (follower != null)
        {
            float oldSpeed = follower.currentSpeed;
            
            if (addSpeed)
            {
                follower.SetSpeed(follower.currentSpeed + speedChange);
                Debug.Log("Speeeed Chenged");
            }
            else
            {
                follower.SetSpeed(follower.currentSpeed - speedChange);
                Debug.Log("Speeeed Chenged");
            }

            if (applyTiltEffect && capsule != null)
            {
                float tiltDirection = addSpeed ? -1f : 1f; 
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
        
        capsule.SetManualTilt(0f, capsule.GetMotionData().currentRoll);
    }
}