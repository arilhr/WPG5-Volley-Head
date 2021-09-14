using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public BoxCollider2D midBoundary;
    public float maxSpeed;
    public int onAreaTeam = 0;

    private bool isPlayed = true;
    private Rigidbody2D ballRb;

    private int lastTeamTouchBall;
    private int latestTeamTouchCount;

    private Player latestPlayerTouchBall = null;
    private int latestPlayerTouchCount;

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
        latestPlayerTouchCount = 0;
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
                    if (latestTeamTouchCount >= 3)
                    {
                        // add score to enemy
                        int scoredTeam;
                        if (collideTeamID == 0) scoredTeam = 1;
                        else scoredTeam = 0;

                        StartCoroutine(WaitToNewRound(scoredTeam));
                    }
                    else
                    {
                        if (latestPlayerTouchBall == playerCollided)
                        {
                            latestPlayerTouchCount++;
                            if (latestPlayerTouchCount >= 2)
                            {
                                // add score to enemy

                            }
                        }
                        else
                        {
                            latestPlayerTouchBall = playerCollided;
                            latestPlayerTouchCount = 1;
                        }
                    }
                }
                else
                {
                    lastTeamTouchBall = collideTeamID;
                    latestTeamTouchCount = 1;
                }
            }
        }
        
        if (collision.gameObject.tag == "Ground")
        {
            if (isPlayed)
            {
                if (onAreaTeam == 0)
                {
                    Debug.Log($"Team 2 has scored..");
                    StartCoroutine(WaitToNewRound(1));
                }
                else if (onAreaTeam == 1)
                {
                    Debug.Log($"Team 1 has scored..");
                    StartCoroutine(WaitToNewRound(0));
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Area1")
        {
            onAreaTeam = 0;
        }
        else if (collision.gameObject.tag == "Area2")
        {
            onAreaTeam = 1;
        }
    }

}
