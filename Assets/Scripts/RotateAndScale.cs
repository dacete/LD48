using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class RotateAndScale : MonoBehaviour
{
    public Vector3 speed;
    public float offset, mult, speed2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(speed * Time.deltaTime);
        transform.localScale = new float3(mult * Mathf.Sin(Time.realtimeSinceStartup * speed2) + offset);

    }
}
