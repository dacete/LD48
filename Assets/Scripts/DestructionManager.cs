using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
[BurstCompile]
public unsafe class DestructionManager : MonoBehaviour
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
    bool doWork = true;

    // Start is called before the first frame update
    void Start()
    {
        Texture.allowThreadedTextureCreation = true;
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
                    //ProcessDestructable(dest);
                }
            }
            stopwatch.Stop();
            print(stopwatch.Elapsed.TotalMilliseconds);
        }
        if (Input.GetMouseButtonDown(1))
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
                    var shatterJob = new ShatterImageJob()
                    {
                        image = dest.renderer.sprite.texture.GetRawTextureData<Color32>(),
                        pos = new float2(localPos.x, localPos.y),
                        pixelsPerUnit = Constants.pixelsPerUnit,
                        size = dest.size,
                        seed = (uint)UnityEngine.Random.Range(1, 10000)
                    };
                    shatterJob.Schedule().Complete();
                    dest.renderer.sprite.texture.Apply();
                    ProcessDestructable(dest);
                    //ProcessDestructable(dest);
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

        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    var stopwatch = new System.Diagnostics.Stopwatch();
        //    stopwatch.Start();
        //    ProcessDestructable(destructablePool.Get());
        //    stopwatch.Stop();
        //    print(stopwatch.Elapsed.TotalMilliseconds);
        //}
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    var stopwatch = new System.Diagnostics.Stopwatch();
        //    stopwatch.Start();
        //    var obj = destructablePool.Get();

        //    var job = new MakeEdgeJob()
        //    {
        //        image = obj.renderer.sprite.texture.GetRawTextureData<Color32>(),
        //        size = obj.size,
        //        pixelsPerUnit = Constants.pixelsPerUnit,
        //        points = new NativeList<float2>(Allocator.TempJob),
        //        points2 = new NativeList<float2>(Allocator.TempJob)
        //    };
        //    job.Schedule().Complete();
        //    obj.SetCollision(job.points2);
        //    obj.collider.enabled = true;
        //    stopwatch.Stop();
        //    print(stopwatch.Elapsed.TotalMilliseconds);
        //    print(job.points2.Length);
        //    job.points.Dispose();
        //    job.points2.Dispose();

        //}
    }
    void ProcessDestructable(DestructableObject dest)
    {
        if (destructablePool.texturePools[dest.size].Dequeue(out var texture2))
        {
            var job = new IslandPrepImageJob()
            {
                image = dest.renderer.sprite.texture.GetRawTextureData<Color32>(),
                image2 = texture2.image,
                size = dest.size,
                islands = new NativeArray<byte>(dest.size * dest.size + 1, Allocator.Persistent),
                positions = new NativeQueue<int2>(Allocator.TempJob)
            };
            job.Schedule().Complete();
            job.positions.Dispose();
            var count = job.islands[dest.size * dest.size];
            print(count);
            var job2 = new IslandPostImageJob() { size = dest.size, original = job.image2, islands = job.islands, images = new NativeArray<IntPtr>(count, Allocator.Persistent) };
            var textures = destructablePool.GetUnsafeTextureArray(dest.size, count);
            for (int i = 0; i < count; i++)
            {
                job2.images[i] = new IntPtr(textures[i].image.GetUnsafePtr());
            }
            job2.Schedule(count, 1).Complete();
            var pos = dest.transform.position;
            var rot = dest.transform.rotation;
            dest.collider.enabled = false;
            dest.renderer.enabled = false;
            destructablePool.Release(dest);
            MakeEdgeJob[] edgeJobs = new MakeEdgeJob[count];
            JobHandle[] handles = new JobHandle[count];
            for (int i = 0; i < count; i++)
            {
                edgeJobs[i] = new MakeEdgeJob()
                {
                    size = dest.size,
                    image = textures[i].image,
                    pixelsPerUnit = Constants.pixelsPerUnit,
                    points = new NativeList<float2>(Allocator.TempJob),
                    points2 = new NativeList<float2>(Allocator.TempJob)
                };
                handles[i] = edgeJobs[i].Schedule();
            }
            JobHandle.ScheduleBatchedJobs();
            for (int i = 0; i < count; i++)
            {
                handles[i].Complete();
            }
            for (int i = 0; i < count; i++)
            {
                destructablePool.Get(out var d);
                d.collider.enabled = true;
                textures[i].texture.Apply();
                d.transform.position = pos;
                d.transform.rotation = rot;
                d.SetTexture(textures[i].texture);
                d.SetCollision(edgeJobs[i].points2);
                edgeJobs[i].points.Dispose();
                edgeJobs[i].points2.Dispose();
            }
        }
    }
    //void ProcessDestructable(DestructableObject dest)
    //{
    //    //var stopwatch = new System.Diagnostics.Stopwatch();
    //    //stopwatch.Start();

    //    if (destructablePool.texturePools[dest.size].Dequeue(out var texture2))
    //    {
    //        var job0 = new ClearImageJob() { image = texture2.GetRawTextureData<Color32>() };
    //        job0.Schedule(job0.image.Length, 4096).Complete();
    //        var job = new IslandImageJob()
    //        {
    //            image = dest.renderer.sprite.texture.GetRawTextureData<Color32>(),
    //            image2 = texture2.GetRawTextureData<Color32>(),
    //            size = dest.size,
    //            pixelsPerUnit = Constants.pixelsPerUnit,
    //            positions = new NativeQueue<int2>(Allocator.TempJob)
    //        };
    //        job.Schedule().Complete();
    //        var job2 = new CountPixelsJob() { image = job.image, count = new NativeArray<int>(1, Allocator.TempJob) };
    //        job2.Schedule().Complete();
    //        if (job2.count[0] > Constants.minPixels)
    //        {
    //            var dest2 = destructablePool.Get();
    //            dest2.transform.position = dest.transform.position;
    //            dest2.rigidbody.velocity = dest.rigidbody.velocity;
    //            dest2.rigidbody.angularVelocity = dest.rigidbody.angularVelocity;
    //            dest.renderer.sprite.texture.Apply();
    //            dest2.SetTexture(dest.renderer.sprite.texture);
    //            ProcessDestructable(dest2);
    //        }
    //        texture2.Apply();
    //        dest.SetTexture(texture2);
    //        ProcessCollider(dest);
    //        job2.count.Dispose();
    //        job.positions.Dispose();
    //    }



    //    //stopwatch.Stop();
    //    //print(stopwatch.Elapsed.TotalMilliseconds);
    //}
    //void ProcessCollider(DestructableObject dest)
    //{
    //    var stopwatch = new System.Diagnostics.Stopwatch();
    //    stopwatch.Start();
    //    var job = new MakeEdgeJob()
    //    {
    //        image = dest.renderer.sprite.texture.GetRawTextureData<Color32>(),
    //        size = dest.size,
    //        pixelsPerUnit = Constants.pixelsPerUnit,
    //        points = new NativeList<float2>(Allocator.TempJob),
    //        points2 = new NativeList<float2>(Allocator.TempJob)
    //    };
    //    job.Schedule().Complete();
    //    dest.SetCollision(job.points2);
    //    dest.collider.enabled = true;
    //    stopwatch.Stop();
    //    print("Collider Time: " + stopwatch.Elapsed.TotalMilliseconds);
    //    job.points.Dispose();
    //    job.points2.Dispose();
    //}
    private void OnApplicationQuit()
    {
        doWork = false;
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
public struct ShatterImageJob : IJob
{
    public NativeArray<Color32> image;
    public float2 pos;
    public int size;
    public uint seed;
    public int pixelsPerUnit;
    Unity.Mathematics.Random random;
    int count;
    public void Execute()
    {
        var localPos = (int2)(pos * pixelsPerUnit);
        random = Unity.Mathematics.Random.CreateFromIndex(seed);
        Recursive(localPos, random.NextFloat2Direction());
        //Recursive(localPos, random.NextFloat2Direction());
    }
    void Recursive(int2 startPos, float2 direction)
    {
        if (count > 100)
        {
            return;
        }
        count++;
        var end = Cut(startPos, direction, pixelsPerUnit *random.NextFloat(0.1f,0.4f));
        if (IsInside(end))
        {
            var cnt = random.NextInt(1, 4);
            for (int i = 0; i < cnt; i++)
            {
                Recursive(end, RotatePoint(0, 0, math.radians((cnt-2)* 30f + random.NextFloat(-25f, 30f)), direction));
            }
        }
    }
    int Frac0(float x)
    {
        return (int)(x - math.floor(x));
    }
    int Frac1(float x)
    {
        return (int)(1 - x + math.floor(x));
    }
    int2 Cut(float2 voxel, float2 direction, float length)
    {
        float2 originalPos = voxel;
        float tMaxX, tMaxY, tDeltaX, tDeltaY;
        float x1, y1; // start point   
        float x2, y2; // end point   
        x1 = voxel.x;
        y1 = voxel.y;
        x2 = (voxel + direction).x;
        y2 = (voxel + direction).y;
        int dx = (int)math.sign(x2 - x1);
        if (dx != 0) tDeltaX = math.min(dx / (x2 - x1), 10000000.0f); else tDeltaX = 10000000.0f;
        if (dx > 0) tMaxX = tDeltaX * Frac1(x1); else tMaxX = tDeltaX * Frac0(x1);
        voxel.x = (int)x1;

        int dy = (int)math.sign(y2 - y1);
        if (dy != 0) tDeltaY = math.min(dy / (y2 - y1), 10000000.0f); else tDeltaY = 10000000.0f;
        if (dy > 0) tMaxY = tDeltaY * Frac1(y1); else tMaxY = tDeltaY * Frac0(y1);
        voxel.y = (int)y1;
        while (true)
        {
            if (tMaxX < tMaxY)
            {
                tMaxX = tMaxX + tDeltaX;
                voxel.x += dx;
            }
            else
            {
                tMaxY = tMaxY + tDeltaY;
                voxel.y += dy;
            }
            float distance = math.distance(voxel, originalPos);
            if (distance > length)
            {
                break;
            }
            if (IsInside((int2)voxel))
            {
                WriteEmptyVoxel((int2)voxel);
            }
            else
            {
                break;
            }
            // process voxel here
        }
        return (int2)voxel;
    }
    void WriteEmptyVoxel(int2 pos)
    {
        for (int x = math.max(0, pos.x - 1); x < math.min(size, pos.x + 2); x++)
        {
            for (int y = math.max(0, pos.y - 1); y < math.min(size, pos.y + 2); y++)
            {
                image[x + y * size] = new Color32() { a = 0 };
            }
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
    float2 RotatePoint(float cx, float cy, float angle, float2 p)
    {
        float s = math.sin(angle);
        float c = math.cos(angle);

        // translate point back to origin:
        p.x -= cx;
        p.y -= cy;

        // rotate point
        float xnew = p.x * c - p.y * s;
        float ynew = p.x * s + p.y * c;

        // translate point back:
        p.x = xnew + cx;
        p.y = ynew + cy;
        return p;
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
    int2 nextPos;
    int2 prevPos;
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
                    prevPos = localPos;
                    nextPos = localPos + dir;
                    goto after;
                }
            }
        }
        after:
        int counter = 0;
        while (true)
        {
            if (nextPos.Equals(startPos))
            {
                break;
            }
            var dir = GetEdgeDirection(nextPos);
            if (dir.Equals(new int2(0)))
            {
                Debug.LogError("Yikes");
                return;
            }
            if (counter % 5 == 0)
            {
                points.Add((float2)nextPos / (float)pixelsPerUnit);
            }
            counter++;
            count++;
            prevPos = nextPos;
            nextPos = nextPos + dir;
        }


        for (int i = 0; i < points.Length; i += 1)
        {
            var point = points[i];
            var point2 = points[Constants.mod(i + 1, points.Length)];
            var point0 = points[Constants.mod(i - 1, points.Length)];
            var dot = math.dot(math.normalize(point0 - point), math.normalize(point - point2));
            if (dot < 0.95f)
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
        int2 direction;
        if (a0 == 0 && a3 == 0 && a1 == 1 && a2 == 1)
        {
            if (position.Equals(prevPos))
            {
                Debug.LogError("NO");
                return new int2();
            }
            var delta = position - prevPos;
            direction.y = -delta.x;
            direction.x = -delta.y;
            return direction;
        }
        else if (a0 == 1 && a3 == 1 && a1 == 0 && a2 == 0)
        {
            if (position.Equals(prevPos))
            {
                Debug.LogError("NO");
                return new int2();
            }
            var delta = position - prevPos;
            direction.y = delta.x;
            direction.x = delta.y;
            return direction;
        }
        else
        {
            var lookup = a0 + (a1 << 1) + (a2 << 2) + (a3 << 3);
            direction = Constants.edgeDirections[lookup];
            return direction;
        }
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

[BurstCompile]
public struct IslandPrepImageJob : IJob
{
    public NativeArray<Color32> image;
    public NativeArray<Color32> image2;
    public NativeArray<byte> islands;
    public int size;
    byte counter;
    int islandSizeCounter;
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
                    CalculateIsland(localPos);
                }
            }
        }
        islands[size * size] = counter;
    }
    void CalculateIsland(int2 position)
    {
        islandSizeCounter = 0;
        Enqueue(position);
        while (positions.Count > 0)
        {
            var pos = positions.Dequeue();
            var index = GetIndex(pos);
            var pixel = image[index];
            if (pixel.a > 0)
            {
                pixel.a = 0;
                image[index] = pixel;
                pixel.a = 255;
                image2[index] = pixel;
                islands[index] = counter;
                for (int p = 0; p < Constants.nLookup.Length; p++)
                {
                    Enqueue(pos + Constants.nLookup[p]);
                }
            }
        }
        if (islandSizeCounter < 20)
        {
            DeleteIsland(position);
        }
        counter++;
    }
    void DeleteIsland(int2 position)
    {
        Enqueue(position);
        while (positions.Count > 0)
        {
            var pos = positions.Dequeue();
            var index = GetIndex(pos);
            var pixel = image[index];
            if (pixel.a > 0)
            {
                pixel.a = 0;
                image2[index] = pixel;
                islands[index] = 0;
                for (int p = 0; p < Constants.nLookup.Length; p++)
                {
                    Enqueue(pos + Constants.nLookup[p]);
                }
            }
        }
    }
    void Enqueue(int2 position)
    {
        islandSizeCounter++;
        if (IsInside(position))
        {
            positions.Enqueue(position);
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
[BurstCompile]
public unsafe struct IslandPostImageJob : IJobParallelFor
{
    public NativeArray<IntPtr> images;
    [ReadOnly]
    public NativeArray<Color32> original;
    [ReadOnly]
    public NativeArray<byte> islands;
    public int size;
    public void Execute(int index)
    {
        var p = (Color32*)images[index].ToPointer();
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var offset = x + y * size;
                if (islands[offset] == index)
                {
                    *(p + offset) = original[offset];
                }
            }
        }
    }
}

