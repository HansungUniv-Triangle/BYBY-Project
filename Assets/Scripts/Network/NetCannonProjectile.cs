﻿using System;
using Fusion;
using TMPro;
using UnityEngine;
using Types;

namespace Network
{
    public class NetCannonProjectile : NetworkProjectileBase
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

            if (objectLayer.Equals(LayerMask.NameToLayer("World")) || objectLayer.Equals(LayerMask.NameToLayer("Enemy")))
            {
                var hit = collision.contacts[0];
                var point = hit.point - hit.normal * 0.01f;
                var special = (int)_baseStat(WeaponStat.Special);

                point.x = (float)Math.Round(point.x, 3);
                point.y = (float)Math.Round(point.y, 3);
                point.z = (float)Math.Round(point.z, 3);

                WorldManager.Instance.GetWorld().ExplodeBlocks(point, special, 1);
                GameManager.Instance.NetworkManager.AddBlockHitData(point, special, 1);
                
                Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, special, (int)Layer.Enemy);
                if (hitColliders.Length > 0)
                {
                    GameManager.Instance.NetworkManager.AddCharacterHitData(Object, (int)Damage);
                }

                IsHit = true;
            }
        }
    }
}