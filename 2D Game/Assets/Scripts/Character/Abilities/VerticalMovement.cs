using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Characters.Abilities
{
    public class VerticalMovement : BaseAbility
    {
        [SerializeField]
        private bool isAirJumpingLimited;
        protected bool IsAirJumpingLimited
        {
            get => isAirJumpingLimited;
            private set => isAirJumpingLimited = value;
        }

        [SerializeField]
        private float jumpForce;
        protected float JumpForce
        {
            get => jumpForce;
            private set => jumpForce = value;
        }

        [SerializeField]
        private float holdForce;
        protected float HoldForce
        {
            get => holdForce;
            private set => holdForce = value;
        }

        [SerializeField]
        private float jumpButtonHoldingTime;
        protected float JumpButtonHoldingTime
        {
            get => jumpButtonHoldingTime;
            private set => jumpButtonHoldingTime = value;
        }

        [SerializeField]
        private int maxJumps;
        protected int MaxJumps
        {
            get => maxJumps;
            private set => maxJumps = value;
        }

        [SerializeField]
        private int maxJumpSpeed;
        protected int MaxJumpSpeed
        {
            get => maxJumpSpeed;
            private set => maxJumpSpeed = value;
        }

        [SerializeField]
        private int maxFallSpeed;
        protected int MaxFallSpeed
        {
            get => maxFallSpeed;
            private set => maxFallSpeed = value;
        }

        [SerializeField]
        private int acceptedFallSpeed;
        protected int AcceptedFallSpeed
        {
            get => acceptedFallSpeed;
            private set => acceptedFallSpeed = value;
        }

        [SerializeField]
        private float distanceToCollider;
        protected float DistanceToCollider
        {
            get => distanceToCollider;
            private set => distanceToCollider = value;
        }

        [SerializeField]
        private LayerMask collisionLayer;
        protected LayerMask CollisionLayer
        {
            get => collisionLayer;
            private set => collisionLayer = value;
        }

        private bool isJumping;
        private float jumpCountDown;
        private int jumpsLeft;


        protected override void Initialize()
        {
            base.Initialize();
            jumpsLeft = MaxJumps;
            jumpCountDown = JumpButtonHoldingTime;
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            JumpPressed();
            JumpHeld();
        }

        protected virtual private void FixedUpdate()
        {
            Jump();
            GroundCheck();
        }

        protected virtual bool JumpPressed()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!Character.IsGrounded && jumpsLeft == MaxJumps)
                {
                    isJumping = false;
                    return false;
                }

                if (IsAirJumpingLimited && IsFalling(AcceptedFallSpeed))
                {
                    isJumping = false;
                    return false;
                }

                jumpsLeft--;

                if (jumpsLeft >= 0)
                {
                    jumpCountDown = JumpButtonHoldingTime;
                    isJumping = true;
                }

                return true;
            }

            return false;
        }

        protected virtual bool JumpHeld()
            => Input.GetKey(KeyCode.Space);

        protected virtual void Jump()
        {
            if (isJumping)
            {
                Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, 0);
                Rigidbody.AddForce(Vector2.up * JumpForce);
                AddJumpForce();
            }

            if (Rigidbody.velocity.y > MaxJumpSpeed)
            {
                Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, MaxJumpSpeed);
            }
        }

        protected virtual void AddJumpForce()
        {
            if (JumpHeld())
            {
                jumpCountDown -= Time.deltaTime;
                if (jumpCountDown <= 0)
                {
                    jumpCountDown = 0;
                    isJumping = false;
                }
                else
                {
                    Rigidbody.AddForce(Vector2.up * HoldForce);
                }
            }
            else
            {
                isJumping = false;
            }
        }

        protected virtual void GroundCheck()
        {
            bool isColliding = IsColliding(Vector2.down, distanceToCollider, CollisionLayer);
            Character.IsGrounded = isColliding;

            if (isColliding && !isJumping)
            {
                jumpsLeft = MaxJumps;
            }
            else
            {
                if (IsFalling(0) && Rigidbody.velocity.y < MaxFallSpeed)
                {
                    Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, MaxFallSpeed);
                }
            }
        }
    }
}
