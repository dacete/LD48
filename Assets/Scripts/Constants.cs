using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class Constants
{
    public static int pixelsPerUnit = 64;
    public static int minPixels = 15;
    public static readonly int2[] nLookup = new int2[4]
    {
        new int2(1,0),
        new int2(-1,0),
        new int2(0,1),
        new int2(0,-1)
    }; public static readonly int2[] edgeLookup = new int2[4]
 {
        new int2(-1,0),
        new int2(0,0),
        new int2(-1,-1),
        new int2(0,-1)
 };

    public static readonly int2[] edgeDirections = new int2[16] {
        new int2(0,0),
        new int2(0,1),
        new int2(1,0),//2
        new int2(1,0),
        new int2(-1,0),//4
        new int2(0,1),
        new int2(0,0),
        new int2(1,0),
        new int2(0,-1),//8
        new int2(0,0),
        new int2(0,-1),
        new int2(0,-1),
        new int2(-1,0),//12
        new int2(0,1),
        new int2(-1,0),
        new int2(0,0)//15
    }; public static int mod(int x, int m)
    {
        return ((x % m + m) % m);
    }
}
