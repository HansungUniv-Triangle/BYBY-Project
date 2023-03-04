using UnityEngine;

namespace Weapon
{
    public class Shield : ProjectileBase<Shield>
    {
        private bool _isTouched = false;
        
        protected override bool CheckDestroy()
        {
            return Distance > MaxRange || _isTouched;
        }

        protected override void OnCollisionEnter(Collision collision)
        {
            if(collision.gameObject.CompareTag("Bullet")) _isTouched = true;
        }

        protected override void MoveProjectile()
        {
            transform.Rotate(Vector3.up, TotalVelocity * Time.deltaTime);
            AddBulletSize(0.01f);
        }
    }
}