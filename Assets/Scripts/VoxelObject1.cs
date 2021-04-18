using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Rendering;
using Unity.Mathematics;
using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine.UI;
using Unity.Collections.LowLevel.Unsafe;
using System.Runtime.InteropServices;

public class VoxelObject : MonoBehaviour
{
    public NativeArray<byte> voxels;
    public int3 size;
    public int users;
    bool destroyed;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void Initialize()
    {
        voxels = new NativeArray<byte>(size.x * size.y * size.z,Allocator.Persistent);
    }
    public void Dispose()
    {
        if (destroyed) return;
        voxels.Dispose();
    }


    private void OnDestroy()
    {
        Dispose();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
