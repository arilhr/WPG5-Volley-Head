using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VollyHead.Offline
{
    public class Ball : MonoBehaviour
    {
        public BoxCollider2D midBoundary;
        public float maxSpeed;
        public string onAreaTeam;

        private bool isPlayed = true;
        private Rigidbody2D ballRb;

        private int lastTeamTouchBall;
        private int latestTeamTouchCount;

        private Player latestPlayerTouchBall = null;

        // Start is called before the first frame update
        void Start()
        {
            CircleCollider2D collider = GetComponent<CircleCollider2D>();
            ballRb = GetComponent<Rigidbody2D>();

            Physics2D.IgnoreCollision(collider, midBoundary);
        }

        private void FixedUpdate()
        {
            CheckMaxSpeed();
        }

        private void CheckMaxSpeed()
        {
            if (ballRb.velocity.magnitude > maxSpeed)
            {
                ballRb.velocity = Vector3.ClampMagnitude(ballRb.velocity, maxSpeed);
            }
        }

        private IEnumerator WaitToNewRound(int scoredTeam)
        {
            isPlayed = false;
            lastTeamTouchBall = -1;
            latestTeamTouchCount = 0;
            latestPlayerTouchBall = null;
            GameManager.instance.AddScore(scoredTeam);

            yield return new WaitForSeconds(GameManager.instance.timeToNewRound);

            GameManager.instance.StartNewRound(scoredTeam);
            ballRb.velocity = Vector3.zero;
            isPlayed = true;
        }

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
                            int scoredTeam;
                            if (collideTeamID == 0) scoredTeam = 1;
                            else scoredTeam = 0;

                            StartCoroutine(WaitToNewRound(scoredTeam));
                            return;
                        }
                        else
                        {
                            
                        }
                    }
                    else
                    {
                        lastTeamTouchBall = collideTeamID;
                        latestTeamTouchCount = 1;
                    }

                    if (latestPlayerTouchBall == playerCollided)
                    {
                        int scoredTeam;
                        if (collideTeamID == 0) scoredTeam = 1;
                        else scoredTeam = 0;

                        // add score to enemy
                        StartCoroutine(WaitToNewRound(scoredTeam));
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
                        StartCoroutine(WaitToNewRound(1));
                    }
                    else if (onAreaTeam == "Area2")
                    {
                        Debug.Log($"Team 1 has scored..");
                        StartCoroutine(WaitToNewRound(0));
                    }
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag != onAreaTeam)
            {
                lastTeamTouchBall = -1;
                latestTeamTouchCount = 0;
                latestPlayerTouchBall = null;
            }
        }

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

