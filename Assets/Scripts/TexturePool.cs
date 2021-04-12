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
    LockFreeQueue<Texture2D> queue;
    int count;
    public TexturePool(int size, int idleCount, int maxPerFrame)
    {
        this.size = size;
        this.idleCount = idleCount;
        this.maxPerFrame = maxPerFrame;
        queue = new LockFreeQueue<Texture2D>();
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
    Texture2D NewTexture()
    {
        var text = new Texture2D(size, size, TextureFormat.RGBA32, false);
        return text;
    }
    public void Enqueue(Texture2D texture)
    {
        count++;
        queue.Enqueue(texture);
    }public bool Dequeue(out Texture2D texture)
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
