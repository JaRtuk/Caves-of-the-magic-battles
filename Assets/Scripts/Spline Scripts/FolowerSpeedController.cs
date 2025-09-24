using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class FolowerSpeedController : MonoBehaviour
{
    public float speedChange;
    public bool addSpeed;

    void OnTriggerEnter(Collider other)
    {
        SplineFollower follower = other.GetComponent<SplineFollower>();

        if (follower != null)
        {
            if (addSpeed)
                follower.currentSpeed += speedChange;
            else
                follower.currentSpeed -= speedChange;
        }
    }
}
