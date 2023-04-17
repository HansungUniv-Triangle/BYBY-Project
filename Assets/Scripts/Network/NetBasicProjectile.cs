using System;
using Fusion;
using TMPro;
using UnityEngine;
using Types;

namespace Network
{
    public class NetBasicProjectile : NetworkProjectileBase
    {
        protected override bool IsExpirationProjectile()
        {
            if (IsHit) return true;
            return Distance > MaxRange;
        }

        protected override void UpdateProjectile()
        {
            transform.position += gameObject.transform.forward * (TotalVelocity * Runner.DeltaTime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(IsHit) return;
            var objectLayer = collision.collider.gameObject.layer;

            if (objectLayer.Equals(LayerMask.NameToLayer("World")))
            {
                var hit = collision.contacts[0];
                var point = hit.point - hit.normal * 0.01f;

                point.x = (float)Math.Round(point.x, 3);
                point.y = (float)Math.Round(point.y, 3);
                point.z = (float)Math.Round(point.z, 3);

                WorldManager.Instance.GetWorld().HitBlock(point, 1);
                GameManager.Instance.NetworkManager.AddHitData(Runner.LocalPlayer, point, 1);
                IsHit = true;
            }
            else if (objectLayer.Equals(LayerMask.NameToLayer("Player")))
            {
                if (!Object.HasStateAuthority)
                {
                    GetNetworkPlayer(collision.gameObject).NetworkOnHit(Object);
                    RPCHitPlayer();
                }
            }
        }
    }
}