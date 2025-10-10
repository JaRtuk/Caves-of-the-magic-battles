using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Sit : MonoBehaviour
{
    [SerializeField] public GameObject Object;
    private Vector3 targetPos;

    private void Start()
    {
        targetPos = new Vector3(0, -0.3f, 0);
        gameObject.transform.localPosition += targetPos;
    }
    void Ubdate()
    {
        //gameObject.transform.localPosition = targetPos;
    }
}
