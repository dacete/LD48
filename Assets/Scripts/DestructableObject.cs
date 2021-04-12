using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructableObject : MonoBehaviour
{
    public new SpriteRenderer renderer;
    public new Rigidbody2D rigidbody;
    public new PolygonCollider2D collider;
    public int size;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetTexture(Texture2D tex)
    {
        size = tex.width;
        print(tex.width);
        renderer.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.0f, 0.0f), Constants.pixelsPerUnit, 0, SpriteMeshType.FullRect);
        print("Set texture: " + name);
    }


}
