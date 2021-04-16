using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Segway : MonoBehaviour
{
    public WheelJoint2D wheel;
    public Rigidbody2D rb;
    public float motorSpeed;
    public float jump;
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var motor = wheel.motor;
        motor.motorSpeed = Mathf.DeltaAngle(Input.GetAxis("Horizontal") * speed, transform.eulerAngles.z)*motorSpeed;
        wheel.motor = motor;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector2.up * jump, ForceMode2D.Impulse);
        }
        //rb.AddForceAtPosition(transform.TransformDirection(new Vector2(Input.GetAxisRaw("Horizontal") * speed, 0)), transform.TransformPoint(new Vector2(0,2)));
    }
}
