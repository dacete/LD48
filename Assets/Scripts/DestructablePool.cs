using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;

public class DestructablePool : MonoBehaviour
{
    public LockFreeQueue<DestructableObject> objectPool= new LockFreeQueue<DestructableObject>();
    public int objectDesiredCount = 50;
    public int objectCount;
    public int spawnPerFrame = 1;
    bool taken;
    public Dictionary<int, TexturePool> texturePools;
    // Start is called before the first frame update
    void Awake()
    {
        objectCount = 0;
        texturePools = new Dictionary<int, TexturePool>();
        
        texturePools.Add(256, new TexturePool(256, 50, 1));
        texturePools.Add(128, new TexturePool(128, 50, 1));
        texturePools.Add(64, new TexturePool(64, 50, 1));
        texturePools.Add(32, new TexturePool(32, 50, 1));
        texturePools.Add(16, new TexturePool(16, 50, 1));
    }
    public UnsafeTexture[] GetUnsafeTextureArray(int size, int count)
    {
        var array = new UnsafeTexture[count];
        for (int i = 0; i < count; i++)
        {
            texturePools[size].Dequeue(out var texture);
            array[i] = texture;
        }
        return array;
    }
    private void Update()
    {
        if(objectCount < objectDesiredCount)
        {
            for (int i = 0; i < spawnPerFrame; i++)
            {
                var dest = SpawnNewObject();
                objectPool.Enqueue(dest);
                objectCount++;
            }
        }
        foreach(int key in texturePools.Keys)
        {
            texturePools[key].Update();
        }
    }
    public bool Get(out DestructableObject obj)
    {
        if (objectCount == 0)
        {
            obj = null;
            return false;
        }
        else
        {
            var bul = objectPool.Dequeue(out obj);
            objectCount--;
            return bul;
        }
    }
    DestructableObject SpawnNewObject()
    {
        GameObject obj = Instantiate(Resources.Load("DestructablePrefab")) as GameObject;
        obj.transform.parent = transform;
        //obj.GetComponent<DestructableObject>().SetTexture(Instantiate(Resources.Load("Texture"))as Texture2D);
        return obj.GetComponent<DestructableObject>();
    }
}
public struct DestructableStruct
{
    public Vector3 position;
    public Quaternion rotation;
    public NativeArray<Color32> image;
    public int size;
    public Vector2[] points;
}
