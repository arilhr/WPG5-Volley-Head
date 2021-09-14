using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int teamID;
    public float speed;
    public float jumpForce;
    public Transform serviceArea;
    public GameObject serviceBallPos;
    [SerializeField] private LayerMask layerMask;

    public KeyCode rightButton;
    public KeyCode leftButton;
    public KeyCode jumpButton;

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
        if (Input.GetKey(rightButton))
        {
            horizontalAxis = 1;
        }
        else if (Input.GetKey(leftButton))
        {
            horizontalAxis = -1;
        }
        else
        {
            horizontalAxis = 0;
        }

        if (Input.GetKeyDown(jumpButton))
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
        if (GroundCheck())
        {
            playerRb.AddForce(new Vector2(0, jumpForce * 10));
        }
    }

    private bool GroundCheck()
    {
        float offsetHeight = 1f;
        Collider2D collider = GetComponent<Collider2D>();
        RaycastHit2D raycastHit = Physics2D.BoxCast(collider.bounds.center, collider.bounds.size, 0f, Vector2.down, offsetHeight, layerMask);

        return raycastHit.collider != null;
    }

    public int GetTeam()
    {
        return teamID;
    }
}
