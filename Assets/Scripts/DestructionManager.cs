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
    public ObjectPool<DestructableObject> pool;
    Vector3 firstPos;
    bool cutting;
    // Start is called before the first frame update
    void Start()
    {
        pool = FindObjectOfType<DestructablePool>().pool;
        texture = Instantiate(Resources.Load("Texture")) as Texture2D;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            firstPos = cam.ScreenToWorldPoint(Input.mousePosition);
            cutting = true;
        }
        if (cutting && Input.GetMouseButtonUp(0))
        {
            cutting = false;
            var endPos = cam.ScreenToWorldPoint(Input.mousePosition);
            var dest = FindObjectOfType<DestructableObject>();
            endPos = dest.transform.InverseTransformPoint(endPos);
            firstPos = dest.transform.InverseTransformPoint(firstPos);
            var counter1 = new NativeArray<int>(1, Allocator.TempJob);
            var counter2 = new NativeArray<int>(1, Allocator.TempJob);
            var job = new CutImageJob()
            {
                image = dest.renderer.sprite.texture.GetRawTextureData<Color32>(),
                endPos = new float2(endPos.x, endPos.y),
                startPos = new float2(firstPos.x, firstPos.y),
                size = dest.size,
                pixelsPerUnit = Constants.pixelsPerUnit
            };
            var texture2 = new Texture2D(dest.size, dest.size, TextureFormat.RGBA32, false, true);
            var job2 = new IslandImageJob()
            {
                image = job.image,
                image2 = texture2.GetRawTextureData<Color32>(),
                pixelsPerUnit = Constants.pixelsPerUnit,
                size = dest.size,
                positions = new NativeQueue<int2>(Allocator.TempJob)
            };
            var job1 = new ClearImageJob() { image = texture2.GetRawTextureData<Color32>() };
            var job3 = new CountPixelsJob() { count = counter1, image = job2.image };
            var job4 = new CountPixelsJob() { count = counter2, image = job2.image2 };
            job4.Schedule(job3.Schedule(job2.Schedule(job1.Schedule(job.Schedule(job.image.Length, 64))))).Complete();
            var texture = dest.renderer.sprite.texture;
            texture.Apply();
            texture2.Apply();
            dest.SetTexture(texture);

            var dest2 = pool.Get();
            dest2.SetTexture(texture2);
            dest2.transform.position = dest.transform.position;

            job2.positions.Dispose();
            print(firstPos);
            print(endPos);
            print(counter1[0]);
            print(counter2[0]);
            counter1.Dispose();
            counter2.Dispose();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            var text = texture;
            var data = text.GetRawTextureData<Color32>();
            var job = new ImageJob() { image = data, size = 256 };
            job.Schedule(data.Length, 64).Complete();
            text.Apply();
            stopwatch.Stop();
            print(stopwatch.Elapsed.TotalMilliseconds);

        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            var obj = pool.Get();
            print(obj);
            print(texture);
            obj.SetTexture(texture);
            obj.transform.position = new Vector3(0, 3);
            stopwatch.Stop();
            print(stopwatch.Elapsed.TotalMilliseconds);

        }
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
        if (distance < 0.05f)
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
public struct ClearImageJob : IJob
{
    public NativeArray<Color32> image;
    public void Execute()
    {
        for (int i = 0; i < image.Length; i++)
        {
            image[i] = new Color32() { a = 0 };
        }
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