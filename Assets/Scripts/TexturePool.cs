using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class TexturePool
{
    int size;
    int idleCount;
    int maxPerFrame;
    LockFreeQueue<UnsafeTexture> queue;
    int count;
    public TexturePool(int size, int idleCount, int maxPerFrame)
    {
        this.size = size;
        this.idleCount = idleCount;
        this.maxPerFrame = maxPerFrame;
        queue = new LockFreeQueue<UnsafeTexture>();
    }
    public void Update()
    {
        if(count < idleCount)
        {
            for (int i = 0; i < maxPerFrame; i++)
            {
                Enqueue(NewTexture());
            }
        }
    }
    UnsafeTexture NewTexture()
    {
        var text = new Texture2D(size, size, TextureFormat.RGBA32, false);
        text.filterMode = FilterMode.Point;
        var container = new UnsafeTexture();
        container.texture = text;
        container.image = text.GetRawTextureData<Color32>();
        new ClearImageJob() { image = text.GetRawTextureData<Color32>() }.Schedule(size*size, size).Complete();
        return container;
    }
    public void Enqueue(UnsafeTexture texture)
    {
        count++;
        queue.Enqueue(texture);
    }public bool Dequeue(out UnsafeTexture texture)
    {
        var bo = queue.Dequeue(out var outText);
        if (bo)
        {
            count--;
        }
        texture = outText;
        return bo;
    }

}
public struct UnsafeTexture
{
    public Texture2D texture;
    public NativeArray<Color32> image;
}