using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidBodySpeedCap : MonoBehaviour
{
    Rigidbody rb = null;
    [SerializeField]
    float MaxSpeed = 3f;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, MaxSpeed);
    }
}
