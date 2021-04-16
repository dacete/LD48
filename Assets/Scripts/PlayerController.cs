using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public new CapsuleCollider2D collider;
    public float jumpPower;
    public ContactFilter2D filter;
    public float moveSpeed;
    public float gravityMult;
    Collider2D[] results;
    ContactPoint2D[] points;
    public Transform circle;
    public Vector2 velocity;
    // Start is called before the first frame update
    void Start()
    {
        results = new Collider2D[10];
        points = new ContactPoint2D[10];
    }

    // Update is called once per frame
    void Update()
    {

        velocity.y -= 9.81f * gravityMult * Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = jumpPower;
        }
        velocity.x = Input.GetAxisRaw("Horizontal") * moveSpeed;

        transform.Translate(new Vector3(velocity.x, velocity.y,0) * Time.deltaTime);
        //var hits = Physics2D.CapsuleCastAll(transform.position, collider.size, collider.direction, 0, Vector3.down, 0, filter.layerMask);
        var count = Physics2D.OverlapCapsuleNonAlloc(transform.position, collider.size, collider.direction, 0,results,filter.layerMask);
        if(count > 0)
        {
            print("detected");
            for (int i = 0; i < count; i++)
            {
                var col = results[i];
                if(col == collider)
                {
                    continue;
                }

                var dist = col.Distance(collider);
                var hitVel = dist.normal* math.dot(dist.pointA - dist.pointB, velocity);
                velocity -= hitVel;
                if (dist.isOverlapped)
                {
                    transform.Translate(dist.pointA - dist.pointB);
                }
            }
        }
    }
}
