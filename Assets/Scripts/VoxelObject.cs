using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class VoxelObject : MonoBehaviour
{
    public Vector3 speed;
    public float offset, mult, speed2;
    public Line[] lines;
    public NativeArray<Line> nLines;
    // Start is called before the first frame update
    void Start()
    {
        nLines = new NativeArray<Line>(lines.Length, Allocator.Persistent);
        for (int i = 0; i < lines.Length; i++)
        {
            nLines[i] = lines[i];
        }
        FindObjectOfType<Disposer>().CallDispose += nLines.Dispose;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(speed*Time.deltaTime);
        transform.localScale = new float3(mult*Mathf.Sin(Time.realtimeSinceStartup * speed2) + offset);
    }
}
