using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace VollyHead.Online
{
    public class Ball : NetworkBehaviour
    {
        public float maxSpeed;

        private Rigidbody2D ballRb;

        // Start is called before the first frame update
        void Start()
        {
            ballRb = GetComponent<Rigidbody2D>();
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
    }
}

