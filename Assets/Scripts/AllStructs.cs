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

[BurstCompile]
public struct BuildMeshJob : IJob
{
    [NativeDisableContainerSafetyRestriction]
    public NativeChunkMeshData mesh;
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<byte> voxels;
    int groundCounter;
    public void Execute()
    {
        for (int x = 0; x < Constants.chunkWidth - 0; x++)
        {

            for (int y = 0; y < Constants.chunkWidth - 0; y++)
            {
                for (int z = 0; z < Constants.chunkWidth - 0; z++)
                {
                    var localPos = new int3(x, y, z);
                    PointUtilities.to1D(localPos, Constants.chunkWidth, out var index);
                    var voxel = voxels[index];
                    if (voxel == 0)
                    {
                        continue;
                    }
                    for (byte p = 0; p < 6; p++)
                    {
                        var pOffset = localPos + BurstConstants.pLookup[p];
                        byte pVox;
                        if (IsInside(pOffset))
                        {
                            PointUtilities.to1D(pOffset, Constants.chunkWidth, out var pIndex);
                            pVox = voxels[pIndex];
                        }
                        else
                        {
                            pVox = 0;
                        }
                        if (pVox == 0)
                        {
                            BuildFace(p, localPos, voxel);
                        }
                    }
                }
            }
        }
        mesh.vertexCount[0] = groundCounter;
    }
    bool IsInside(int3 position)
    {
        if (position.x >= 0 && position.x < Constants.chunkWidth && position.y >= 0 && position.y < Constants.chunkWidth && position.z >= 0 && position.z < Constants.chunkWidth)
        {
            return true;
        }
        else return false;
    }
    Vector2 GetUvs(int texId, int corner)
    {
        Vector2 temp = new Vector2();
        float horizontalValue = texId % Constants.textureAtlasBlockWidth;
        float verticalValue = 15 - texId / Constants.textureAtlasBlockWidth;
        horizontalValue += corner % 2;
        verticalValue += corner / 2;
        horizontalValue /= 16F;
        verticalValue /= 16F;
        temp.x = horizontalValue;
        temp.y = verticalValue;
        return temp;
    }
    void BuildFace(byte p, int3 offset, byte texId)
    {
        mesh.posVertices.Add(new PositionVertex(offset + BurstConstants.voxelVerts[BurstConstants.voxelTris[p].x], texId));
        mesh.posVertices.Add(new PositionVertex(offset + BurstConstants.voxelVerts[BurstConstants.voxelTris[p].y], texId));
        mesh.posVertices.Add(new PositionVertex(offset + BurstConstants.voxelVerts[BurstConstants.voxelTris[p].z], texId));
        mesh.posVertices.Add(new PositionVertex(offset + BurstConstants.voxelVerts[BurstConstants.voxelTris[p].w], texId));
        float3 nor;
        switch (p)
        {
            case 0:
                nor = -BurstConstants.forward;
                break;
            case 1:
                nor = BurstConstants.forward;
                break;
            case 2:
                nor = BurstConstants.up;
                break;
            case 3:
                nor = -BurstConstants.up;
                break;
            case 4:
                nor = -BurstConstants.right;
                break;
            default:
                nor = BurstConstants.right;
                break;
        }
        mesh.normVertices.Add(new NormalVertex(nor));
        mesh.normVertices.Add(new NormalVertex(nor));
        mesh.normVertices.Add(new NormalVertex(nor));
        mesh.normVertices.Add(new NormalVertex(nor));
        mesh.Indices.Add(0 + groundCounter);
        mesh.Indices.Add(1 + groundCounter);
        mesh.Indices.Add(3 + groundCounter);
        mesh.Indices.Add(2 + groundCounter);
        groundCounter += 4;
    }
}

[BurstCompile]
public struct ClearJob : IJobParallelFor
{
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<byte> voxels;
    public void Execute(int i)
    {
        for (int g = 0; g < 64; g++)
        {
            voxels[i * 64 + g] = 0;
        }
    }
}

[BurstCompile]
public struct LineJob : IJobParallelFor
{
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<byte> voxels;
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<Line> lines;
    public float3 position;
    public quaternion rotation;
    public float3 scale;
    public void Execute(int i)
    {
        var line = lines[i];
        Traverse(TransformExtensions.TransformPointUnscaled(position, rotation, scale * line.start),
            TransformExtensions.TransformPointUnscaled(position, rotation, scale * line.end),
            line.id);
    }

    float FRAC0(float x)
    {
        return x - math.floor(x);
    }
    float FRAC1(float x)
    {
        return 1 - x + math.floor(x);
    }


