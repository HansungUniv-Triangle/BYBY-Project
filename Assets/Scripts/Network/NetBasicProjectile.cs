﻿using System;
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

        private void OnCollisionEnter(Collision collision)
        {
            if(IsHit || !HasStateAuthority) return;
            
            var objectLayer = collision.collider.gameObject.layer;

            if (objectLayer.Equals(LayerMask.NameToLayer("World")))
            {
                var hit = collision.contacts[0];
                var point = hit.point - hit.normal * 0.01f;

                point.x = (float)Math.Round(point.x, 3);
                point.y = (float)Math.Round(point.y, 3);
                point.z = (float)Math.Round(point.z, 3);

                WorldManager.Instance.GetWorld().HitBlock(point, 1);
                GameManager.Instance.NetworkManager.AddBlockHitData(point, 1);
                IsHit = true;
            }
            else if (objectLayer.Equals(LayerMask.NameToLayer("Enemy")))
            {
                GameManager.Instance.NetworkManager.AddCharacterHitData(Object);
                IsHit = true;
                IsEnemyHit = true;
            }
        }
    }
}