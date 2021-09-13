using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed;
    public float jumpForce;
    [SerializeField] private LayerMask layerMask;

    private Rigidbody2D playerRb;
    private float horizontalAxis;

    private void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        InputPlayer();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void InputPlayer()
    {
        horizontalAxis = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    private void Move()
    {
        playerRb.velocity = new Vector2(speed * horizontalAxis * Time.fixedDeltaTime * 10, playerRb.velocity.y);
    }

    private void Jump()
    {
        if (GroundCheck() && Input.GetKeyDown(KeyCode.Space))
        {
            playerRb.AddForce(new Vector2(0, jumpForce * 10));
        }
    }

    private bool GroundCheck()
    {
        float offsetHeight = 1f;
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        RaycastHit2D raycastHit = Physics2D.BoxCast(collider.bounds.center, collider.bounds.size, 0f, Vector2.down, offsetHeight, layerMask);

        return raycastHit.collider != null;
    }
}
