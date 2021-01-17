using UnityEngine;

namespace Game.Characters.Abilities
{
    public class HorizontalMovement : BaseAbility
    {
        [SerializeField]
        private float timeTillMaxSpeed;
        protected float TimeTillMaxSpeed
        {
            get => timeTillMaxSpeed;
            private set => timeTillMaxSpeed = value;
        }

        [SerializeField]
        private float maxSpeed;
        protected float MaxSpeed
        {
            get => maxSpeed;
            private set => maxSpeed = value;
        }

        [SerializeField]
        private float sprintMultiplier;
        protected float SprintMultiplier
        {
            get => sprintMultiplier;
            private set => sprintMultiplier = value;
        }

        private float acceleration;
        private float currentSpeed;
        private float horizontalInput;
        private float runningDuration;

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected virtual void Update()
        {
            MovementPressed();
            SprintingHeld();
        }

        protected virtual void FixedUpdate()
        {
            Move();
        }

        protected virtual bool SprintingHeld()
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                return true;
            }

            return false;
        }

        protected virtual bool MovementPressed()
        {
            if (Input.GetAxis("Horizontal") != 0)
            {
                horizontalInput = Input.GetAxis("Horizontal");
                return true;
            }

            return false;
        }

        protected virtual void Move()
        {
            if (MovementPressed())
            {
                acceleration = MaxSpeed / TimeTillMaxSpeed;
                runningDuration += Time.deltaTime;
                currentSpeed = horizontalInput * acceleration * runningDuration;
                CheckDirection();
            }
            else
            {
                acceleration = 0;
                runningDuration = 0;
                currentSpeed = 0;
            }

            ModifySpeed();

            Rigidbody.velocity = new Vector2(currentSpeed, Rigidbody.velocity.y);
        }

        protected virtual void CheckDirection()
        {
            if (currentSpeed == 0)
            {
                return;
            }

            bool isFacingLeft = Character.IsFacingLeft;

            if (currentSpeed > 0)
            {
                if (isFacingLeft)
                {
                    FlipDirection(isFacingLeft);
                }

                if (currentSpeed > MaxSpeed)
                {
                    currentSpeed = MaxSpeed;
                }
            }
            else
            {
                if (!isFacingLeft)
                {
                    FlipDirection(isFacingLeft);
                }

                if (currentSpeed < -MaxSpeed)
                {
                    currentSpeed = -MaxSpeed;
                }
            }
        }

        protected virtual void ModifySpeed()
        {
            if (SprintingHeld())
            {
                currentSpeed *= SprintMultiplier;
            }
        }

        private void FlipDirection(bool isFacingLeft)
        {
            Character.IsFacingLeft = !isFacingLeft;
            Flip();
        }
    }
}
