using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideLine : MonoBehaviour
{
    public SpriteRenderer line;
    public void UpdatePosition(Vector3 from, Vector3 to)
    {
        line.transform.position = new Vector3(to.x,from.y,to.z + 0.1f);
        line.size = new Vector2(line.size.x,
            Vector3.Distance(from,to));        
    }
}
