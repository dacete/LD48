using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class DestructionManager : MonoBehaviour
{
    public Texture2D texture;
    public Camera cam;
    Vector3 firstPos;
    DestructablePool destructablePool;
    bool cutting;
    public List<DestructableObject> objects;
    public float bombRadious;
    public float bombPower;
    public LayerMask destructableOnlyLayerMask;
    // Start is called before the first frame update
    void Start()
    {
        objects = new List<DestructableObject>();
        destructablePool = FindObjectOfType<DestructablePool>();
        texture = Instantiate(Resources.Load("Texture")) as Texture2D;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();



            var pos = cam.ScreenToWorldPoint(Input.mousePosition);
            var cols = Physics2D.OverlapCircleAll(pos, bombRadious, destructableOnlyLayerMask);
            if (cols.Length != 0)
            {
                for (int i = 0; i < cols.Length; i++)
                {
                    print("one");
                    var col = cols[i];
                    var dest = col.gameObject.GetComponent<DestructableObject>();
                    var localPos = col.transform.InverseTransformPoint(pos);
                    var bombJob = new BombImageJob()
                    {
                        image = dest.renderer.sprite.texture.GetRawTextureData<Color32>(),
                        bombPos = new float2(localPos.x, localPos.y),
                        pixelsPerUnit = Constants.pixelsPerUnit,
                        size = dest.size,
                        bombSize = bombRadious,
                        bombPower = bombPower
                    };
                    bombJob.Schedule(bombJob.image.Length, 4096).Complete();
                    dest.renderer.sprite.texture.Apply();
                    ProcessDestructable(dest);
                }
            }
            stopwatch.Stop();
            print(stopwatch.Elapsed.TotalMilliseconds);
        }
        //if (Input.GetMouseButtonDown(0))
        //{
        //    firstPos = cam.ScreenToWorldPoint(Input.mousePosition);
        //    cutting = true;
        //}
        //if (cutting && Input.GetMouseButtonUp(0))
        //{
        //    cutting = false;
        //    var endPos = cam.ScreenToWorldPoint(Input.mousePosition);
        //    var dest = destructablePool.Get();
        //    if (dest != null)
        //    {
        //        dest.SetTexture(Instantiate(Resources.Load("Texture") as Texture2D));
        //        endPos = dest.transform.InverseTransformPoint(endPos);
        //        firstPos = dest.transform.InverseTransformPoint(firstPos);
        //        var job = new CutImageJob()
        //        {
        //            image = dest.renderer.sprite.texture.GetRawTextureData<Color32>(),
        //            endPos = new float2(endPos.x, endPos.y),
        //            startPos = new float2(firstPos.x, firstPos.y),
        //            size = dest.size,
        //            pixelsPerUnit = Constants.pixelsPerUnit
        //        };
        //        job.Schedule(job.image.Length, 64).Complete();
        //        var texture = dest.renderer.sprite.texture;
        //        texture.Apply();
        //        dest.SetTexture(texture);
        //        //print(firstPos);
        //        //print(endPos);
        //        destructablePool.Release(dest);
        //    }
        //}

        if (Input.GetKeyDown(KeyCode.R))
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            ProcessDestructable(destructablePool.Get());
            stopwatch.Stop();
            print(stopwatch.Elapsed.TotalMilliseconds);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            var obj = destructablePool.Get();

            var job = new MakeEdgeJob()
            {
                image = obj.renderer.sprite.texture.GetRawTextureData<Color32>(),
                size = obj.size,
                pixelsPerUnit = Constants.pixelsPerUnit,
                points = new NativeList<float2>(Allocator.TempJob),
                points2 = new NativeList<float2>(Allocator.TempJob)
            };
            job.Schedule().Complete();
            obj.SetCollision(job.points2);
            obj.collider.enabled = true;
            stopwatch.Stop();
            print(stopwatch.Elapsed.TotalMilliseconds);
            print(job.points2.Length);
            job.points.Dispose();
            job.points2.Dispose();

        }
    }
    void ProcessDestructable(DestructableObject dest)
    {
        //var stopwatch = new System.Diagnostics.Stopwatch();
        //stopwatch.Start();

        if (destructablePool.texturePools[dest.size].Dequeue(out var texture2))
        {
            var job0 = new ClearImageJob() { image = texture2.GetRawTextureData<Color32>() };
            job0.Schedule(job0.image.Length, 4096).Complete();
            var job = new IslandImageJob()
            {
                image = dest.renderer.sprite.texture.GetRawTextureData<Color32>(),
                image2 = texture2.GetRawTextureData<Color32>(),
                size = dest.size,
                pixelsPerUnit = Constants.pixelsPerUnit,
                positions = new NativeQueue<int2>(Allocator.TempJob)
            };
            job.Schedule().Complete();
            var job2 = new CountPixelsJob() { image = job.image, count = new NativeArray<int>(1, Allocator.TempJob) };
            job2.Schedule().Complete();
            if (job2.count[0] > Constants.minPixels)
            {
                var dest2 = destructablePool.Get();
                dest2.transform.position = dest.transform.position;
                dest2.rigidbody.velocity = dest.rigidbody.velocity;
                dest2.rigidbody.angularVelocity = dest.rigidbody.angularVelocity;
                dest.renderer.sprite.texture.Apply();
                dest2.SetTexture(dest.renderer.sprite.texture);
                ProcessDestructable(dest2);
            }
            texture2.Apply();
            dest.SetTexture(texture2);
            ProcessCollider(dest);
            job2.count.Dispose();
            job.positions.Dispose();
        }



        //stopwatch.Stop();
        //print(stopwatch.Elapsed.TotalMilliseconds);
    }
    void ProcessCollider(DestructableObject dest)
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        var job = new MakeEdgeJob()
        {
            image = dest.renderer.sprite.texture.GetRawTextureData<Color32>(),
            size = dest.size,
            pixelsPerUnit = Constants.pixelsPerUnit,
            points = new NativeList<float2>(Allocator.TempJob),
            points2 = new NativeList<float2>(Allocator.TempJob)
        };
        job.Schedule().Complete();
        dest.SetCollision(job.points2);
        dest.collider.enabled = true;
        stopwatch.Stop();
        print("Collider Time: "+stopwatch.Elapsed.TotalMilliseconds);
        job.points.Dispose();
        job.points2.Dispose();
    }
}
[BurstCompile]
public struct ImageJob : IJobParallelFor
{
    public NativeArray<Color32> image;
    public int size;
    public void Execute(int i)
    {
        int2 localPos = new int2(i % size, i / size);
        var pixel = image[i];
        pixel.r = (byte)localPos.y;
        pixel.a = (byte)localPos.x;
        image[i] = pixel;
    }
}
[BurstCompile]
public struct CutImageJob : IJobParallelFor
{
    public NativeArray<Color32> image;
    public float2 startPos;
    public float2 endPos;
    public int size;
    public int pixelsPerUnit;
    public void Execute(int i)
    {
        int2 localPosInPixels = new int2(i % size, i / size);
        float2 localPos = (float2)localPosInPixels / (float)pixelsPerUnit;
        //var distance = FindDistanceToSegment(localPos, startPos,endPos, out var closest);
        var distance = minimum_distance(startPos, endPos, localPos);
        var pixel = image[i];
        var random = noise.snoise(localPos);
        if (distance < 0.1f + random * 0.05f)
        {
            pixel.a = 0;
        }
        else
        {
            //pixel.a = 255;
        }
        image[i] = pixel;
    }
    float minimum_distance(float2 v, float2 w, float2 p)
    {
        // Return minimum distance between line segment vw and point p
        float l2 = math.lengthsq(v - w);  // i.e. |w-v|^2 -  avoid a sqrt
        if (l2 == 0.0) return math.distance(p, v);   // v == w case
                                                     // Consider the line extending the segment, parameterized as v + t (w - v).
                                                     // We find projection of point p onto the line. 
                                                     // It falls where t = [(p-v) . (w-v)] / |w-v|^2
                                                     // We clamp t from [0,1] to handle points outside the segment vw.
        float t = math.max(0, math.min(1, math.dot(p - v, w - v) / l2));
        float2 projection = v + t * (w - v);  // Projection falls on the segment
        return math.distance(p, projection);
    }
}
[BurstCompile]
public struct BombImageJob : IJobParallelFor
{
    public NativeArray<Color32> image;
    public float2 bombPos;
    public int size;
    public int pixelsPerUnit;
    public float bombPower;
    public float bombSize;
    public void Execute(int i)
    {
        int2 localPosInPixels = new int2(i % size, i / size);
        float2 localPos = (float2)localPosInPixels / (float)pixelsPerUnit;
        //var distance = FindDistanceToSegment(localPos, startPos,endPos, out var closest);
        var distance = math.distance(bombPos, localPos);
        var pixel = image[i];
        var random = noise.snoise(localPos);
        if (distance < bombSize)
        {
            pixel.a = 0;
        }
        else
        {
            //pixel.a = 255;
        }
        image[i] = pixel;
    }
}

