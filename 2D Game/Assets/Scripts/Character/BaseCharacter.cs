using UnityEngine;

namespace Game.Characters
{
    public abstract class BaseCharacter : MonoBehaviour
    {
        public bool IsFacingLeft { get; set; }

        public bool IsGrounded { get; set; }

        protected Collider2D Collider { get; set; }

        protected Rigidbody2D Rigidbody { get; set; }

        private Vector2 facingLeft;

        protected virtual void Initialize()
        {
            Collider = GetComponent<Collider2D>();
            Rigidbody = GetComponent<Rigidbody2D>();
            facingLeft = GetLeftDirection();
        }

        protected virtual bool IsColliding(Vector2 direction, float distance, LayerMask collision)
        {
            var hits = new RaycastHit2D[10];
            int hitsCount = Collider.Cast(direction, hits, distance);
            for (int i = 0; i < hitsCount; i++)
                if ((1 << hits[i].collider.gameObject.layer & collision) != 0)
                    return true;

            return false;
        }

        protected virtual void Flip()
        {
            if (IsFacingLeft)
            {
                transform.localScale = facingLeft;
                return;
            }

            transform.localScale = GetLeftDirection();
        }

        protected virtual bool IsFalling(float velocity)
            => !IsGrounded && Rigidbody.velocity.y < velocity;

        private Vector2 GetLeftDirection()
            => new Vector2(-transform.localScale.x, transform.localScale.y);
    }
}
