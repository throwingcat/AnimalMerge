using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBase : MonoBehaviour
{
    public SpriteRenderer Texture;
    public Rigidbody2D Rigidbody2D;
    public CircleCollider2D CircleCollider2D;

    public void OnSpawn(Vector3 position)
    {
        transform.localPosition = position;
        Rigidbody2D.gravityScale = 0;
        CircleCollider2D.enabled = false;
    }

    public void Drop()
    {
        Rigidbody2D.gravityScale = 1;
        CircleCollider2D.enabled = true;
    }
}
