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
        public SpriteRenderer graphic;
        private int team;

        [Header("Move Attribute")]
        public float speed;
        public float jumpForce;
        public Transform groundChecker;
        public LayerMask groundLayer;
        private Rigidbody2D playerRb;
        private int inputHorizontal;

        [Header("Serve Attribute")]
        public float servePowerMultiplier = 100f;
        private float servePower;

        private PlayerState state = PlayerState.MOVE;
        private PhysicsScene2D physicsScene;

        private void Start()
        {
            playerRb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (isLocalPlayer)
            {
                InputPlayer();
                CmdInputHorizontal(inputHorizontal);
            }
        }

        private void FixedUpdate()
        {
            if (isServer)
            {
                Move(inputHorizontal);
            }
        }

        [Server]
        public void InitializeDataServerPlayer(GameManager gameManager, int team)
        {
            this.team = team;

            SetGraphic(team == 1);

            this.gameManager = gameManager;
            if (isServer)
            {
                physicsScene = gameObject.scene.GetPhysicsScene2D();
            }
            CmdInitializeDataClientPlayer(this.gameManager, this.team);
        }

        [ClientRpc]
        private void SetGraphic(bool isFlip)
        {
            graphic.flipX = isFlip;
        }

        [TargetRpc]
        private void CmdInitializeDataClientPlayer(GameManager gameManager, int team)
        {
            this.team = team;
            this.gameManager = gameManager;
            if (isLocalPlayer)
            {
                gameManager.gameUI.serveButton.onReleased.AddListener(() => CmdServe(servePower));
                gameManager.gameUI.jumpButton.onPressed.AddListener(() => CmdJump());
            }
        }

        private void InputPlayer()
        {
            if (state == PlayerState.MOVE)
            {
                if (gameManager.gameUI.leftButton.IsPressed)
                {
                    inputHorizontal = -1;
                }
                else if (gameManager.gameUI.rightButton.IsPressed)
                {
                    inputHorizontal = 1;
                }
                else
                {
                    inputHorizontal = 0;
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
        private void CmdInputHorizontal(int currentInput)
        {
            inputHorizontal = currentInput;
        }

        #region Player Move

        public void StartMove()
        {
            state = PlayerState.MOVE;
            inputHorizontal = 0;
            servePower = 0;
        }

        [TargetRpc]
        public void StartMoveRpc()
        {
            StartMove();
            gameManager.gameUI.SetMoveUI();
        }

        private void Move(int direction)
        {
            if (state != PlayerState.MOVE) return;

            playerRb.velocity = new Vector2(speed * direction * Time.fixedDeltaTime * 10, playerRb.velocity.y);
        }

        #endregion

        #region Jump

        [Command]
        private void CmdJump()
        {
            Jump();
        }

        private void Jump()
        {
            if (GroundCheck() && state == PlayerState.MOVE)
            {
                playerRb.AddForce(new Vector2(0, jumpForce * 10));
            }
        }

        #endregion

        #region Serve
        public void StartServe()
        {
            state = PlayerState.SERVE;
            inputHorizontal = 0;
            servePower = 0;
        }

        [TargetRpc]
        public void StartServeRpc()
        {
            StartServe();
            gameManager.gameUI.SetServeUI();
        }

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
        }

        [Command]
        private void CmdServe(float servePower)
        {
            Serve(servePower);
            StartMove();
            StartMoveRpc();
        }

        private void Serve(float power)
        {
            float finalPower = team == 0 ? power : -power;
            gameManager.ball.GetComponent<Ball>().ServeBall(finalPower * servePowerMultiplier);
        }
        #endregion


        private bool GroundCheck()
        {
            return (physicsScene.OverlapPoint(groundChecker.position, groundLayer));
        }

        public int GetTeam() { return team; }
    }
}