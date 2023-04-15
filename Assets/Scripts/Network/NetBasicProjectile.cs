using System;
using Fusion;
using UnityEngine;
using Types;

namespace Network
{
    public class NetBasicProjectile : NetworkProjectileBase
    {
        private bool _isHit;
        
        protected override bool IsExpirationProjectile()
        {
            if (_isHit) return true;
            return Distance > MaxRange;
        }

        protected override void UpdateProjectile()
        {
            transform.position += gameObject.transform.forward * (TotalVelocity * Runner.DeltaTime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(_isHit) return;
            if (collision.collider.gameObject.layer.Equals(LayerMask.NameToLayer("World")))
            {
                var hit = collision.contacts[0];
                var point = hit.point - hit.normal * 0.01f;
                WorldManager.Instance.GetWorld().HitBlock(point, 1);
                GameManager.Instance.NetworkManager.SendHitData(point, 1);
                _isHit = true;
            }
            else if (collision.collider.gameObject.layer.Equals(LayerMask.NameToLayer("Enemy")))
            {
                Debug.Log("적을 맞춤");
                _isHit = true;
            }
        }
    }
}