using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public BoxCollider2D midBoundary;

    // Start is called before the first frame update
    void Start()
    {
        CircleCollider2D collider = GetComponent<CircleCollider2D>();

        Physics2D.IgnoreCollision(collider, midBoundary);
    }

}
