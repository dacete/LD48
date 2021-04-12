using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class Constants 
{
    public static int pixelsPerUnit = 64;
    public static readonly int2[] nLookup = new int2[4]
    {
        new int2(1,0),
        new int2(-1,0),
        new int2(0,1),
        new int2(0,-1)
    };
}
