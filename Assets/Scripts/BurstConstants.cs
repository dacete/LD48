using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Rendering;
public static class BurstConstants
{
    #region statics
    public static readonly int leafWidth = 16;
    public static readonly int cubedLeafSize = leafWidth * leafWidth * leafWidth;
    public static readonly int chunkWidth = 32;
    public static readonly int chunkHeight = 32;
    public static readonly int leavesPerVertical = chunkHeight / leafWidth;
    public static readonly int leavesPerHorizontal = chunkWidth / leafWidth;
    public static readonly int leavesPerChunk = (chunkWidth / leafWidth) * (chunkWidth / leafWidth) * (chunkHeight / leafWidth);
    public static readonly int squaredChunkSize = chunkWidth * chunkWidth;
    public static readonly int squaredChunkVerticalSize = chunkWidth * chunkHeight;
    public static readonly int cubedChunkSize = chunkWidth * chunkWidth * chunkHeight;
    public static readonly float3 sunColor = new float3(1, 1, 1);
    public static readonly float3 up = new float3(0, 1, 0);
    public static readonly float3 forward = new float3(0, 0, 1);
    public static readonly float3 right = new float3(1, 0, 0);
    public static readonly int3[] voxelVerts = new int3[8] {

        new int3(0, 0, 0),//[
        new int3(1, 0, 0),//1//[
        new int3(1, 1, 0),//[
        new int3(0, 1, 0),//3//[
        new int3(0, 0, 1),
        new int3(1, 0, 1),//5
        new int3(1, 1, 1),
        new int3(0, 1, 1),//7

    }; public static readonly float3[] waterVerts = new float3[8] {

        new float3(0, 0, 0),//[
        new float3(1f, 0, 0),//1//[
        new float3(1f, 0.7f, 0),//[
        new float3(0, 0.7f, 0),//3//[
        new float3(0, 0,1f),
        new float3(1f, 0, 1f),//5
        new float3(1f, 0.7f, 1f),
        new float3(0, 0.7f, 1f),//7

    };


    public static readonly int3[] pLookup = new int3[6] {

        new int3(0, 0, -1),
        new int3(0, 0, 1),
        new int3(0, 1, 0),
        new int3(0, -1, 0),
        new int3(-1, 0, 0),
        new int3(1, 0, 0)

    }; public static readonly int2[] pLookup2D = new int2[4] {

        new int2(0,  -1),
        new int2(0,  1),
        new int2(-1,  0),
        new int2(1,  0)

    }; public static readonly int3[] AOValues = new int3[26]{
        new int3(-1,-1,-1),//0
        new int3(-1,-1,0),
        new int3(-1,-1,1),
        new int3(-1,0,-1),
        new int3(-1,0,0),
        new int3(-1,0,1),///5
        new int3(-1,1,-1),
        new int3(-1,1,0),
        new int3(-1,1,1),
        new int3(0,-1,-1),
        new int3(0,-1,0),//10
        new int3(0,-1,1),
        new int3(0,0,-1),
        new int3(0,0,1),
        new int3(0,1,-1),
        new int3(0,1,0),//15
        new int3(0,1,1),
        new int3(1,-1,-1),
        new int3(1,-1,0),
        new int3(1,-1,1),
        new int3(1,0,-1),//20
        new int3(1,0,0),
        new int3(1,0,1),
        new int3(1,1,-1),
        new int3(1,1,0),//25
        new int3(1,1,1)
    }; public static readonly int3x4[] AOLookup = new int3x4[6] {
        new int3x4(new int3(3,0,9),new int3(3,6,14),new int3(9,17,20),new int3(14,23,20)),
        new int3x4(new int3(11,19,22),new int3(16,25,22),new int3(5,2,11),new int3(5,8,16)),
        new int3x4(new int3(7,6,14),new int3(7,8,16),new int3(14,23,24),new int3(16,25,24)),
        new int3x4(new int3(9,17,18),new int3(11,19,18),new int3(1,0,9),new int3(1,2,11)),
        new int3x4(new int3(1,2,5),new int3(5,8,7),new int3(1,0,3),new int3(3,6,7)),
        new int3x4(new int3(18,17,20),new int3(20,23,24),new int3(18,19,22),new int3(22,25,24))
    };

    public static readonly int4[] voxelTris = new int4[6] {

        new int4(0, 3, 1, 2), // Front Face    0
		new int4(5, 6, 4, 7), // back Face   1
		new int4(3, 7, 2, 6), // Top Face     2
		new int4(1, 5, 0, 4), // Bottom Face  3
		new int4(4, 7, 0, 3), // Left Face    4
		new int4(1, 2, 5, 6) // Right Face    5

	}; public static readonly float2[] voxelUvs = new float2[4] {
        new float2 (0.0f, 0.0f),
        new float2 (0.0f, 1.0f),
        new float2 (1.0f, 0.0f),
        new float2 (1.0f, 1.0f)
    };
    public static readonly MeshUpdateFlags chunkMeshUpdateFlags = MeshUpdateFlags.DontNotifyMeshUsers
                                                          | MeshUpdateFlags.DontValidateIndices
                                                          | MeshUpdateFlags.DontRecalculateBounds
                                                          | MeshUpdateFlags.DontResetBoneBounds;
    public static readonly VertexAttributeDescriptor[] chunkMeshLayout = {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.UNorm16, 4, stream:0),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.SNorm8, 4, stream:1),
        };
    #endregion



}
