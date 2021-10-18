using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace VollyHead.Online
{
    public class Player : NetworkBehaviour
    {
        public enum PlayerState
        {
            MOVE,
            SERVE
        }

        private GameManager gameManager;
        private int team;

        [Header("Move Attribute")]
        public float speed;
        public float jumpForce;
        public LayerMask groundLayer;
        private Rigidbody2D playerRb;
        private float inputHorizontal;

        [Header("Serve Attribute")]
        public float servePowerMultiplier = 100f;
        private float servePower;

        private PlayerState state = PlayerState.MOVE;

        private void Start()
        {
            playerRb = GetComponent<Rigidbody2D>();
            
        }

        private void Update()
        {
            if (isLocalPlayer)
            {
                InputPlayer();
            }
        }

        private void FixedUpdate()
        {
            if (isLocalPlayer)
            {
                if (state == PlayerState.MOVE)
                {
                    CmdMove(inputHorizontal);
                }
            }
                
        }

        [Server]
        public void InitializeDataServerPlayer(GameManager gameManager, int team)
        {
            this.team = team;
            this.gameManager = gameManager;
            CmdInitializeDataClientPlayer(this.gameManager, this.team);
        }

        [TargetRpc]
        private void CmdInitializeDataClientPlayer(GameManager gameManager, int team)
        {
            this.team = team;
            this.gameManager = gameManager;
            if (isLocalPlayer)
                gameManager.gameUI.serveButton.onReleased.AddListener(() => Serve());
        }

        [Client]
        private void InputPlayer()
        {
            if (state == PlayerState.MOVE)
            {
                inputHorizontal = Input.GetAxisRaw("Horizontal");
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    CmdJump();
                }
            }
            else if (state == PlayerState.SERVE)
            {
                if (gameManager.gameUI.serveButton.IsPressed)
                {
                    IncreasingServePower();
                }
            }
        }
        
        [Command]
        private void CmdMove(float direction)
        {
            playerRb.velocity = new Vector2(speed * direction * Time.fixedDeltaTime * 10, playerRb.velocity.y);
        }

        [Command]
        private void CmdJump()
        {
            if (GroundCheck())
            {
                playerRb.AddForce(new Vector2(0, jumpForce * 10));
            }
        }

        [TargetRpc]
        public void StartServe()
        {
            // reset value
            servePower = 0;
            state = PlayerState.SERVE;
            gameManager.gameUI.SetServeUI();
            Debug.Log($"Player Serve...");
            // servePowerBar.value = 0;
        }

        [Client]
        private void IncreasingServePower()
        {
            if (servePower >= 1)
            {
                servePower = 1;
            }
            else
            {
                servePower += Time.deltaTime;
            }

            gameManager.gameUI.SetServePowerUI(servePower);
            CmdIncreaseServePower(servePower);
        }

        [Command]
        private void CmdIncreaseServePower(float power)
        {
            servePower = power;
        }

        [Client]
        private void Serve()
        {
            CmdServe();
        }

        [Command]
        private void CmdServe()
        {
            float finalPower = team == 0 ? servePower : -servePower;
            gameManager.ball.GetComponent<Ball>().ServeBall(finalPower * servePowerMultiplier);
            EndServeRpc();
        }

        [TargetRpc]
        private void EndServeRpc()
        {
            state = PlayerState.MOVE;
            gameManager.gameUI.SetMoveUI();
        }

        

        [TargetRpc]
        public void StartMove()
        {
            // reset value
            servePower = 0;
            // servePowerBar.value = 0;

            gameManager.gameUI.SetMoveUI();
        }

        private bool GroundCheck()
        {
            float offsetHeight = 1f;
            Collider2D collider = GetComponentInChildren<Collider2D>();
            RaycastHit2D raycastHit = Physics2D.BoxCast(collider.bounds.center, collider.bounds.size, 0f, Vector2.down, offsetHeight, groundLayer);

            return raycastHit.collider != null;
        }

        public int GetTeam() { return team; }
    }
}