[BurstCompile]
public struct MakeEdgeJob : IJob
{
    public NativeArray<Color32> image;
    public int size;
    public int pixelsPerUnit;
    public NativeList<float2> points;
    public NativeList<float2> points2;
    int2 startPos;
    int count;
    public void Execute()
    {
        startPos = new int2(-10, 10);
        points.Clear();
        count = 0;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                int2 localPos = new int2(x, y);
                int index = x + y * size;
                var dir = GetEdgeDirection(localPos);
                if (!dir.Equals(new int2(0)))
                {
                    startPos = localPos;
                    points.Add((float2)localPos / (float)pixelsPerUnit);
                    count++;
                    RecursiveWalk(localPos + dir);
                    goto after;
                }
            }
        }
        after:
        for (int i = 0; i < points.Length; i++)
        {
            var point = points[i];
            var point2 = points[Constants.mod(i + 1, points.Length)];
            var point0 = points[Constants.mod(i - 1, points.Length)];
            var dot = math.dot(math.normalize(point0 - point), math.normalize(point - point2));
            if (dot < 0.5f)
            {
                points2.Add(point);
            }
        }
        return;
    }
    void RecursiveWalk(int2 position)
    {
        if (position.Equals(startPos))
        {
            return;
        }
        var dir = GetEdgeDirection(position);
        points.Add((float2)position / (float)pixelsPerUnit);
        count++;
        RecursiveWalk(position + dir);
    }
    int2 GetEdgeDirection(int2 position)
    {
        var a0 = GetAlpha(position + Constants.edgeLookup[0]);
        var a1 = GetAlpha(position + Constants.edgeLookup[1]);
        var a2 = GetAlpha(position + Constants.edgeLookup[2]);
        var a3 = GetAlpha(position + Constants.edgeLookup[3]);
        var lookup = a0 + (a1 << 1) + (a2 << 2) + (a3 << 3);
        var direction = Constants.edgeDirections[lookup];
        return direction;
    }
    int GetAlpha(int2 position)
    {
        if (IsInside(position))
        {
            return image[position.x + position.y * size].a > 0 ? 1 : 0;
        }
        else
        {
            return 0;
        }
    }
    bool IsInside(int2 position)
    {
        if (position.x >= 0 && position.x < size && position.y >= 0 && position.y < size)
        {
            return true;
        }
        else return false;
    }
}
[BurstCompile]
public struct CountPixelsJob : IJob
{
    public NativeArray<Color32> image;
    public NativeArray<int> count;
    public void Execute()
    {
        for (int i = 0; i < image.Length; i++)
        {
            if (image[i].a > 0) { count[0]++; }
        }
    }
}
[BurstCompile]
public struct TestJob : IJob
{
    public NativeArray<NativeList<float2>> array;
    public void Execute()
    {

    }
}
[BurstCompile]
public struct ClearImageJob : IJobParallelFor
{
    public NativeArray<Color32> image;
    public void Execute(int index)
    {
        image[index] = new Color32() { a = 0 };
    }
}

