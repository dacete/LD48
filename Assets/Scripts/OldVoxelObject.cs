using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class OldVoxelObject : MonoBehaviour
{
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
    }
}
