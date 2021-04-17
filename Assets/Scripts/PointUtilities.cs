using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.Text;
using Unity.Burst;

[BurstCompile]
public class PointUtilities
{
   


    [BurstCompile]
    public static void to1D(in int3 localPosition, int width, int height, out int result)
    {
        result = (localPosition.z * width * height) + (localPosition.y * width) + localPosition.x;
    }
    [BurstCompile]
    public static void to1D(in int3 localPosition, int width, out int result)
    {
        result = (localPosition.z * width * width) + (localPosition.y * width) + localPosition.x;
    }

    public static string sha256(string randomString)
    {
        var crypt = new System.Security.Cryptography.SHA256Managed();
        var hash = new System.Text.StringBuilder();
        byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));
        foreach (byte theByte in crypto)
        {
            hash.Append(theByte.ToString("x2"));
        }
        return hash.ToString();
    }
}