[BurstCompile]
public struct IslandImageJob : IJob
{
    public NativeArray<Color32> image;
    public NativeArray<Color32> image2;
    public int size;
    public int pixelsPerUnit;
    public NativeQueue<int2> positions;
    public void Execute()
    {
        //positions = new NativeQueue<int2>(Allocator.Temp);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int2 localPos = new int2(x, y);
                int index = x + y * size;
                var pixel = image[index];
                if (pixel.a > 0)
                {
                    Enqueue(localPos);
                    goto after;
                }
            }
        }
        after:
        while (positions.Count > 0)
        {
            var pos = positions.Dequeue();
            var pixel = image[GetIndex(pos)];
            if (pixel.a > 0)
            {
                pixel.a = 0;
                image[GetIndex(pos)] = pixel;
                pixel.a = 255;
                image2[GetIndex(pos)] = pixel;
                for (int p = 0; p < Constants.nLookup.Length; p++)
                {
                    Enqueue(pos + Constants.nLookup[p]);
                }
            }
        }
        return;

    }
    void Enqueue(int2 position)
    {
        if (IsInside(position))
        {
            positions.Enqueue(position);
        }
    }
    void CheckRecursive(int2 position)
    {
        var pixel = image[GetIndex(position)];
        if (pixel.a > 0)
        {
            pixel.a = 0;
            image[GetIndex(position)] = pixel;
            pixel.a = 255;
            image2[GetIndex(position)] = pixel;
            for (int p = 0; p < Constants.nLookup.Length; p++)
            {
                var offset = Constants.nLookup[p];
                if (IsInside(offset + position))
                {
                    CheckRecursive(offset + position);
                }
            }
        }
    }
    int GetIndex(int2 position)
    {
        return position.x + position.y * size;
    }
    bool IsInside(int2 position)
    {
        if (position.x >= 0 && position.x < size && position.y >= 0 && position.y < size)
        {
            return true;
        }
        else return false;
    }
}
