using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class HighlightVoxels : MonoBehaviour
{
    public int3 startPos, endPos;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public float offset, thickness;
    NativeHighlightMeshData meshData;
    Mesh mesh;
    public bool draw;
    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        meshData = new NativeHighlightMeshData();
        meshData.Initialize();
        FindObjectOfType<Disposer>().CallDispose += Dispose;
    }

    // Update is called once per frame
    void Update()
    {
        if (draw)
        {
            var job = new HighlightMeshJob()
            {
                startPos = (float3)startPos * Constants.voxelSize - (offset * Constants.voxelSize * new float3(1)),
                endPos = (float3)endPos * Constants.voxelSize+(offset * Constants.voxelSize * new float3(1)),
                thickness = thickness, mesh = meshData
            };
            job.Schedule().Complete();
            var vertexCount = meshData.vertexCount[0];
            mesh.Clear();
            mesh.SetVertexBufferParams(vertexCount, BurstConstants.highlightMeshLayout);
            mesh.SetIndexBufferParams(vertexCount, IndexFormat.UInt32);
            mesh.subMeshCount = 1;
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, vertexCount, MeshTopology.Quads), BurstConstants.chunkMeshUpdateFlags);
            mesh.SetVertexBufferData<float4>(meshData.posVertices, 0, 0, vertexCount, 0, BurstConstants.chunkMeshUpdateFlags);
            mesh.SetIndexBufferData<int>(meshData.Indices, 0, 0, vertexCount, BurstConstants.chunkMeshUpdateFlags);
            mesh.bounds = new Bounds(new float3(0.5f), new float3(1f));
            meshFilter.sharedMesh = mesh;
        }
        else
        {

        }
    }
    public void Dispose()
    {
        meshData.Dispose();
    }
}

[BurstCompile]
public struct HighlightMeshJob : IJob
{
    public NativeHighlightMeshData mesh;
    public float3 startPos, endPos;
    public float thickness;
    int counter;
    int3 one;
    public void Execute()
    {
        one = new int3(1);
        mesh.Clear();
        for (byte p = 0; p < 6; p++)
        {
            BuildFace(p);
        }
        mesh.vertexCount[0] = counter;
    }
    void BuildFace(byte p)
    {
        var v0 = (float3)BurstConstants.voxelVerts[BurstConstants.voxelTris[p].x];
        var v1 = (float3)BurstConstants.voxelVerts[BurstConstants.voxelTris[p].y];
        var v2 = (float3)BurstConstants.voxelVerts[BurstConstants.voxelTris[p].z];
        var v3 = (float3)BurstConstants.voxelVerts[BurstConstants.voxelTris[p].w];

        v0 = ((one - v0) * startPos + v0 * endPos);
        v1 = ((one - v1) * startPos + v1 * endPos);
        v2 = ((one - v2) * startPos + v2 * endPos);
        v3 = ((one - v3) * startPos + v3 * endPos);

        AddQuad(v0,v1,v2,v3);
    }
    void AddQuad(float3 v0, float3 v1, float3 v2, float3 v3)
    {
        mesh.posVertices.Add(new float4(v0, 0));
        mesh.posVertices.Add(new float4(v1, 0));
        mesh.posVertices.Add(new float4(v2, 0));
        mesh.posVertices.Add(new float4(v3, 0));
        mesh.Indices.Add(0 + counter);
        mesh.Indices.Add(1 + counter);
        mesh.Indices.Add(3 + counter);
        mesh.Indices.Add(2 + counter);
        counter += 4;
    }

}