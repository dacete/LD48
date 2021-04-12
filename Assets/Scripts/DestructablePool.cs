using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class DestructablePool : MonoBehaviour
{
    public ObjectPool<DestructableObject> pool;
    // Start is called before the first frame update
    void Awake()
    {
        pool = new ObjectPool<DestructableObject>(SpawnNewObject);
    }


    DestructableObject SpawnNewObject()
    {
        GameObject obj = Instantiate(Resources.Load("DestructablePrefab")) as GameObject;
        return obj.GetComponent<DestructableObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
