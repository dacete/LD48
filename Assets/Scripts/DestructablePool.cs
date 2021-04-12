using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;

public class DestructablePool : MonoBehaviour
{
    public ObjectPool<DestructableObject> pool;
    Queue<DestructableObject> freeObjects= new Queue<DestructableObject>();
    public int chunkCount = 50;
    public int extraCount = 50;
    public int spawnPerFrame = 1;
    bool taken;
    NativeArray<Color32> texArray;
    public LockFreeQueue<SpawnNewObject> spawnNewDestroyable;
    public Dictionary<int, TexturePool> texturePools;
    // Start is called before the first frame update
    void Awake()
    {
        texturePools = new Dictionary<int, TexturePool>();
        spawnNewDestroyable = new LockFreeQueue<SpawnNewObject>();
        texArray = new NativeArray<Color32>(chunkCount, Allocator.Persistent);
        pool = new ObjectPool<DestructableObject>(SpawnNewObject);
        texturePools.Add(256, new TexturePool(256, 50, 1));
    }

    private void Update()
    {
        if(freeObjects.Count < chunkCount + extraCount)
        {
            for (int i = 0; i < spawnPerFrame; i++)
            {
                var dest = SpawnNewObject();
                freeObjects.Enqueue(dest);
            }
        }
        foreach(int key in texturePools.Keys)
        {
            texturePools[key].Update();
        }
    }
    public DestructableObject Get()
    {
        if (freeObjects.Count == 0)
        {
            Debug.Log("NONO");
            return SpawnNewObject();
        }
        return freeObjects.Dequeue();
    }
    public void Release(DestructableObject dest)
    {
        freeObjects.Enqueue(dest);
    }
    DestructableObject SpawnNewObject()
    {
        GameObject obj = Instantiate(Resources.Load("DestructablePrefab")) as GameObject;
        obj.transform.parent = transform;
        //obj.GetComponent<DestructableObject>().SetTexture(Instantiate(Resources.Load("Texture"))as Texture2D);
        return obj.GetComponent<DestructableObject>();
    }

    NativeArray<Color32> GetTextureArray()
    {
        return texArray;
    }
}
public struct SpawnNewObject
{
    public Vector3 position;
    public Quaternion rotation;
    public Texture2D texture;
    public Vector2[] points;
    
}
