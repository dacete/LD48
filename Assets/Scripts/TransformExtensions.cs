using UnityEngine;

public static class TransformExtensions
{
    public static Vector3 TransformPointUnscaled(Vector3 tPos, Quaternion tRot, Vector3 position)
    {
        return tPos + tRot * position;
    }

    public static Vector3 InverseTransformPointUnscaled(Vector3 tPos, Quaternion tRot, Vector3 position)
    {
        position -= tPos;
        return Quaternion.Inverse(tRot) * position;
    }
}