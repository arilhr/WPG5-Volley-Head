using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace VollyHead.Online
{
    public class Ball : NetworkBehaviour
    {
        public float maxSpeed;
        private string onAreaTeam;

        private bool isPlayed = true;
        public Rigidbody2D ballRb;

        private float defaultGravityScale;

        private int lastTeamTouchBall;
        private int latestTeamTouchCount;

        private Player latestPlayerTouchBall = null;

        void Start()
        {
            CircleCollider2D collider = GetComponent<CircleCollider2D>();
            ballRb = GetComponent<Rigidbody2D>();
            defaultGravityScale = ballRb.gravityScale;

            Physics2D.IgnoreCollision(collider, GameManager.instance.midBoundary);
        }

        private void FixedUpdate()
        {
            CheckMaxSpeed();
        }

        public void StartNewRound()
        {
            isPlayed = true;
        }

        public void ServeMode()
        {
            isPlayed = true;
            ballRb.gravityScale = 0;
            ballRb.velocity = Vector2.zero;
        }

        public void CmdServeBall(float power)
        {
            ballRb.gravityScale = 0.8f;
            ballRb.AddForce(new Vector2(10f * power,  6f * Mathf.Abs(power)));
        }

        private void Scored(int scoredTeam)
        {
            isPlayed = false;
            ResetBallData();
            GameManager.instance.Scored(scoredTeam);
        }

        public void ResetBallData()
        {
            lastTeamTouchBall = -1;
            latestTeamTouchCount = 0;
            latestPlayerTouchBall = null;
        }

        // limit the velocity of ball
        private void CheckMaxSpeed()
        {
            if (ballRb.velocity.magnitude > maxSpeed)
            {
                ballRb.velocity = Vector3.ClampMagnitude(ballRb.velocity, maxSpeed);
            }
        }

        [ServerCallback]
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.GetComponent<Player>() != null)
            {
                if (isPlayed)
                {
                    Player playerCollided = collision.gameObject.GetComponent<Player>();
                    int collideTeamID = playerCollided.GetTeam();

                    if (lastTeamTouchBall == collideTeamID)
                    {
                        latestTeamTouchCount++;
                        if (latestTeamTouchCount > 3)
                        {
                            // add score to enemy
                            int scoredTeam = collideTeamID == 0 ? 1 : 0;

                            Scored(scoredTeam);
                            return;
                        }
                    }
                    else
                    {
                        lastTeamTouchBall = collideTeamID;
                        latestTeamTouchCount = 1;
                    }

                    if (latestPlayerTouchBall == playerCollided)
                    {
                        int scoredTeam = collideTeamID == 0 ? 1 : 0;

                        // add score to enemy
                        Scored(scoredTeam);
                        return;
                    }
                    else
                    {
                        latestPlayerTouchBall = playerCollided;
                    }
                }
            }

            if (collision.gameObject.tag == "Ground")
            {
                if (isPlayed)
                {
                    if (onAreaTeam == "Area1")
                    {
                        Debug.Log($"Team 2 has scored..");
                        Scored(1);
                    }
                    else if (onAreaTeam == "Area2")
                    {
                        Debug.Log($"Team 1 has scored..");
                        Scored(0);
                    }
                }
            }
        }

        [ServerCallback]
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag != onAreaTeam)
            {
                ResetBallData();
            }
        }

        [ServerCallback]
        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Area1")
            {
                onAreaTeam = collision.gameObject.tag;
            }
            else if (collision.gameObject.tag == "Area2")
            {
                onAreaTeam = collision.gameObject.tag;
            }
        }
    }
}