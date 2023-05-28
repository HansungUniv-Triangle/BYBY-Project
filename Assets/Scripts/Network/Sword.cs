using System;
using Fusion;
using UnityEngine;

namespace Network
{
    public class Sword : NetworkProjectileBase
    {
        protected override bool IsExpirationProjectile()
        {
            if (IsHit) return true;
            return transform.position.y < -50;
        }

        protected override void UpdateProjectile()
        {
            _rigidbody.velocity = new Vector3(0, -30f, 0);
            DamageSave = Damage * 2;
        }

        protected override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
            
            var objectLayer = other.gameObject.layer;
            if (objectLayer.Equals(LayerMask.NameToLayer("Enemy")))
            {
                GameManager.Instance.NetworkManager.AddCharacterHitData(Object, (int)DamageSave, _projectileHolder.WeaponData.isMainWeapon);
                IsHit = true;
            }
            else if (objectLayer.Equals(LayerMask.NameToLayer("World")))
            {
                IsHit = true;
            }
        }
    }
}