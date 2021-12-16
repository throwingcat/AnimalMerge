using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideLine : MonoBehaviour
{
    public SpriteRenderer line;
    public void UpdatePosition(Vector3 from, Vector3 to)
    {
        var center = (from + to) / 2f;
        line.transform.position = new Vector3(to.x,center.y,to.z);
        line.size = new Vector2(
            line.size.x,
            Vector3.Distance(from,to));        
    }
}
