using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace VollyHead.Online
{
    public class Player : NetworkBehaviour
    {
        public int teamID;
        public float speed;
        public float jumpForce;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private LayerMask playerLayer;

        private Rigidbody2D playerRb;
        private float horizontalAxis;

        private void Start()
        {
            playerRb = GetComponent<Rigidbody2D>();
            
        }

        private void Update()
        {
            if (isLocalPlayer)
                InputPlayer();
        }

        private void FixedUpdate()
        {
            Move();
        }

        [Client]
        private void InputPlayer()
        {
            InputHorizontal(Input.GetAxisRaw("Horizontal"));

            if (Input.GetKeyDown(KeyCode.W))
            {
                Jump();
            }
        }

        [Command]
        public void InputHorizontal(float direction)
        {
            horizontalAxis = direction;
        }
        
        [ServerCallback]
        private void Move()
        {
            playerRb.velocity = new Vector2(speed * horizontalAxis * Time.fixedDeltaTime * 10, playerRb.velocity.y);
        }

        [Command]
        private void Jump()
        {
            if (GroundCheck())
            {
                playerRb.AddForce(new Vector2(0, jumpForce * 10));
            }
        }

        [ServerCallback]
        private bool GroundCheck()
        {
            float offsetHeight = 0f;
            Collider2D collider = GetComponent<Collider2D>();
            RaycastHit2D raycastHit = Physics2D.BoxCast(collider.bounds.center, collider.bounds.size, 0f, Vector2.down, offsetHeight, layerMask);
            Debug.Log(raycastHit.collider != null);

            return raycastHit.collider != null;
        }

        public int GetTeam()
        {
            return teamID;
        }
    }

}
