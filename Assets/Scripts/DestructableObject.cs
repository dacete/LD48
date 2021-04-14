using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class DestructableObject : MonoBehaviour
{
    public new SpriteRenderer renderer;
    public new Rigidbody2D rigidbody;
    public new PolygonCollider2D collider;
    public int size;
    public Texture2D texture;
    public DestructionManager manager;
    public bool loadTexture;
    public JobHandle handle;
    // Start is called before the first frame update
    void Start()
    {
        if (manager == null)
        {
            manager = FindObjectOfType<DestructionManager>();
        }
        if (loadTexture)
        {
            SetTexture(Instantiate(Resources.Load("Texture")) as Texture2D);
        }
        manager.objects.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetCollision(NativeList<float2> points)
    {
        //collider.points = new Vector2[points.Length];
        collider.SetPath(0, points.AsArray().Reinterpret<Vector2>().ToArray());
        //points.AsArray().Reinterpret<Vector2>().CopyTo(collider.points);
    }
    private void OnDestroy()
    {
        manager.objects.Remove(this);
    }
    public void SetTexture(Texture2D tex)
    {
        size = tex.width;
        renderer.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.0f, 0.0f), Constants.pixelsPerUnit, 0, SpriteMeshType.FullRect);
    }


}