    void Traverse(float3 start, float3 end, byte id)
    {
        float tMaxX, tMaxY, tMaxZ, tDeltaX, tDeltaY, tDeltaZ;
        int3 voxel;

        float x1, y1, z1; // start point   
        float x2, y2, z2; // end point   
        x1 = start.x;
        y1 = start.y;
        z1 = start.z;
        x2 = end.x;
        y2 = end.y;
        z2 = end.z;
        int dx = (int)math.sign(x2 - x1);
        if (dx != 0) tDeltaX = math.min(dx / (x2 - x1), 10000000.0f); else tDeltaX = 10000000.0f;
        if (dx > 0) tMaxX = tDeltaX * FRAC1(x1); else tMaxX = tDeltaX * FRAC0(x1);
        voxel.x = (int)x1;

        int dy = (int)math.sign(y2 - y1);
        if (dy != 0) tDeltaY = math.min(dy / (y2 - y1), 10000000.0f); else tDeltaY = 10000000.0f;
        if (dy > 0) tMaxY = tDeltaY * FRAC1(y1); else tMaxY = tDeltaY * FRAC0(y1);
        voxel.y = (int)y1;

        int dz = (int)math.sign(z2 - z1);
        if (dz != 0) tDeltaZ = math.min(dz / (z2 - z1), 10000000.0f); else tDeltaZ = 10000000.0f;
        if (dz > 0) tMaxZ = tDeltaZ * FRAC1(z1); else tMaxZ = tDeltaZ * FRAC0(z1);
        voxel.z = (int)z1;

        while (true)
        {
            if (tMaxX < tMaxY)
            {
                if (tMaxX < tMaxZ)
                {
                    voxel.x += dx;
                    tMaxX += tDeltaX;
                }
                else
                {
                    voxel.z += dz;
                    tMaxZ += tDeltaZ;
                }
            }
            else
            {
                if (tMaxY < tMaxZ)
                {
                    voxel.y += dy;
                    tMaxY += tDeltaY;
                }
                else
                {
                    voxel.z += dz;
                    tMaxZ += tDeltaZ;
                }
            }
            if (tMaxX > 1 && tMaxY > 1 && tMaxZ > 1) break;
            if (IsInside(voxel))
            {
                PointUtilities.to1D(voxel, Constants.chunkWidth, Constants.chunkWidth, out var indx);
                voxels[indx] = id;
            }
        }
    }
    bool IsInside(int3 position)
    {
        if (position.x >= 0 && position.x < Constants.chunkWidth && position.y >= 0 && position.y < Constants.chunkWidth && position.z >= 0 && position.z < Constants.chunkWidth)
        {
            return true;
        }
        else return false;
    }
}


[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct Line
{
    public float3 start, end;
    public byte id;
}
public struct NativeHighlightMeshData
{
    public NativeList<float4> posVertices;
    public NativeList<int> Indices;
    public NativeArray<int> vertexCount;
    public void Initialize()
    {
        posVertices = new NativeList<float4>(Allocator.Persistent);
        Indices = new NativeList<int>(Allocator.Persistent);
        vertexCount = new NativeArray<int>(1, Allocator.Persistent);
    }

    public void Dispose()
    {
        if (posVertices.IsCreated) posVertices.Dispose();
        if (Indices.IsCreated) Indices.Dispose();
        if (vertexCount.IsCreated) vertexCount.Dispose();
    }

    public void Clear()
    {
        posVertices.Clear();
        Indices.Clear();
        vertexCount[0] = 0;
    }
}
public struct NativeChunkMeshData
{
    public NativeList<PositionVertex> posVertices;
    public NativeList<NormalVertex> normVertices;
    public NativeList<UvVertex> uvVertices;
    public NativeList<int> Indices;
    public NativeArray<int> vertexCount;
    public void Initialize()
    {
        posVertices = new NativeList<PositionVertex>(Allocator.Persistent);
        normVertices = new NativeList<NormalVertex>(Allocator.Persistent);
        uvVertices = new NativeList<UvVertex>(Allocator.Persistent);
        Indices = new NativeList<int>(Allocator.Persistent);
        vertexCount = new NativeArray<int>(1, Allocator.Persistent);
    }

    public void Dispose()
    {
        if (posVertices.IsCreated) posVertices.Dispose();
        if (normVertices.IsCreated) normVertices.Dispose();
        if (uvVertices.IsCreated) uvVertices.Dispose();
        if (Indices.IsCreated) Indices.Dispose();
        if (vertexCount.IsCreated) vertexCount.Dispose();
    }

    public void Clear()
    {
        posVertices.Clear();
        normVertices.Clear();
        uvVertices.Clear();
        Indices.Clear();
        vertexCount[0] = 0;
    }
}
public struct PositionVertex
{
    public UInt16 x;
    public UInt16 y;
    public UInt16 z;
    public UInt16 w;
    public PositionVertex(UInt16 _x, UInt16 _y, UInt16 _z, UInt16 _w)
    {
        x = _x;
        y = _y;
        z = _z;
        w = _w;
    }
    public PositionVertex(float3 pos, int _w = 0)
    {
        x = (UInt16)(pos.x / BurstConstants.chunkWidth * UInt16.MaxValue);
        y = (UInt16)(math.clamp(pos.y, 0, BurstConstants.chunkHeight) / BurstConstants.chunkHeight * UInt16.MaxValue);
        z = (UInt16)(pos.z / BurstConstants.chunkWidth * UInt16.MaxValue);
        w = (UInt16)_w;
    }
    public PositionVertex(float3 pos, float size, int _w = 0)
    {
        x = (UInt16)(pos.x / size * UInt16.MaxValue);
        y = (UInt16)(pos.y / size * UInt16.MaxValue);
        z = (UInt16)(pos.z / size * UInt16.MaxValue);
        w = (UInt16)_w;
    }
}
public struct NormalVertex
{
    public sbyte x;
    public sbyte y;
    public sbyte z;
    public sbyte w;
    public NormalVertex(sbyte _x, sbyte _y, sbyte _z, sbyte _w)
    {
        x = _x;
        y = _y;
        z = _z;
        w = _w;
    }
    public NormalVertex(float3 normal, sbyte _w = 0)
    {
        x = (sbyte)(normal.x * 127);
        y = (sbyte)(normal.y * 127);
        z = (sbyte)(normal.z * 127);
        w = _w;
    }
}
public struct UvVertex
{
    public UInt16 x;
    public UInt16 y;
    public UInt16 z;
    public UInt16 w;
    public UvVertex(UInt16 _x, UInt16 _y, UInt16 _z, UInt16 _w)
    {
        x = _x;
        y = _y;
        z = _z;
        w = _w;
    }
    public UvVertex(float2 uv, UInt16 _z = 0, UInt16 _w = 0)
    {
        x = (UInt16)(uv.x * 65535f);
        y = (UInt16)(uv.y * 65535f);
        z = _z;
        w = _w;
    }
}