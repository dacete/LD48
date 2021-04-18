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

public unsafe class Voxel : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    NativeArray<byte> voxels;
    public OldVoxelObject obj;
    NativeChunkMeshData meshData;
    Mesh mesh;
    // Start is called before the first frame update
    void Start()
    {
        PointUtilities.to1D(new int3(), 2, out var g);
        mesh = new Mesh();
        meshFilter.mesh = mesh;
        var dsp = FindObjectOfType<Disposer>();
        dsp.CallDispose += Dispose;
        dsp.CallStopJob += StopJob;
        meshData = new NativeChunkMeshData();
        meshData.Initialize();
        voxels = new NativeArray<byte>(Constants.cubedChunkSize, Allocator.Persistent);
        PointUtilities.to1D(new int3(12, 12, 12), 32, out var i);
        voxels[i] = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (true)
        {
            meshData.Clear();
            //var clearJob = new ClearJob() { voxels = voxels };
            //clearJob.Schedule(voxels.Length / 64, 64).Complete();
            UnsafeUtility.MemClear(voxels.GetUnsafePtr(), voxels.Length);

            obj.nLines.CopyFrom(obj.lines);

            var job = new LineJob()
            {
                lines = obj.nLines,
                position = obj.transform.position,
                scale = obj.transform.localScale,
                rotation = obj.transform.rotation,
                voxels = voxels
            };
            job.Schedule(obj.nLines.Length, 1).Complete();
            var meshJob = new BuildMeshJob()
            {
                mesh = meshData,
                voxels = voxels
            };
            meshJob.Schedule().Complete();
            var vertexCount = meshData.vertexCount[0];
            print(vertexCount); mesh.Clear();
            mesh.SetVertexBufferParams(vertexCount, BurstConstants.chunkMeshLayout);
            mesh.SetIndexBufferParams(vertexCount, IndexFormat.UInt32);
            mesh.subMeshCount = 1;
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, vertexCount, MeshTopology.Quads), BurstConstants.chunkMeshUpdateFlags);
            mesh.SetVertexBufferData<PositionVertex>(meshData.posVertices, 0, 0, vertexCount, 0, BurstConstants.chunkMeshUpdateFlags);
            mesh.SetVertexBufferData<NormalVertex>(meshData.normVertices, 0, 0, vertexCount, 1, BurstConstants.chunkMeshUpdateFlags);
            mesh.SetIndexBufferData<int>(meshData.Indices, 0, 0, vertexCount, BurstConstants.chunkMeshUpdateFlags);
            mesh.bounds = new Bounds(new float3(0.5f), new float3(1f));
            //mesh.RecalculateBounds();
            meshFilter.sharedMesh = mesh;
        }
        
       
    }



    public void StopJob()
    {

    }
    public void Dispose()
    {
        meshData.Dispose();
        voxels.Dispose();
    }
}

