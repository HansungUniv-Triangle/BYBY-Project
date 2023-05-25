using UnityEngine;

namespace Network
{
    public class ReallySlowBullet : NetworkProjectileBase
    {
        protected override bool IsExpirationProjectile()
        {
            if (IsHit)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void UpdateProjectile()
        {
            _rigidbody.velocity = transform.forward * TotalVelocity * 0.1f;
        }

        protected override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
            
            var objectLayer = other.gameObject.layer;
            if (objectLayer.Equals(LayerMask.NameToLayer("Enemy")))
            {
                if (Object.HasStateAuthority)
                {
                    GameManager.Instance.NetworkManager.AddCharacterHitData(Object, (int)Damage, _projectileHolder.WeaponData.isMainWeapon);
                    IsHit = true;
                }
            }
        }
    }
}